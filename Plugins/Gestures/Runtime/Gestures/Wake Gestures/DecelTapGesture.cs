using Leap;
using Leap.Interaction.Internal.InteractionEngineUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public class DecelTapGesture : BaseGesture
    {
        private List<Vector3> _oldPositions = new List<Vector3>();

        private float velocityCurrent, angularCurrent;
        [SerializeField] // Min max
        private Vector2 velocityMinMax, angularMinMax;
        [SerializeField]
        private float velocityMMLerpTime = 0.1f;

        private float velocityMMLerpCurrent = 0f;

        private Vector3 _oldPosition;
        private Quaternion _oldRotation;

        [SerializeField]
        private Vector3 euroA, euroABad, euroB, euroC;
        [SerializeField]
        private Vector3 offsetRotation = Vector3.zero;

        private List<OneEuroFilter<Vector3>> _oneEuroFilters = new List<OneEuroFilter<Vector3>>();

        private OneEuroFilter<Vector3> _oneEuroFilterPosition;
        private OneEuroFilter<Vector3> _oneEuroFilterVelocity;
        private OneEuroFilter<Vector3> _oneEuroFilterDeceleration;

        private bool _tap = false;
        private float _tapTime = 0f;

        [SerializeField]
        private bool _holdTap = false;

        private float _currentSmoothVel = 0f;

        private int _timer = 1;
        private int _timerA = 0, _timerB = 0;

        [SerializeField]
        private float _resetTimer = 0.2f;
        private float _resetTimerCurrent = 0f;

        [SerializeField]
        private float _thresholdDown, _thresholdUp;

        private void Awake()
        {

            for (int i = 0; i < 1; i++)
            {
                _oldPositions.Add(Vector3.zero);
                _oneEuroFilters.Add(new OneEuroFilter<Vector3>(euroA.x, euroA.y, euroA.z));
            }

            _oneEuroFilterPosition = new OneEuroFilter<Vector3>(euroA.x, euroA.y, euroA.z);
            _oneEuroFilterVelocity = new OneEuroFilter<Vector3>(euroB.x, euroB.y, euroB.z);
            _oneEuroFilterDeceleration = new OneEuroFilter<Vector3>(euroC.x, euroC.y, euroC.z);
        }

        protected override void StartTrackingFunction(Hand hand)
        {
            _resetTimerCurrent = _resetTimer;
        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            result = false;
            value = 0;

            if (_tap)
            {
                _tapTime += Time.deltaTime;
            }

            Vector3 velocity = PhysicsUtility.ToLinearVelocity(_oldPosition, hand.PalmPosition, Time.deltaTime);

            Vector3 angularVelocity = PhysicsUtility.ToAngularVelocity(_oldRotation, hand.Rotation, Time.deltaTime);

            velocityCurrent = velocity.magnitude;

            angularCurrent = angularVelocity.magnitude;

            velocityMMLerpCurrent = Mathf.Lerp(velocityMMLerpCurrent,
                Mathf.Max(Mathf.InverseLerp(velocityMinMax.x, velocityMinMax.y, velocity.magnitude), Mathf.InverseLerp(angularMinMax.x, angularMinMax.y, angularVelocity.magnitude)),
                Time.deltaTime * (1.0f / velocityMMLerpTime));

            Vector3 euroPos = Vector3.Lerp(euroA, euroABad, velocityMMLerpCurrent);

            _oneEuroFilterPosition.UpdateParams(euroPos.x, euroPos.y, euroPos.z);
            _oneEuroFilterPosition.Filter(Quaternion.Euler(offsetRotation) * ContactUtils.PointToHandLocal(hand, hand.Fingers[0].TipPosition));

            Debug.DrawRay(ContactUtils.HandPointToWorld(hand, Quaternion.Inverse(Quaternion.Euler(offsetRotation)) * _oneEuroFilterPosition.currValue), Vector3.up * 0.2f, Color.black, Time.deltaTime);

            Debug.DrawRay(hand.Fingers[0].bones[3].NextJoint, (_oneEuroFilterPosition.currValue - _oneEuroFilterPosition.prevValue) / Time.deltaTime, Color.cyan, Time.deltaTime);

            if (_resetTimerCurrent > 0f)
            {
                _resetTimerCurrent -= Time.deltaTime;
                return;
            }

            _oneEuroFilterVelocity.UpdateParams(euroB.x, euroB.y, euroB.z);
            _oneEuroFilterVelocity.Filter((_oneEuroFilterPosition.currValue - _oneEuroFilterPosition.prevValue) / Time.deltaTime);
            Debug.DrawRay(hand.Fingers[0].bones[3].NextJoint, _oneEuroFilterVelocity.currValue, Color.red, Time.deltaTime);

            _oneEuroFilterDeceleration.UpdateParams(euroC.x, euroC.y, euroC.z);
            _oneEuroFilterDeceleration.Filter((_oneEuroFilterVelocity.currValue - _oneEuroFilterVelocity.prevValue) / Time.deltaTime);

            Debug.DrawRay(hand.Fingers[0].bones[3].NextJoint, _oneEuroFilterDeceleration.currValue, Color.green, Time.deltaTime);
            //Debug.DrawRay(hand.Fingers[0].bones[3].NextJoint, _oneEuroFilterDeceleration.currValue.normalized, Color.cyan, Time.deltaTime);
            //Debug.DrawRay(hand.Fingers[0].bones[3].NextJoint, _oneEuroFilterVelocity.currValue.normalized, Color.magenta, Time.deltaTime);

            bool allowDown = Vector3.Dot(Quaternion.AngleAxis(-90, hand.PalmNormal) * hand.Direction, _oneEuroFilterDeceleration.currValue) > 0.15f;
            bool allowUp = Vector3.Dot(Quaternion.AngleAxis(90, hand.PalmNormal) * hand.Direction, _oneEuroFilterDeceleration.currValue) > 0.15f;

            _currentSmoothVel = _oneEuroFilterVelocity.currValue.magnitude;


            if (allowDown && _oneEuroFilterDeceleration.currValue.y < _thresholdDown)
            {
                _timerA++;
                _timerB = 0;
                if (_timerA > _timer)
                {
                    _tap = true;
                    _tapTime = 0f;
                }
            }
            else if (allowUp && _oneEuroFilterDeceleration.currValue.y > _thresholdUp)
            {
                _timerA = 0;
                _timerB++;
                if (_timerB > _timer)
                {
                    _tap = false;
                    _tapTime = 0f;
                }
            }
            else
            {
                _timerA = 0;
                _timerB = 0;
            }

            result = _tap;
            value = _tap ? 1 : (float)_timerA / _timer;

            _oldPosition = hand.PalmPosition;
            _oldRotation = hand.Rotation;
        }

        protected override void StopTrackingFunction()
        {

        }
    }
}