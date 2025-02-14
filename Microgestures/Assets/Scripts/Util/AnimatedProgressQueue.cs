using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedProgressQueue : MonoBehaviour
{
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private AnimationCurve _animCurve;

    public Action<float> Progress;
    public Action<SwipeDirection> Advance;

    private List<SwipeDirection> _queue = new List<SwipeDirection>();
    private float _value = 0.0f;

    private float _t = 0.0f;

    public void AddDirectionToQueue(SwipeDirection direction)
    {
        _queue.Add(direction);
    }

    void Update()
    {
        if (_queue.Count > 0)
        {
            _t += Time.deltaTime;
            if (_t >= _duration)
                _t = _duration;

            float t = _animCurve.Evaluate(_t / _duration);
            switch (_queue[0])
            {
                case SwipeDirection.LEFT:
                    _value -= t;
                    break;
                case SwipeDirection.RIGHT:
                    _value += t;
                    break;
            }
            if (Mathf.Abs(_value) >= 1.0f)
            {
                Advance?.Invoke(_queue[0]);
                _queue.RemoveAt(0);
                _value = 0.0f;
                _t = 0.0f;
            }

            Progress?.Invoke(_value);
        }
    }
}
