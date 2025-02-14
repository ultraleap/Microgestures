using Leap.Unity.Preview.Locomotion;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    [RequireComponent(typeof(LightweightPinchDetector))]
    public class PinchGesture : BaseGesture
    {
        [SerializeField]
        private LightweightPinchDetector _lightweightPinchDetector = null;

        protected override void StartTrackingFunction(Hand hand)
        {
            _lightweightPinchDetector.leapProvider = gestureMachine.leapProvider;
            _lightweightPinchDetector.chirality = hand.GetChirality();
        }

        protected override void StopTrackingFunction()
        {
        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            result = _lightweightPinchDetector.IsPinching;
            value = result ? 1 : 0;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            _lightweightPinchDetector = GetComponent<LightweightPinchDetector>();
        }
    }
}