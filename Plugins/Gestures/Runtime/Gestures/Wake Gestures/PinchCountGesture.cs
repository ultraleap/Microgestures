using Leap;
using Leap.Unity;
using Leap.Unity.Preview.Locomotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    [RequireComponent(typeof(LightweightPinchDetector))]
    public class PinchCountGesture : BaseGesture
    {
        public float individualPinchTimer = 0.5f;
        public int pinchCounts = 2;
        [Tooltip("Holding the last pinch keeps the result as true until unpinched")]
        public bool holdingKeepsTrue = true;

        private bool _wasPinching = false;
        private float _currentTime = 0f;
        private int _currentCount = 0;

        [SerializeField]
        private LightweightPinchDetector _lightweightPinchDetector = null;

        protected override void StartTrackingFunction(Hand hand)
        {
            _lightweightPinchDetector.leapProvider = gestureMachine.leapProvider;
            _lightweightPinchDetector.chirality = hand.GetChirality();
        }

        protected override void StopTrackingFunction()
        {
            _currentTime = 0f;
        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            if (_currentCount == pinchCounts && holdingKeepsTrue && _lightweightPinchDetector.IsPinching)
            {
                result = true;
                value = 1;
                return;
            }

            _currentTime += Time.deltaTime;

            if (_currentTime >= individualPinchTimer)
            {
                _currentCount = 0;
            }

            if (_lightweightPinchDetector.IsPinching && !_wasPinching)
            {
                if (_currentTime < individualPinchTimer || _currentCount == 0)
                {
                    _currentCount++;
                }
                _currentTime = 0f;
                _wasPinching = true;
            }

            if (!_lightweightPinchDetector.IsPinching)
            {
                _wasPinching = false;
                if (_currentCount == pinchCounts)
                {
                    _currentCount = 0;
                }
            }

            result = _currentCount == pinchCounts;
            value = (float)_currentCount / pinchCounts;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            _lightweightPinchDetector = GetComponent<LightweightPinchDetector>();
        }
    }
}