using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class MgHinting : MonoBehaviour
{
    [SerializeField] private GameObject _ui;

    private const string _microgesturesHint = "microgestures";

    private void Start()
    {
        HandTrackingHintManager.RequestHandTrackingHints(new string[] { _microgesturesHint });
        Destroy(_ui);
    }
}
