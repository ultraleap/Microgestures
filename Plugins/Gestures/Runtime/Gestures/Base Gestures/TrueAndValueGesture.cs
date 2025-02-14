using Leap;
using Leap.Unity.GestureMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrueAndValueGesture : BaseGesture
{
    protected override void OnValidate()
    {
        base.OnValidate();

        while(childGestures.Count > 2 )
        {
            childGestures.RemoveAt(childGestures.Count - 1);
        }
    }

    protected override void StartTrackingFunction(Hand hand)
    {
    }

    protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
    {
        result = childGestures[0].result;
        value = childGestures[1].value;
    }

    protected override void StopTrackingFunction()
    {
    }
}
