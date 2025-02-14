using Leap.Unity.GestureMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* 
 * 
 * When a user swipes left/right while contacting they will essentially apply a force in that direction.
 * This generator handles a fake "friction" which slows the force over time, creating a resisted velocity effect.
 * 
 */

[RequireComponent(typeof(VelocityWithFriction))]
public class InertiaGenerator : InteractionDetector
{
    public UnityEvent<float> OnInertia;

    public float Velocity => _velocityHandler.Velocity;

    private VelocityWithFriction _velocityHandler;

    public MicrogestureSystem System => _thumbSlider;

    private void Start()
    {
        _velocityHandler = GetComponent<VelocityWithFriction>();   
    }

    protected override void OnContactStart(float value)
    {
        _velocityHandler.UpdateVelocity(value);
        _velocityHandler.Clear();
    }

    protected override void OnContactEnd(float value)
    {
        _velocityHandler.UpdateVelocity(value);
        _velocityHandler.RollBackVelocity();
    }

    protected override void OnSlide(float slide)
    {
        _velocityHandler.UpdateVelocity(slide);
    }

    void Update()
    {
        OnInertia?.Invoke(_velocityHandler.Velocity);
    }
}
