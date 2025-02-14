using Leap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public class RepeatedGesture : BaseGesture
    {
        public float individualTimer = 0.5f;
        [Tooltip("Minimum required time between repeats to prevent false positives between frames.")]
        public float minimumWait = 0.05f;
        public int count = 2;
        [Tooltip("Holding the last gesture activation keeps the result as true until false.")]
        public bool holdingKeepsTrue = true;

        [Tooltip("If this is set to true, the first instance will require 1 less activation.")]
        public bool skipFirstCount = false;

        private bool _wasTrue = false;
        private float _currentTime = 0f, _minimumTime = 0f;
        private int _currentCount = 0;

        protected override void StartTrackingFunction(Hand hand)
        {
            if (skipFirstCount)
            {
                _currentCount = 1;
                _wasTrue = true;
                _currentTime = 0f;
            }
        }

        protected override void StopTrackingFunction()
        {
            _currentCount = 0;
        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            if (childGestures.Count < 1)
            {
                result = false;
                value = 0;
                return;
            }

            if (_currentCount == count && holdingKeepsTrue && childGestures[0].result)
            {
                result = true;
                value = 1;
                return;
            }

            if(_currentTime < individualTimer)
            {
                _currentTime += Time.deltaTime;
            }

            if(_minimumTime < minimumWait)
            {
                _minimumTime += Time.deltaTime;
            }

            if (_currentTime >= individualTimer)
            {
                _currentCount = 0;
            }

            if (childGestures[0].result && !_wasTrue)
            {
                if (_currentTime < individualTimer || _currentCount == 0)
                {
                    _currentCount++;
                }
                _currentTime = 0f;
                _minimumTime = 0f;
                _wasTrue = true;
            }

            if (!childGestures[0].result)
            {
                if(_minimumTime > minimumWait)
                {
                    _wasTrue = false;
                    if (_currentCount == count)
                    {
                        _currentCount = 0;
                    }
                }
            }

            result = _currentCount == count;
            value = (float)_currentCount / count;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            while (childGestures.Count > 1)
            {
                childGestures.RemoveAt(childGestures.Count - 1);
            }
        }

        public override void ResetValues()
        {
            _currentCount = 0;
            _wasTrue = true;
        }
    }
}