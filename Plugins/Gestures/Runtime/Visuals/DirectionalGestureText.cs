using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Leap.Unity.GestureMachine
{
    public class DirectionalGestureText : MonoBehaviour
    {
        [SerializeField]
        private DirectionalGesture[] _directionalGestures;

        private TextMeshProUGUI _text;

        [SerializeField]
        private float _lerpTime = 1.0f;
        private float _currentTime = 0;

        private Color _fadedColor = Color.white;
        private Vector3 _fadedScale = Vector3.one * 0.8f;

        private void Awake()
        {
            _directionalGestures = FindObjectsByType<DirectionalGesture>(findObjectsInactive: FindObjectsInactive.Include, sortMode: FindObjectsSortMode.None);
            _fadedColor.a = 0f;
            _text = GetComponent<TextMeshProUGUI>();
            _text.transform.localScale = _fadedScale;
            _text.color = _fadedColor;
        }

        private void Update()
        {
            if (_currentTime >= 0)
            {
                _currentTime -= Time.deltaTime;
                if (_currentTime < 0)
                {
                    _currentTime = 0;
                }
            }
            foreach (var direction in _directionalGestures)
            {
                if (direction.result && direction.changedThisFrame)
                {
                    _text.text = direction.resultDirection.ToString();
                    _currentTime = _lerpTime;
                }
            }
            UpdateTextVisuals();
        }

        private void UpdateTextVisuals()
        {
            _text.color = Color.Lerp(_fadedColor, Color.white, _currentTime / _lerpTime);
            _text.transform.localScale = Vector3.Lerp(_fadedScale, Vector3.one, _currentTime / _lerpTime);
        }
    }
}
