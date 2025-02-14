using Leap.Unity.GestureMachine;
using UnityEngine;

public class CarouselMgLogicInertia : CarouselMgLogic
{
    [SerializeField] private float _returnToZeroLerp = 50.0f;
    [SerializeField] private float _returnToZeroThreshold = 0.2f;

    [SerializeField] private float _decay = 1.0f;
    [SerializeField] private float _speed = 10.0f;

    private float _convertedProgressVal = 0.0f;

    private float _prevValue = 0.0f;
    private float _value = 0.0f;

    private float _prevSlideVal = 0.0f;

    protected override void OnContactStart(float value)
    {
        _prevSlideVal = value;
        base.OnContactStart(value);
    }

    protected override void OnContactEnd(float value)
    {
        UpdateValue(value);
        base.OnContactEnd(value);
    }

    protected override void OnSlide(float slide)
    {
        UpdateValue(slide);
    }

    private void UpdateValue(float value)
    {
        _value += (value - _prevSlideVal) * _speed;
        _prevSlideVal = value;
    }

    void Update()
    {
        if (Mathf.Abs(_value) < _returnToZeroThreshold && _thumbSlider.State != MicrogestureSystem.MicrogestureState.CONTACTING)
        {
            _convertedProgressVal = Mathf.Lerp(_convertedProgressVal, 0.0f, Time.deltaTime * _returnToZeroLerp);
        }
        else
        {
            _prevValue = _value;
            _value -= _decay * _value * Time.deltaTime;

            float diffThisFrame = _value - _prevValue;

            _convertedProgressVal += diffThisFrame;
            if (_convertedProgressVal > 1.0f || _convertedProgressVal < -1.0f)
            {
                Advance?.Invoke(_convertedProgressVal > 1.0f ? SwipeDirection.LEFT : SwipeDirection.RIGHT);
                _convertedProgressVal = 0.0f;
            }
        }

        Progress?.Invoke(_convertedProgressVal * -1.0f);
    }
}
