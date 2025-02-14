using Leap;
using Leap.Unity.GestureMachine;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalmFacingCameraGesture : BaseGesture
{
    [SerializeField]
    private float _minAllowedDotProduct = -0.4f;
    
    private Transform _palmTransform;
    protected override void StartTrackingFunction(Hand hand)
    {
    }

    protected override void StopTrackingFunction()
    {
    }

    protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
    {
        _palmTransform.position = hand.PalmPosition;
        _palmTransform.rotation = hand.Rotation;

        float dotProd = Vector3.Dot((Camera.main.transform.position - _palmTransform.position).normalized, -_palmTransform.up);

        bool isFacingCamera = dotProd > _minAllowedDotProduct;

        result = isFacingCamera;
        value = dotProd;
    }

    // Start is called before the first frame update
    void Start()
    {
        _palmTransform = new GameObject("PalmTransform").transform;
        _palmTransform.parent = transform;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
