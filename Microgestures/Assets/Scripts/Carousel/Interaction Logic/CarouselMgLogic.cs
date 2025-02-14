using System;

public class CarouselMgLogic : InteractionDetector
{
    public Action<SwipeDirection> Advance;
    public Action<float> Progress;

    public float RealValue => _realValue;
    protected float _realValue = 0.0f;

    public bool Interacting => _interacting;
    protected bool _interacting = false;

    protected override void OnContactEnd(float value)
    {
        _interacting = false;
    }

    protected override void OnContactStart(float value)
    {
        _interacting = true;
    }

    protected override void OnSlide(float value)
    {

    }
}
