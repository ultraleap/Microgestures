using Leap;
using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public class FingerCurlGesture : BaseGesture
    {
        public int fingerIndex = 1;

        public float curlAmount = 0.5f;

        protected override void StartTrackingFunction(Hand hand)
        {
        }

        protected override void StopTrackingFunction()
        {
        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            float val = hand.GetFingerStrength(fingerIndex);

            result = val > curlAmount;
            value = Mathf.InverseLerp(0, curlAmount, val);
        }
    }
}