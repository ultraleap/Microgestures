using Leap.Unity.GestureMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarouselMgLogicSlide : CarouselMgLogic
{
    [Header("Swipe Configuration")]

    [Tooltip("This value is used to multiply the swipe progress to speed/slow alert swipes.")]
    [SerializeField] private float _sliderValueModifier = 2.5f;

    [Range(0, 1)]
    [Tooltip("Swipe will be considered accepted if progress exceeds this value")]
    [SerializeField] private float _acceptAfterProgress = 0.6f;

    [Tooltip("If checked, swipe progress will only be checked for an acceptable progress level when the user stops contacting (otherwise it will just accept as soon as it's hit)")]
    [SerializeField] private bool _acceptAfterThumbUp = false;

    [Space]

    [Tooltip("Smoothness applied to the slider's real value from the microgesture detector")]
    [SerializeField] private float _sliderDampening = 15.0f;

    [Tooltip("Speed applied to the ambient 'return to zero' speed when user isn't interacting: set this to zero to disable that functionality")]
    [SerializeField] private float _returnToZeroSpeed = 10.0f;

    [Tooltip("Speed applied to the auto completion of the swipe")]
    [SerializeField] private float _fillToOneSpeed = 10.0f;

    protected float _swipeProgressReal = 0.0f;
    protected float _swipeProgressLerped = 0.0f;

    private SwipeDirection _direction = SwipeDirection.NONE;

    private float _swipeStart = 0.0f;
    private float _tBase = 0.0f;
    private float _t = 0.0f;

    protected bool _completed = false;
    protected Action<SwipeDirection> OnCompletedToThreshold;

    private bool _hasThumbDown = false;

    private int _contactID = 0;
    private int _latestProgressedContactID = -1;

    public float AcceptAfterProgress => _acceptAfterProgress;

    protected void Update()
    {
        Progress?.Invoke(_swipeProgressLerped);
        _realValue = _swipeProgressReal;
    }

    private void ResetProgress()
    {
        _swipeStart = 0.0f;
        _swipeProgressReal = 0.0f;
        _swipeProgressLerped = 0.0f;
        _tBase = 0.0f;
        _t = 0.0f;

        _direction = SwipeDirection.NONE;
        _completed = false;
    }

    protected virtual void DoAdvance(SwipeDirection direction)
    {
        Advance?.Invoke(direction);
    }

    private void CheckForCompletion()
    {
        if (Mathf.Abs(_swipeProgressLerped) >= _acceptAfterProgress && !_completed)
        {
            _tBase = _swipeProgressLerped;
            _t = 0.0f;

            _completed = true;
            OnCompletedToThreshold?.Invoke(_direction);
        }
    }

    protected void FixedUpdate()
    {
        //Keep track of if we're going up or down
        _direction = _swipeProgressLerped > 0.0f ? SwipeDirection.RIGHT : SwipeDirection.LEFT;

        if (_completed || (!_completed && _thumbSlider.State != MicrogestureSystem.MicrogestureState.CONTACTING))
        {
            //If we reached an acceptable level, lerp it all the way to the end
            //If we didn't & the user stopped contacting, lerp it back down to nothing
            if (_t >= 1.0f)
            {
                _swipeProgressLerped = _completed ? _direction == SwipeDirection.RIGHT ? 1.0f : -1.0f : 0.0f;
                _t = 1.0f;

                if (_completed)
                {
                    DoAdvance(_direction);
                    _latestProgressedContactID = _contactID;
                    ResetProgress();

                    if (!_acceptAfterThumbUp && _thumbSlider.State == MicrogestureSystem.MicrogestureState.CONTACTING)
                        OnContactStart(_thumbSlider.SlideValue);
                }
            }
            else
            {
                _swipeProgressLerped = Mathf.Lerp(_tBase, _completed ? _direction == SwipeDirection.RIGHT ? 1.0f : -1.0f : 0.0f, _t);
                _t += Time.fixedDeltaTime * (_completed ? _fillToOneSpeed : _returnToZeroSpeed);
            }
        }
        else
        {
            //Apply a little smoothing for UI from the direct microgesture input
            _swipeProgressLerped = Mathf.Lerp(_swipeProgressLerped, _swipeProgressReal, Time.deltaTime * _sliderDampening);
        }

        //Check to see if we're past the completion threshold, if not checking when on thumb up
        if (!_acceptAfterThumbUp && !_completed && _contactID != _latestProgressedContactID)
        {
            CheckForCompletion();
            if (_completed)
                OnContactEnd(_thumbSlider.SlideValue);
        }
    }

    protected override void OnContactStart(float value)
    {
        _contactID++;
        _hasThumbDown = true;

        _swipeStart = value;

        base.OnContactStart(value);
    }

    protected override void OnContactEnd(float value)
    {
        _hasThumbDown = false;

        //Check to see if we were past our completion threshold
        if (_acceptAfterThumbUp)
            CheckForCompletion();

        _tBase = _swipeProgressLerped;
        _t = 0.0f;
        _swipeProgressReal = 0.0f;

        base.OnContactEnd(value);
    }

    protected override void OnSlide(float value)
    {
        if (_hasThumbDown)
            _swipeProgressReal = (value - _swipeStart) * _sliderValueModifier;
    }
}
