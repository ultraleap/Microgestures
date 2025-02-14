using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeObjectRotator : MonoBehaviour
{
    public GameObject rotableObject;
    public InertiaGenerator inertiaGenerator;
    public float rotationSpeed = 1;

    public enum TransformAxis { X, Y, Z }
    public TransformAxis axis = TransformAxis.Y;
    public bool localSpace = false;

    [SerializeField] private float _minSpeed = 0.01f;

    private SwipeDirection _dir = SwipeDirection.LEFT;

    private void Update()
    {
        Quaternion rotation = Quaternion.identity;
        float rotToAdd = -inertiaGenerator.Velocity * Time.deltaTime * rotationSpeed;
        if (inertiaGenerator.System.State != Leap.Unity.GestureMachine.MicrogestureSystem.MicrogestureState.CONTACTING && (
            rotToAdd >= 0 && rotToAdd <= _minSpeed ||
            rotToAdd < 0 && rotToAdd >= -_minSpeed))
        {
            rotToAdd = _dir == SwipeDirection.LEFT ? _minSpeed : -_minSpeed;
        }
        else
        {
            _dir = rotToAdd >= 0 ? SwipeDirection.LEFT : SwipeDirection.RIGHT;
        }
        switch (axis)
        {
            case TransformAxis.X:
                rotation = Quaternion.Euler(rotToAdd, 0, 0);
                break;
            case TransformAxis.Y:
                rotation = Quaternion.Euler(0, rotToAdd, 0);
                break;
            case TransformAxis.Z:
                rotation = Quaternion.Euler(0, 0, rotToAdd);
                break;
        }

        Quaternion newRotation = Quaternion.identity;
        if (localSpace)
        {
            newRotation = rotableObject.transform.localRotation * rotation;
            rotableObject.transform.localRotation = newRotation;
        }
        else
        {
            newRotation = rotableObject.transform.rotation * rotation;
            rotableObject.transform.rotation = newRotation;
        }
    }
}
