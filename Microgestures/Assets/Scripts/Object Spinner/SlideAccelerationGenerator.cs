using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* 
 * 
 * When a user swipes left/right while contacting, this will generate an acceleration value.
 * 
 */

public class SlideAccelerationGenerator : InteractionDetector
{
    [Tooltip("1 = no effect at all.\n0 = no movement, >1 faster movement")]
    [SerializeField] private float _modifier = 0.6f;

    [Tooltip("If true, a mouselike acceleration is applied to the slide - movements are amplified. Faster movements move you further, and smaller movements less")]
    [SerializeField] private bool _acceleration = true;

    public UnityEvent<float> OnAcceleration;

    public float NormalizedValue => _normalizedValue;
    private float _normalizedValue;

    private float _oldT = 0;

    protected override void OnContactStart(float t)
    {
        _oldT = t;
    }

    protected override void OnContactEnd(float t)
    {

    }

    protected override void OnSlide(float t)
    {
        float deltaT = t - _oldT;
        float speed = Mathf.Abs(deltaT / Time.deltaTime);
        _oldT = t;

        SetValue(Mathf.Clamp01(_normalizedValue + (deltaT * (_modifier * (_acceleration ? speed : 1.0f)))));
    }

    public void SetValue(float value)
    {
        _normalizedValue = Mathf.Clamp01(value);
        OnAcceleration?.Invoke(_normalizedValue);
    }
}
