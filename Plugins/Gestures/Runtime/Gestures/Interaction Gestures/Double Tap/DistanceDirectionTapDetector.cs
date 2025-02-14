using Leap.Unity.GestureMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DistanceDirectionTapDetector : InteractionDetector
{
    [Header("Setup")]
    [SerializeField] private ThumbPosCloseToIndexGesture _contactDetector;

    [Header("Params")]
    [Tooltip("If the thumb's slide value moves this distance away from its initial position on \"contact\" then it no longer counts as a tap - the user is trying to slide.")]
    [SerializeField] private float _slideDeadzone = 0.25f;

    [Tooltip("If the thumb's movement stays within this distance of the previous position frame over frame, then it is remaining still")]
    [SerializeField] private float _contactStillDeadzone = 0.0003f;

    [Tooltip("The amount of time the thumb's contact direction must consistently move in to consitute a trend")]
    [SerializeField] private float _positiveTrendTime = 0.0333f;

    [Tooltip("The amount of time the thumb's contact direction must consistently move in a different direction to what we're looking for to break the trend")]
    [SerializeField] private float _negativeTrendTime = 0.022f;

    [Header("Debug")]
    [SerializeField] private bool _enableDebugLogs = false;

    public UnityEvent OnTap;

    private bool _tapped = false;
    private float _slideStartValue;
    private float _prevContactValue = -1;
    private bool _deadzoneBroken = false;
    private Queue<ContactTimestamp> _contactTimestamps = new Queue<ContactTimestamp>();

    protected override void OnContactEnd(float slideValue)
    {
        DeadzoneCheck(slideValue);
        if(!_deadzoneBroken)
        {
            Tap();
        }

        _prevContactValue = -1;
    }

    protected override void OnContactStart(float slideValue)
    {
        _contactTimestamps.Clear();
        _deadzoneBroken = false;
        _prevContactValue = _contactDetector.DistanceToContact;
        _slideStartValue = slideValue;
        _tapped = false;
    }

    protected override void OnSlide(float slideValue)
    {

        DeadzoneCheck(slideValue);

        float delta = _contactDetector.DistanceToContact - _prevContactValue;
        TapMovementDirection direction;

        if (Mathf.Abs(delta) < _contactStillDeadzone)
        {
            direction = TapMovementDirection.STILL;
        }
        else if (delta < 0)
        {
            direction = TapMovementDirection.DOWN;
        }
        else
        {
            direction = TapMovementDirection.UP;
        }


        //Arbitrary queue length... could probably be smaller - currently around 1.3seconds
        while (_contactTimestamps.Count > 120)
        {
            _contactTimestamps.Dequeue();
        }
        _contactTimestamps.Enqueue(new ContactTimestamp(Time.time, direction, _contactDetector.DistanceToContact));

        if (direction == TapMovementDirection.UP)
        {
            if (!_tapped && !_deadzoneBroken)
            {
                bool isMovingUp = CheckForDirectionTrend(TapMovementDirection.UP);

                if (isMovingUp)
                {
                    Tap();
                }
            }
        } 
        else if (direction == TapMovementDirection.DOWN && _tapped)
        {
            _slideStartValue = slideValue;

            bool isMovingDown = CheckForDirectionTrend(TapMovementDirection.DOWN);
            if (isMovingDown)
            {
                _tapped = false;
            }
        }

        _prevContactValue = _contactDetector.DistanceToContact;
    }

    private void DeadzoneCheck(float slideValue)
    {
        if (Mathf.Abs(slideValue - _slideStartValue) > _slideDeadzone)
        {
            if (_enableDebugLogs && !_deadzoneBroken) { Debug.Log("TAP DEADZONE BROKEN"); }
            _deadzoneBroken = true;
        }
    }

    private void Tap()
    {
        if (_tapped) return;

        OnTap?.Invoke();
        _tapped = true;

        if (_enableDebugLogs) Debug.Log("TAP!");
    }

    
    private bool CheckForDirectionTrend(TapMovementDirection direction)
    {
        bool prevValueNotDirection = false;
        float timeAtStartOfPositiveTrend = -1;
        float timeAtStartOfNegativeTrend = -1;

        foreach (ContactTimestamp _contactTimestamp in _contactTimestamps.Reverse())
        {
            if (_contactTimestamp.movementDirection == direction)
            {
                if (timeAtStartOfPositiveTrend == -1)
                {
                    timeAtStartOfPositiveTrend = _contactTimestamp.timestamp;
                }

                float currentTimeOfTrend = timeAtStartOfPositiveTrend - _contactTimestamp.timestamp;
                prevValueNotDirection = false;

                if (currentTimeOfTrend >= _positiveTrendTime)
                {
                    return true;
                }
            }
            else
            {

                if (!prevValueNotDirection)
                {
                    timeAtStartOfNegativeTrend = _contactTimestamp.timestamp;
                }
                else
                {
                    float currentTimeOfTrend = timeAtStartOfNegativeTrend - _contactTimestamp.timestamp;
                    if (currentTimeOfTrend >= _negativeTrendTime)
                    {
                        return false;
                    }
                }
                prevValueNotDirection = true;
            }
        }
        return false;
    }

    public enum TapMovementDirection { UP, DOWN, STILL }

    [Serializable]
    public struct ContactTimestamp
    {
        public float timestamp;
        public TapMovementDirection movementDirection;
        public float contactDistance;

        public ContactTimestamp(float timestamp, TapMovementDirection movementDirection, float contactDistance)
        {
            this.timestamp = timestamp;
            this.movementDirection = movementDirection;
            this.contactDistance = contactDistance;
        }
    }
}
