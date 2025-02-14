using Leap;
using Leap.Unity;
using System;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public class GestureMachine : MonoBehaviour
    {
        [SerializeField, Tooltip("Feed this a basic LSP to get local space calculations.")]
        private LeapProvider _leapProvider = null;

        public LeapProvider leapProvider => _leapProvider;

        public Chirality chirality = Chirality.Right;

        public BaseGesture gesture = null;
        private BaseGesture _currentGesture = null;
        private DirectionalGesture _directionalGesture = null;

        private Hand _hand;
        private bool _wasNull = true;

        public bool CurrentState { get { return gesture != null && gesture.result; } }
        public float CurrentValue { get { if (gesture == null) return 0f; return gesture.value; } }
        public DirectionalGesture.Direction? CurrentDirection { get { return _directionalGesture?.resultDirection; } }

        public bool ChangedThisFrame { get { return gesture != null && gesture.changedThisFrame; } }

        public bool Tracked { get { return _hand != null; } }

        [Tooltip("Set this to false to treat all the hand data as null, thus causing all the gestures to gracefully exit.")]
        public bool processing = true;

        public Action<bool, float> OnStateChange;
        public Action OnGestureChange;

        private void Awake()
        {
            if (_leapProvider == null)
            {
                _leapProvider = Hands.Provider;
            }
            SetGestureMachine();
        }

        private void Update()
        {
            _hand = processing ? _leapProvider.GetHand(chirality) : null;

            if (gesture != null)
            {
                if (gesture != _currentGesture)
                {
                    SetGestureMachine();
                }

                if (_hand != null)
                {
                    if (_wasNull)
                    {
                        _wasNull = false;
                        gesture.StartTracking(_hand);
                    }

                    gesture.UpdateHandValue(_hand);

                    if (gesture.changedThisFrame)
                    {
                        OnStateChange?.Invoke(gesture.result, gesture.value);
                    }
                }
                else
                {
                    if (!_wasNull)
                    {
                        _wasNull = true;

                        bool oldResult = gesture.result;

                        gesture.StopTracking();

                        if (gesture.result != oldResult && !gesture.result)
                        {
                            OnStateChange?.Invoke(gesture.result, gesture.value);
                        }
                    }
                }
            }
        }

        private void SetGestureMachine()
        {
            if (_currentGesture != null)
            {
                _currentGesture.StopTracking();
            }
            _currentGesture = gesture;
            _directionalGesture = null;
            if (gesture != null)
            {
                if (gesture is DirectionalGesture)
                {
                    _directionalGesture = (DirectionalGesture)gesture;
                }
                gesture.gestureMachine = this;
                RecursiveGestureMachine(gesture);
            }
            _wasNull = true;
            OnGestureChange?.Invoke();
        }

        private void RecursiveGestureMachine(BaseGesture gesture)
        {
            foreach (var child in gesture.childGestures)
            {
                if (child.childGestures.Count > 0)
                {
                    RecursiveGestureMachine(child);
                }
                child.gestureMachine = this;
            }
        }

        public void ResetValues()
        {
            Debug.Log("Reset " + gesture.name);
            if (gesture != null)
            {
                RecursiveReset(gesture);
            }
        }

        private void RecursiveReset(BaseGesture gesture)
        {
            foreach (var child in gesture.childGestures)
            {
                if (child.childGestures.Count > 0)
                {
                    RecursiveReset(child);
                }
                child.ResetValues();
            }
        }
    }
}