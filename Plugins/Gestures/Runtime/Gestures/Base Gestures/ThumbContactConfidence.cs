using Leap;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace Leap.Unity.GestureMachine
{
    public class ThumbContactConfidence : BaseGesture
    {
        [Tooltip("The contact threshold. X = left, Y = right")]
        [SerializeField] private Vector2 _contactThreshold = new Vector2(0.001f, 0.01f);

        [SerializeField] private float _initContactDeltaThreshold = 0.02f;

        private Vector3 _closestPointIndex = Vector3.zero;
        private Vector3 _closestPointThumb = Vector3.zero;

        private float _startEndLength = 0.01f;

        private int _contactID = 0;
        private int _validatedContactID = 0;

        private float _prevValue = 0.0f;
        private bool _prevResult = false;
        private float _prevDistance = 0.0f;

        private float _value = 0.0f;
        private bool _result = false;
        private bool _isLeft = false;

        private bool _wasNullLastFrame = false;

        protected override void StartTrackingFunction(Hand hand)
        {
        }

        protected override void StopTrackingFunction()
        {
        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            value = _value;
            result = _result;
        }

        private void FixedUpdate()
        {
            Hand hand = _isLeft ? Hands.Left : Hands.Right;
            if (hand == null)
            {
                _wasNullLastFrame = true;
                return;
            }

            _value = CalculateValue(hand);
            float distance = Mathf.Abs(Vector3.Distance(_closestPointThumb, _closestPointIndex));
            float threshold = Mathf.Lerp(_contactThreshold.x, _contactThreshold.y, _value);
            _result = distance < threshold;

            if (_wasNullLastFrame)
            {
                _prevValue = _value;
                _prevResult = _result;
                _wasNullLastFrame = false;
            }

            if (_result && !_prevResult)
                _contactID++;

            if (_validatedContactID != _contactID)
            {
                _result = false;

                float scrubDelta = Mathf.Abs(_value - _prevValue);
                float contactDelta = Mathf.Abs(distance - _prevDistance);
                if (contactDelta <= _initContactDeltaThreshold)
                {
                    _validatedContactID = _contactID;
                    _result = true;
                }
            }

            _prevValue = _value;
            _prevResult = _result;
            _prevDistance = distance;
        }

        private float CalculateValue(Hand hand)
        {
            _closestPointIndex = CalculateClosestPoint(hand, Finger.FingerType.TYPE_INDEX, hand.Fingers[(int)Finger.FingerType.TYPE_THUMB].Bone(Bone.BoneType.TYPE_DISTAL).Center, out Spline index);
            _closestPointThumb = CalculateClosestPoint(hand, Finger.FingerType.TYPE_THUMB, _closestPointIndex, out Spline thumb);

            return Mathf.InverseLerp(1.0f, 0.4f, CalculateProgress(index, _closestPointIndex));
        }

        private Vector3 CalculateClosestPoint(Hand hand, Finger.FingerType finger, Vector3 point, out Spline spline)
        {
            Finger f = hand.Fingers[(int)finger];
            float min = float.MaxValue;
            Vector3 result = Vector3.zero;
            spline = new Spline();

            for (int i = -1; i < 4; i++)
            {
                Vector3 lineStart;
                if (i == -1)
                {
                    Bone b = f.Bone((Bone.BoneType)0);
                    lineStart = b.Center - (b.Direction.normalized * _startEndLength);
                }
                else
                {
                    lineStart = f.Bone((Bone.BoneType)i).Center;
                }
                spline.Add(new BezierKnot(lineStart));

                Vector3 lineEnd;
                if (i == 3)
                {
                    Bone b = f.Bone((Bone.BoneType)3);
                    lineEnd = b.Center + (b.Direction.normalized * _startEndLength);
                    spline.Add(new BezierKnot(lineEnd));
                }
                else
                {
                    lineEnd = f.Bone((Bone.BoneType)(i + 1)).Center;
                }

                Vector3 closest = ContactUtils.GetClosestPointOnFiniteLine(point, lineStart, lineEnd);
                float dist = Mathf.Abs(Vector3.Distance(closest, point));
                if (min > dist)
                {
                    min = dist;
                    result = closest;
                }
            }

            return result;
        }

        private float CalculateProgress(Spline spline, Vector3 point)
        {
            float closestDistance = float.MaxValue;
            float closestT = 0f;
            int samples = 1000;
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                Vector3 samplePoint = spline.EvaluatePosition(t);
                float distance = Vector3.Distance(point, samplePoint);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestT = t;
                }
            }
            return closestT;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_closestPointIndex, 0.01f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(_closestPointThumb, 0.01f);

            Gizmos.color = _prevResult ? Color.green : Color.red;
            Gizmos.DrawLine(_closestPointThumb, _closestPointIndex);

            Handles.Label(_closestPointThumb, "Distance: " + Mathf.Abs(Vector3.Distance(_closestPointThumb, _closestPointIndex)));
        }
#endif
    }
}