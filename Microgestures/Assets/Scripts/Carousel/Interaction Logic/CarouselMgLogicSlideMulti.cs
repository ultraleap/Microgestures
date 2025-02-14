using Leap.Unity.GestureMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarouselMgLogicSlideMulti : CarouselMgLogic
{
    [SerializeField] private float _distanceMultiplier = 5.0f;
    [SerializeField] private float _inputDampening = 20.0f;
    [SerializeField] private float _returnToZeroLerp = 50.0f;

    private float _swipeProgressReal = 0.0f;
    private float _swipeProgressLerped = 0.0f;

    private float _swipeStart = 0.0f;

    private float _convertedProgressVal = 0.0f;
    private float _previousVal = 0.0f;

    protected override void OnContactStart(float value)
    {
        _swipeStart = value;
        base.OnContactStart(value);
    }

    protected override void OnContactEnd(float value)
    {
        _swipeProgressReal = 0.0f;
        base.OnContactEnd(value);
    }

    protected override void OnSlide(float value)
    {
        _swipeProgressReal = value - _swipeStart;
    }

    private void Update()
    {
        _swipeProgressLerped = Mathf.Lerp(_swipeProgressLerped, _swipeProgressReal, Time.deltaTime * _inputDampening);

        float multipliedVal = _swipeProgressLerped * _distanceMultiplier;
        if (_thumbSlider.State == MicrogestureSystem.MicrogestureState.CONTACTING)
        {
            float diff = multipliedVal - _previousVal;

            _convertedProgressVal += diff;
            if (_convertedProgressVal > 1.0f || _convertedProgressVal < -1.0f)
            {
                Advance?.Invoke(_convertedProgressVal > 1.0f ? SwipeDirection.RIGHT : SwipeDirection.LEFT);
                _convertedProgressVal = 0.0f;
            }
        }
        else
        {
            _convertedProgressVal = Mathf.Lerp(_convertedProgressVal, 0.0f, Time.deltaTime * _returnToZeroLerp);
        }
        _previousVal = multipliedVal;

        Progress?.Invoke(_convertedProgressVal);
        _realValue = _swipeProgressReal;
    }
}
