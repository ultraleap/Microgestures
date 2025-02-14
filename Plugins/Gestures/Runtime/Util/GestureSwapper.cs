using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Leap.Unity.GestureMachine
{
    public class GestureSwapper : MonoBehaviour
    {
        [SerializeField]
        private GestureMachine _gestureMachine;

        [SerializeField]
        private List<BaseGesture> _gestures = new List<BaseGesture>();

        [SerializeField]
        private Key _backKey = Key.LeftArrow, _forwardKey = Key.RightArrow;

        private int _index = 0;

        private void Start()
        {
            if (_gestureMachine.gesture != null)
            {
                int ind = _gestures.FindIndex(x => x == _gestureMachine.gesture);
                if (ind != -1)
                {
                    _index = ind;
                }
            }
        }

        private void Update()
        {
            if (Keyboard.current[_backKey].wasPressedThisFrame)
            {
                _index--;
                UpdateIndex();
            }
            if (Keyboard.current[_forwardKey].wasPressedThisFrame)
            {
                _index++;
                UpdateIndex();
            }
        }

        private void UpdateIndex()
        {
            if (_index < 0)
            {
                _index = _gestures.Count - 1;
            }
            if (_index >= _gestures.Count)
            {
                _index = 0;
            }
            _gestureMachine.gesture = _gestures[_index];
        }

        private void OnValidate()
        {
            _gestureMachine = GetComponent<GestureMachine>();
        }
    }
}