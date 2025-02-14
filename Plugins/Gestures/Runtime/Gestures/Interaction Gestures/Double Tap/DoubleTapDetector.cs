using System;
using UnityEngine;
using UnityEngine.Events;

public class DoubleTapDetector : InteractionDetector
{
    //TODO: don't double tap if user has done tap->slide->tap in quick succession

    public UnityEvent OnDoubleTap;
    public DistanceDirectionTapDetector tapDetector; 
    [Tooltip("If the user performs a successful tap twice within this time threshold, it will fire OnDoubleTap.")]
    [Range(0, 2)]
    [SerializeField] private float _maxTimeToConsiderDoubleTap = 0.558f;


    [Header("Params")]
    [Tooltip("If the thumb's slide value moves this distance away from its initial position on \"contact\" then it no longer counts as a tap - the user is trying to slide.")]
    [SerializeField] private float _slideDeadzone = 0.25f;

    private float _timeOfLastTap = -1000f;
    private bool _waitingForFirstTap = false;

    private float _slideStartValue;

    [SerializeField] private bool _showDebugLogs = false;

    void Start()
    {
        tapDetector.OnTap.AddListener(OnTap);
    }

    private void OnTap()
    {
        if (_showDebugLogs) Debug.Log("on tap");
        float deltaTapTime = Time.time - _timeOfLastTap;
        _timeOfLastTap = Time.time;

        if (_waitingForFirstTap)
        {
            //tap #1
            _waitingForFirstTap = false;

            if (_showDebugLogs) Debug.Log("single tap");

            return;
        }


        if (deltaTapTime <= _maxTimeToConsiderDoubleTap && !_waitingForFirstTap)
        {
            OnDoubleTap?.Invoke();
            if (_showDebugLogs) Debug.Log("double tap");
            _waitingForFirstTap = true;
        }
    }

    protected override void OnContactStart(float slideValue)
    {
        if (_waitingForFirstTap)
        {
            _slideStartValue = slideValue;
        }
    }

    protected override void OnContactEnd(float slideValue)
    {
    }

    protected override void OnSlide(float slideValue)
    {
        if (Mathf.Abs(slideValue - _slideStartValue) > _slideDeadzone)
        {
            //deadzone broken 

            if (!_waitingForFirstTap)
            {
                if (_showDebugLogs) Debug.Log("deadzone broken");
            }
            _waitingForFirstTap = true;
        }
    }
}
