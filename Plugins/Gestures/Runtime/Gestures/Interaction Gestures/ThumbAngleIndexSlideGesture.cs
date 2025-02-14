using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Leap.Unity.GestureMachine
{
    public class ThumbAngleIndexSlideGesture : BaseGesture
    {
        public float minAngle = -5f;
        public float maxAngle = 50f;

        protected override void StartTrackingFunction(Hand hand)
        {
        }

        protected override void StopTrackingFunction()
        {
        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            result = true;

            Quaternion thumbInterRotationLocal = Quaternion.Inverse(hand.Rotation) * hand.GetThumb().bones[2].Rotation;
            value = RotationalSlide(thumbInterRotationLocal, hand.IsLeft);
        }

        protected float RotationalSlide(Quaternion thumbInterRotationLocal, bool isLeft)
        {
            var sliderValueNew = thumbInterRotationLocal.eulerAngles.x;
            if (sliderValueNew > 180)
                sliderValueNew -= 360;

            //L: -5 to 50, R:50 to -5
            return InverseLerpUnclamped(isLeft ? minAngle : maxAngle, isLeft ? maxAngle : minAngle, sliderValueNew);
        }
        protected float InverseLerpUnclamped(float a, float b, float value)
        {
            return (value - a) / (b - a);
        }

        public bool drawDebugLines = false;
        public float debugAxisLength = 0.05f;
        private void DrawAxis(Vector3 origin, Quaternion rotation)
        {
            Debug.DrawLine(origin, origin + rotation * Vector3.forward * debugAxisLength, Color.blue);
            Debug.DrawLine(origin, origin + rotation * Vector3.up * debugAxisLength, Color.green);
            Debug.DrawLine(origin, origin + rotation * Vector3.left * debugAxisLength, Color.red);
        }
    }
}