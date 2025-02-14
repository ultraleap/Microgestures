using Leap.Unity.GestureMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapMaterialOnTap : MonoBehaviour
{
    [SerializeField] private MicrogestureSystem _thumbSlider;

    [SerializeField] private MeshRenderer _meshRenderer;

    [SerializeField] private Color _inactiveColour;
    [SerializeField] private Color _activeColour;

    [SerializeField] private float _lerpSpeed = 1.0f;

    private Color _targetColour;

    void Start()
    {
        if(_thumbSlider == null)
            _thumbSlider = FindObjectOfType<MicrogestureSystem>();

        if(_meshRenderer == null)
            _meshRenderer = GetComponent<MeshRenderer>();

        _targetColour = _inactiveColour;
    }

    private void OnEnable()
    {
        _meshRenderer.material.color = _inactiveColour;
        _thumbSlider.OnContactStart.AddListener(OnContactStart);
        _thumbSlider.OnContactEnd.AddListener(OnContactEnd);
    }

    private void OnDisable()
    {
        _thumbSlider.OnContactStart.RemoveListener(OnContactStart);
        _thumbSlider.OnContactEnd.RemoveListener(OnContactEnd);
    }

    private void OnContactStart(float value)
    {
        _targetColour = _activeColour;
    }
    
    private void OnContactEnd(float value)
    {
        _targetColour = _inactiveColour;
    }

    private void Update()
    {
        _meshRenderer.material.color = Color.Lerp(_meshRenderer.material.color, _targetColour, Time.deltaTime * _lerpSpeed);
    }
}
