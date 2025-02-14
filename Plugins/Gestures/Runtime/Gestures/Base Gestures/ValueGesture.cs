using Leap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public class ValueGesture : BaseGesture
    {
        public float min = 0f, max = 1f, threshold = 0.5f;

        protected override void StartTrackingFunction(Hand hand)
        {

        }

        protected override void StopTrackingFunction()
        {
        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            value = Mathf.InverseLerp(min, max, childGestures[0].value);
            result = value > threshold;
        }
    }
}