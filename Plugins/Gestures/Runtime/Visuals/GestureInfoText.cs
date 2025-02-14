using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Leap.Unity.GestureMachine
{
    public class GestureInfoText : MonoBehaviour
    {
        [SerializeField]
        GestureMachine _gestureMachine;

        [SerializeField]
        GestureSystem _gestureSystem;

        BaseGesture _currentGesture;

        TextMeshProUGUI _text;
        private bool _oldSingle = false;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            UpdateText();
        }

        private void LateUpdate()
        {
            UpdateText();
        }

        private void UpdateText()
        {
            if (_gestureMachine != null)
            {
                if (_gestureMachine.gesture != _currentGesture)
                {
                    _currentGesture = _gestureMachine.gesture;
                    if (_currentGesture != null)
                    {
                        _text.text = GetString();
                    }
                }
                if (_gestureSystem != null && _oldSingle != _gestureSystem.singleInteraction)
                {
                    _text.text = GetString();
                    _oldSingle = _gestureSystem.singleInteraction;
                }
            }
        }

        private string GetString()
        {
            return _currentGesture.gameObject.name + (_gestureSystem != null && _gestureSystem.singleInteraction ? " (Single Interaction)" : "");
        }
    }
}