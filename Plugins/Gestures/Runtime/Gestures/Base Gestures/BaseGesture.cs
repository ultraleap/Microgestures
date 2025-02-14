using Leap;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public abstract class BaseGesture : MonoBehaviour
    {
        [HideInInspector]
        public bool result = false;
        [HideInInspector]
        public float value = 0;

        [Tooltip("Retains the previous state when the stop tracking functions are called (when a hand is no longer tracked).")]
        public bool retainValueOnStop = false;

        protected bool isTracked = false;

        private bool _isChildGesture = false;
        public List<BaseGesture> childGestures = new List<BaseGesture>();
        public BaseGesture parentGesture;

        [HideInInspector]
        public GestureMachine gestureMachine;

        [HideInInspector]
        public bool changedThisFrame = false;
        private int _lastFrameUpdate = -1;

        protected bool _oldResult = false;

#if UNITY_EDITOR
        [SerializeField, HideInInspector]
        internal List<int> _gestureOrder = new List<int>();
#endif

        public void StartTracking(Hand hand)
        {
            foreach (var child in childGestures)
            {
                if (child == this)
                    continue;
                child._isChildGesture = true;
                child.StartTracking(hand);
            }

            isTracked = true;
            StartTrackingFunction(hand);
        }

        protected abstract void StartTrackingFunction(Hand hand);

        public virtual void UpdateHandValue(Hand hand)
        {
            if (Time.frameCount > _lastFrameUpdate)
            {
                _lastFrameUpdate = Time.frameCount;
            }
            else
            {
                return;
            }

            changedThisFrame = false;

            foreach (var child in childGestures)
            {
                if (child == this)
                    continue;
                child._isChildGesture = true;
                child.UpdateHandValue(hand);
            }

            UpdateHandFunction(hand, out result, out value);

            ProcessOldResults();
        }

        protected abstract void UpdateHandFunction(Hand hand, out bool result, out float value);

        protected virtual void ProcessOldResults()
        {
            if (_oldResult != result)
            {
                changedThisFrame = true;
            }
            _oldResult = result;
        }

        public virtual void ResetValues()
        {

        }

        public void StopTracking()
        {
            _oldResult = false;
            changedThisFrame = false;
            foreach (var child in childGestures)
            {
                if (child == this)
                    continue;
                child._isChildGesture = true;
                child.StopTracking();
            }

            isTracked = false;
            StopTrackingFunction();
            if (!retainValueOnStop)
            {
                result = false;
                value = 0;
            }
        }

        protected abstract void StopTrackingFunction();

        protected virtual void OnValidate()
        {
            if (parentGesture == this)
                parentGesture = null;

#if UNITY_EDITOR
            if (parentGesture == null)
            {
                _gestureOrder.Clear();
                _gestureOrder.Add(-2);
                for (int i = 0; i < childGestures.Count; i++)
                {
                    if (childGestures[i] == null)
                    {
                        childGestures.RemoveAt(i);
                        i--;
                    }    
                }
                ClearChildOrders(this);
                int order = 0;
                SetChildOrder(this, ref order);
            }
#endif

            foreach (var child in childGestures)
            {
                if (child == null)
                    continue;
                child.parentGesture = this;
                if (child.childGestures.Contains(this))
                {
                    child.childGestures.Remove(this);
                }
                if (parentGesture == child)
                {
                    parentGesture = null;
                }
            }
        }

#if UNITY_EDITOR
        private void ClearChildOrders(BaseGesture gesture)
        {
            foreach (var child in gesture.childGestures)
            {
                if (child == null) continue;

                if (child.childGestures.Count > 0)
                {
                    ClearChildOrders(child);
                }
                child._gestureOrder.Clear();
            }
        }

        private void SetChildOrder(BaseGesture gesture, ref int order)
        {
            foreach (var child in gesture.childGestures)
            {
                if (child == null) continue;

                if (child.childGestures.Count > 0)
                {
                    SetChildOrder(child, ref order);
                }
                int newOrder = order;
                child._gestureOrder.Add(newOrder);
                order++;
            }
        }
#endif
    }
}