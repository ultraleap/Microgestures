using Leap;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    /// <summary>
    /// Returns true if children are enabled in order
    /// </summary>
    public class SequentialGesture : MultiGesture
    {
        [Tooltip("Requires all children to be true to complete, useful for doing complex \"motions\"")]
        public bool requireAll = false;

        [Tooltip("Jumps the index back to zero when completed")]
        public bool resetOnEnd = false;

        [Tooltip("Time that each gesture state should remain true, even if it's not. Useful for cooldowns.")]
        public float stickyTime = 0f;

        private List<int> _gestureFrames = new List<int>();
        private List<float> _gestureTimes = new List<float>();
        [SerializeField, Tooltip("Enables/disables the frame history check on individual items. " +
            "Setting the index to false will simply make it work off a true condition for that item, without checking against the previous. " +
            "Only works when Require All is *false*.")]
        private List<bool> _requireFrameIncrease = new List<bool>();
        private int _currentIndex = 0;

        protected override void StartTrackingFunction(Hand hand)
        {
            for (int i = 0; i < _gestureFrames.Count; i++)
            {
                _gestureFrames[i] = 0;
                _gestureTimes[i] = 0;
            }

            if (childGestures.Count > 0)
            {
                while (_gestureFrames.Count != childGestures.Count)
                {
                    if (_gestureFrames.Count < childGestures.Count)
                    {
                        _gestureFrames.Add(0);
                        _gestureTimes.Add(0);
                    }
                    else
                    {
                        _gestureFrames.RemoveAt(0);
                        _gestureTimes.RemoveAt(0);
                    }
                }
            }
            else
            {
                _gestureFrames.Clear();
                _gestureTimes.Clear();
            }

            UpdateFrameIncrease();

            _currentIndex = 0;
        }

        protected override void StopTrackingFunction()
        {

        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            if (childGestures.Count == 1)
            {
                result = childGestures[0].result;
                value = childGestures[0].result ? 1 : 0;
                return;
            }

            for (int i = 0; i < childGestures.Count; i++)
            {
                if (childGestures[i].result && childGestures[i].changedThisFrame)
                {
                    _gestureFrames[i] = Time.frameCount;
                }
                if (childGestures[i].result)
                {
                    _gestureTimes[i] = stickyTime > 0 ? stickyTime : 1;
                }
                else
                {
                    _gestureTimes[i] = stickyTime > 0 ? _gestureTimes[i] - Time.deltaTime : 0;
                }
            }

            if (requireAll)
            {
                _currentIndex = 0;
                int lastFrame = -1;
                for (int i = 0; i < childGestures.Count; i++)
                {
                    if (_gestureTimes[i] > 0 && _gestureFrames[i] > lastFrame)
                    {
                        _currentIndex++;
                        lastFrame = _gestureFrames[i];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                if (_currentIndex == 0)
                {
                    if (_gestureTimes[_currentIndex] > 0)
                    {
                        _currentIndex++;
                    }
                }
                else if (_currentIndex >= _gestureTimes.Count)
                {
                    if (_gestureTimes[_currentIndex - 1] <= 0)
                    {
                        if (resetOnEnd)
                        {
                            _currentIndex = 0;
                        }
                        else
                        {
                            _currentIndex--;
                        }
                    }
                }
                else
                {
                    if (_gestureTimes[_currentIndex] > 0 && (!_requireFrameIncrease[_currentIndex] || _gestureFrames[_currentIndex] > _gestureFrames[_currentIndex - 1]))
                    {
                        _currentIndex++;
                    }
                    else if (_gestureTimes[_currentIndex - 1] <= 0)
                    {
                        _currentIndex--;
                    }
                }
            }

            result = _currentIndex == childGestures.Count;
            value = (float)_currentIndex / childGestures.Count;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            UpdateFrameIncrease();
        }

        private void UpdateFrameIncrease()
        {
            for (int i = 0; i < childGestures.Count; i++)
            {
                if (i > _requireFrameIncrease.Count - 1)
                {
                    _requireFrameIncrease.Add(true);
                }
            }
            for (int i = _requireFrameIncrease.Count - 1; i >= 0; i--)
            {
                if (i > childGestures.Count - 1)
                {
                    _requireFrameIncrease.RemoveAt(i);
                }
            }
        }
    }
}