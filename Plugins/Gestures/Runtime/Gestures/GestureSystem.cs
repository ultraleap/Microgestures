using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Leap.Unity.GestureMachine
{
    public class GestureSystem : MonoBehaviour
    {
        [SerializeField]
        private GestureMachine _wakeGesture;
        [SerializeField]
        private List<GestureMachine> _interactionGestures = new List<GestureMachine>();

        private bool _interacting = false;

        [SerializeField]
        private Key _singleToggleKey = Key.Space;

        public bool singleInteraction = true;

        [SerializeField]
        private float _timeOut = 1f;
        private float _currentTimeOut = 1f;

        private void Start()
        {
            _wakeGesture.OnStateChange += WakeState;

            foreach (var item in _interactionGestures)
            {
                item.OnStateChange += InteractionState;
                item.processing = false;
            }
        }

        private void Update()
        {
            if (Keyboard.current[_singleToggleKey].wasPressedThisFrame)
            {
                singleInteraction = !singleInteraction;
            }

            if (_interacting)
            {
                _currentTimeOut -= Time.deltaTime;
                if (_currentTimeOut <= 0f)
                {
                    _interacting = false;
                    _wakeGesture.processing = true;
                    ChangeInteractions(false);
                }
                foreach (var item in _interactionGestures)
                {
                    if (Mathf.Abs(item.CurrentValue) > 0f)
                    {
                        _currentTimeOut = _timeOut;
                        ResetInteractions(item);
                    }
                }
            }
        }

        private void WakeState(bool result, float val)
        {
            if (_interacting)
                return;

            if (result)
            {
                ChangeInteractions(true);
                _interacting = true;
                _currentTimeOut = _timeOut;
            }
        }

        private void InteractionState(bool result, float val)
        {
            foreach (var item in _interactionGestures)
            {
                item.ResetValues();
            }
            if (result && singleInteraction)
            {
                ChangeInteractions(false);
                _interacting = false;
                _wakeGesture.processing = true;
            }
        }

        private void ChangeInteractions(bool processing)
        {
            foreach (var item in _interactionGestures)
            {
                item.processing = processing;
            }
        }

        private void ResetInteractions(GestureMachine ignoreMe)
        {
            foreach (var item in _interactionGestures)
            {
                if (ignoreMe == item)
                    continue;

                item.ResetValues();
            }
        }
    }
}