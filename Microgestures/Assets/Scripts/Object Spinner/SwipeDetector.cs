using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.Events;

/* 
 * 
 * This checks for a swipe, defined as:
 *    - User touches
 *    - User moves their finger left or right from initial touch
 *    - User releases touch
 *    
 * Once the swipe has completed, we fire OnSwiped with the overall velocity.
 * OnSwipeDirectionChange will fire at any time during the swipe with current direction.
 * 
 */

public class SwipeDetector : InteractionDetector
{
    [SerializeField] private float _distanceThreshold = 0.125f;

    public UnityEvent<SwipeDirection, float> OnSwiped;
    public UnityEvent<SwipeDirection> OnSwipeDirectionChange;

    public SwipeDirection CurrentDirection => _currentDirection;
    private SwipeDirection _currentDirection = SwipeDirection.NONE;

    public float Velocity => _velocity;
    private float _velocity = 0.0f;
    
    public float Acceleration => _acceleration;
    private float _acceleration = 0.0f;

    private float _prevSlideValue;
    private float _prevVelocity;

    private float SlideValueOnContactStart;
    private float _totalDistanceDelta;

    protected override void OnContactStart(float slideValue)
    {
        SlideValueOnContactStart = slideValue;
        _currentDirection = SwipeDirection.NONE;
        OnSwipeDirectionChange?.Invoke(_currentDirection);
    }

    protected override void OnContactEnd(float slideValue)
    {
        UpdateSwipeValues();
        CheckForSwipe(false);
    }

    protected override void OnSlide(float value)
    {
        UpdateSwipeValues();
        CheckForSwipe(true);
    }

    private void UpdateSwipeValues()
    {
        float distanceMoved = _thumbSlider.SlideValue - _prevSlideValue;
        _velocity = distanceMoved / Time.deltaTime;
        _acceleration = (_velocity - _prevVelocity) / Time.time;

        _prevSlideValue = _thumbSlider.SlideValue;
        _prevVelocity = _velocity;
        _totalDistanceDelta = _thumbSlider.SlideValue - SlideValueOnContactStart;
    }

    private void CheckForSwipe(bool isFutureSwipe)
    {
        SwipeDirection newDirection = SwipeDirection.NONE;
        if (/*velocity > velocityThreshold || */Mathf.Abs(_totalDistanceDelta) > _distanceThreshold)
        {
            float distance = _totalDistanceDelta;
            if (distance < 0)
            {
                newDirection = SwipeDirection.LEFT;
            }
            else
            {
                newDirection = SwipeDirection.RIGHT;
            }

        }

        if (isFutureSwipe)
        {
            if(newDirection != _currentDirection)
            {
                _currentDirection = newDirection;
                OnSwipeDirectionChange?.Invoke(_currentDirection);
            }
        }
        else
        {
            OnSwiped?.Invoke(newDirection, _velocity);
        }
    }
}

public enum SwipeDirection { LEFT, RIGHT, NONE };