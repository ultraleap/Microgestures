using Leap.Unity;
using Leap.Unity.GestureMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractionDetector : MonoBehaviour
{
    [SerializeField] protected MicrogestureSystem _thumbSlider;

    protected virtual void Awake()
    {
        if (_thumbSlider == null)
            _thumbSlider = FindObjectOfType<MicrogestureSystem>();
    }

    protected virtual void OnEnable()
    {
        _thumbSlider.OnContactStart.AddListener(OnContactStart);
        _thumbSlider.OnContactEnd.AddListener(OnContactEnd);
        _thumbSlider.OnSlide.AddListener(OnSlide);
    }

    protected void OnDisable()
    {
        _thumbSlider.OnContactStart.RemoveListener(OnContactStart);
        _thumbSlider.OnContactEnd.RemoveListener(OnContactEnd);
        _thumbSlider.OnSlide.RemoveListener(OnSlide);
    }

    protected abstract void OnContactStart(float slideValue);
    protected abstract void OnContactEnd(float slideValue);
    protected abstract void OnSlide(float slideValue);

    public virtual Chirality chirality
    {
        get
        {
            return _thumbSlider.Chirality;
        }
        set
        {
            _thumbSlider.UpdateSystemChirality(value);
        }
    }

    protected void OnValidate()
    {
        if (_thumbSlider == null)
            _thumbSlider = FindObjectOfType<MicrogestureSystem>();
    }
}
