using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityWithFriction : MonoBehaviour
{
    [Tooltip("The higher this value, the faster the velocity slows down")]
    [SerializeField] private float _frictionCoefficient = 4.0f;
    [SerializeField] private float _minVelocity = 0.1f;
    [SerializeField] private float _inertiaThreshold = 0.05f;

    public float Velocity => _velocity;
    private float _velocity = 0.0f;
    
    public float PreviousVelocity => _prevVel;
    private float _prevVel = 0;

    private float _prevSlideValue = 0;

    void Update()
    {
        _prevVel = _velocity;

        if (Mathf.Abs(_velocity) < _inertiaThreshold)
            _velocity = 0;

        if (_velocity > 0)
            _velocity -= _frictionCoefficient * Time.fixedDeltaTime;
        else if (_velocity < 0)
            _velocity += _frictionCoefficient * Time.fixedDeltaTime;
    }

    public void UpdateVelocity(float slide)
    {
        _prevVel = _velocity;
        var newVelocity = (slide - _prevSlideValue) / Time.deltaTime;

        _prevSlideValue = slide;

        if (Mathf.Abs(newVelocity) < _minVelocity)
            newVelocity = 0;

        _velocity = newVelocity;
    }

    public void RollBackVelocity()
    {
        _velocity = _prevVel;
    }

    public void Clear()
    {
        _velocity = 0;
        _prevVel = 0;
    }
}
