using UnityEngine;
using UnityEngine.UI;

namespace Leap.Unity.GestureMachine
{
    public class GestureProgressVisual : MonoBehaviour
    {
        [SerializeField]
        private GestureMachine _gestureMachine = null, _interactionMachine = null;

        [SerializeField]
        private ThumbFlickGesture _flickGesture;

        [SerializeField]
        private UnityEngine.UI.Image _image = null;

        [Header("Gesture")]
        [SerializeField]
        private Gradient _gestureGradient = null;
        [SerializeField]
        private Color _interactionColor = Color.white;

        [SerializeField]
        private AnimationCurve _gestureScale = null;
        [SerializeField]
        private float _interactionScale = 1f;

        [SerializeField]
        private Color _noTrackingColor = Color.white;

        [SerializeField]
        private float _transitionTime = 0.1f;

        [SerializeField]
        private float _interactionCooldown = 0.5f;

        private float _currentSample = 0f;
        private float _interactionSample = 0f;

        private Color _currentColor;

        private float _currentScale;

        private bool _flicked = false;

        private void Start()
        {
            _interactionMachine.OnGestureChange += ChangeGesture;
            ChangeGesture();
        }

        private void Update()
        {
            _currentSample = _gestureMachine.CurrentValue;

            if (_interactionMachine.processing)
            {
                if (_interactionSample > 0f)
                {
                    _interactionSample -= Time.deltaTime * (1f / _interactionCooldown);

                }

                if (_interactionMachine.CurrentState)
                {
                    if (_flickGesture != null)
                    {
                        if (_flickGesture.changedThisFrame && _flickGesture.result)
                        {
                            _flicked = true;
                        }
                    }
                    else
                    {
                        _interactionSample = 1f;
                    }
                }
            }

            UpdateImage();
        }

        private void UpdateImage()
        {
            if (_flicked)
            {
                _currentColor = _interactionColor;
                _currentScale = _interactionScale;
            }
            else
            {
                if (_interactionMachine.processing)
                {
                    _currentColor = Color.Lerp(_gestureGradient.Evaluate(1), _interactionColor, _interactionSample);
                }
                else
                {
                    _currentColor = Color.Lerp(_currentColor, _gestureMachine.Tracked ? _gestureGradient.Evaluate(_currentSample) : _noTrackingColor, Time.deltaTime * (1f / _transitionTime));
                }


                if (_interactionMachine.processing)
                {
                    _currentScale = Mathf.Lerp(_gestureScale.Evaluate(1), _interactionScale, _interactionSample);
                }
                else
                {
                    _currentScale = Mathf.Lerp(_currentScale, _gestureMachine.Tracked ? _gestureScale.Evaluate(_currentSample) : 0, Time.deltaTime * (1f / _transitionTime));
                }
            }
            _image.color = _currentColor;
            _image.transform.localScale = Vector3.one * _currentScale;
        }

        private void ChangeGesture()
        {
            SetupFlick();
        }

        private void SetupFlick()
        {
            if (_flickGesture != null)
            {
                _flickGesture.OnFlickReset -= OnFlickReset;
            }
            _flicked = false;
            try
            {
                ThumbFlickGesture thumbFlick = (ThumbFlickGesture)_interactionMachine.gesture;
                if (thumbFlick != null)
                {
                    _flickGesture = thumbFlick;
                    _flickGesture.OnFlickReset += OnFlickReset;
                }
            }
            catch { }
        }

        private void OnFlickReset()
        {
            _flicked = false;
            _interactionSample = 1f;
        }
    }
}