using Leap;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public class ThumbPosCloseToIndexGesture : BaseGesture
    {
        private Bone _thumbBone, _indexBone;
        private Vector3 _posA, _posB, _midPoint;

        public float entryDistance = 0.02f, exitDistance = 0.03f;
        private bool _contacting = false;

        public bool useBoneWidth = true;

        public float DistanceToContact { get { return _distanceToContact; } }
        private float _distanceToContact;

        protected override void StartTrackingFunction(Hand hand)
        {
        }

        protected override void StopTrackingFunction()
        {
        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            _thumbBone = hand.Fingers[0].Bone(Bone.BoneType.TYPE_DISTAL);
            float tempDist;
            _distanceToContact = 1;
            for (int i = 1; i < hand.Fingers[1].bones.Length; i++)
            {
                _indexBone = hand.Fingers[1].bones[i];
                _posA = ContactUtils.GetClosestPointOnFiniteLine(_thumbBone.NextJoint, _indexBone.NextJoint, _indexBone.PrevJoint);
                _posB = ContactUtils.GetClosestPointOnFiniteLine(_indexBone.Center, _thumbBone.NextJoint, _thumbBone.Center);

                _midPoint = _posA + (_posB - _posA) / 2f;

                _posA = ContactUtils.GetClosestPointOnFiniteLine(_midPoint, _indexBone.NextJoint, _indexBone.PrevJoint);
                _posB = ContactUtils.GetClosestPointOnFiniteLine(_midPoint, _thumbBone.NextJoint, _thumbBone.Center);

                tempDist = Vector3.Distance(_posA, _posB);
                if (useBoneWidth) tempDist -= _indexBone.Width;

                if (tempDist < _distanceToContact)
                {
                    _distanceToContact = tempDist;
                }
            }

            if (_contacting)
            {
                if (_distanceToContact > exitDistance)
                {
                    _contacting = false;
                }
            }
            else
            {
                if (_distanceToContact < entryDistance)
                {
                    _contacting = true;
                }
            }
            result = _contacting;
            value = Mathf.InverseLerp(0.5f, entryDistance, _distanceToContact);
        }
    }
}