using Leap;
using Leap.Unity;   
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public class PalmFacingGesture : BaseGesture
    {
        public Vector3 direction = Vector3.up;
        public float dotValue = 0.5f;

        [Tooltip("Right dominant by default and will invert the X axis when a left hand is detected.")]
        public bool invertXForLeft = true;

#if UNITY_EDITOR
    private Vector3 _palmPosition, _palmNormal;
    private bool _result;
#endif

        protected override void StartTrackingFunction(Hand hand)
        {

        }

        protected override void StopTrackingFunction()
        {

        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            Vector3 testDirection = direction;
            if (invertXForLeft && hand.GetChirality() == Chirality.Left)
            {
                testDirection.x = -testDirection.x;
            }

            value = Vector3.Dot(hand.PalmNormal, testDirection);
            result = value > dotValue;

#if UNITY_EDITOR
        _palmPosition = hand.PalmPosition;
        _palmNormal = hand.PalmNormal;
        _result = result;
#endif
        }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if(Application.isPlaying)
        {
            Debug.DrawRay(_palmPosition, _palmNormal * .2f, _result ? Color.green : Color.red);
        }
        Debug.DrawRay(_palmPosition, direction * .2f, Color.yellow);
    }
#endif
    }
}