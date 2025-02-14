using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public class ThumbAngleCloseToIndexGesture : BaseGesture
    {
        public float thumbRotationEntryRotation = 10f;
        public float thumbRotationExitRotation = 12f;

        public float minSlideAngle = -5f;
        public float maxSlideAngle = 50f;

        private float AngleBoundary 
        { 
            get
            {
                return _contacting ? thumbRotationExitRotation : thumbRotationEntryRotation;
            } 
        }

        public bool drawDebugLines = false;
        public float debugAxisLength = 0.05f;

        private bool _contacting = false;
        public float DistanceToAngleBoundary { get; private set; }

        protected override void StartTrackingFunction(Hand hand)
        {
        }

        protected override void StopTrackingFunction()
        {
        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            // Get the rotation of the index based on the knuckle and index tip
            Quaternion indexRotation = Quaternion.LookRotation
                (
                    hand.GetIndex().Bone(Bone.BoneType.TYPE_METACARPAL).NextJoint -
                    hand.GetIndex().TipPosition,
                     hand.Rotation * (hand.IsLeft ? Vector3.right : Vector3.left)
                );

            //Orient the rotation so that the up is facing the opposite way to the middle finger
            indexRotation *= Quaternion.Euler(Vector3.up * (hand.IsLeft ? 90 : -90));


            // Get the thumb rotation as the following
            // forward: from the centre of the proximal, to the tip of the thumb. This means that the rotation is fairly stable no matter the bend of the thumb
            Vector3 tip = hand.GetThumb().TipPosition;
            Vector3 proximal = hand.GetThumb().Bone(Bone.BoneType.TYPE_PROXIMAL).Center;
            Vector3 thumbForward = tip - proximal;

            // up: the same up vector as the index rotation
            Vector3 indexUp = indexRotation * Vector3.up;

            Quaternion knuckleTipRotation = Quaternion.LookRotation(thumbForward, indexUp);

            // Get the thumb rotation in the index axis's local space
            Quaternion thumbRotationInLocalIndexAxis = Quaternion.Inverse(indexRotation) * knuckleTipRotation;


            // Change the angles from a range of 0->360, to a range of -180->180
            simplifiedEuler = thumbRotationInLocalIndexAxis.eulerAngles;
            simplifiedEuler = new Vector3(SimplifyAngle(simplifiedEuler.x) * -1, SimplifyAngle(simplifiedEuler.y) * -1, SimplifyAngle(simplifiedEuler.z));

            if (drawDebugLines)
            {
                // Index Axis
                Debug.DrawLine(hand.GetIndex().Bone(Bone.BoneType.TYPE_METACARPAL).NextJoint, hand.GetIndex().TipPosition);
                DrawAxis(hand.GetIndex().bones[1].PrevJoint, indexRotation);

                // Thumb Axis
                Debug.DrawLine(proximal, tip);
                DrawAxis(Vector3.Lerp(tip, proximal, 0.5f), knuckleTipRotation);
            }

            float rotationValue = simplifiedEuler.x;

            DistanceToAngleBoundary = Mathf.InverseLerp(AngleBoundary, 35, rotationValue);

            // If the x value of our thumb rotation is close to the "angle boundary", then it is fairly aligned with our constructed index "up", and therefore contacting
            _contacting = (rotationValue < AngleBoundary);
            
            result = _contacting;
            value = rotationValue;
        }
        public Vector3 simplifiedEuler;
        /// <summary>
        /// Change the range of an angle from 0->360 to -180->180
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private float SimplifyAngle(float angle)
        {
            angle = angle % 360;

            if (angle > 180)
            {
                angle -= 360;
            }
            else if (angle < -180)
            {
                angle += 360;
            }
            return angle;
        }

        protected float InverseLerpUnclamped(float a, float b, float value)
        {
            return (value - a) / (b - a);
        }

        private void DrawAxis(Vector3 origin, Quaternion rotation)
        {
            Debug.DrawLine(origin, origin + rotation * Vector3.forward * debugAxisLength, Color.blue);
            Debug.DrawLine(origin, origin + rotation * Vector3.up * debugAxisLength, Color.green);
            Debug.DrawLine(origin, origin + rotation * Vector3.left * debugAxisLength, Color.red);
        }
    }
}
