using Leap;
using Leap.Unity.GestureMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class TrueAndValueDirectionGesture : DirectionalGesture
{
    DirectionalGesture _directionalChild;
    BaseGesture _baseGesture;

    [Tooltip("Ideally set this to less than 1 so that a true direction value will report as 1.")]
    public Vector2 directionalValueRemap = new Vector2(-0.99999f, 0.99999f);

    protected override void OnValidate()
    {
        base.OnValidate();

        while (childGestures.Count > 2)
        {
            childGestures.RemoveAt(childGestures.Count - 1);
        }
        while (childGestures.Count < 2)
        {
            childGestures.Add(null);
        }
    }

    protected override void StartTrackingFunction(Hand hand)
    {
        int i = childGestures.IndexOf(childGestures.FirstOrDefault(o => o is DirectionalGesture));
        _directionalChild = (DirectionalGesture)childGestures[i];
        _baseGesture = childGestures[i == 0 ? 1 : 0];
    }

    protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
    {
        result = _baseGesture.result;
        value = _directionalChild.result ? _directionalChild.value : map(_directionalChild.value, -1, 1, directionalValueRemap.x, directionalValueRemap.y);
        resultDirection = _directionalChild.resultDirection;
    }

    protected override void StopTrackingFunction()
    {
    }

    private float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}
