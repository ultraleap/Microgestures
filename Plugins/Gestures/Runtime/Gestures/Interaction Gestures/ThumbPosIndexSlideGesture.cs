using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public class ThumbPosIndexSlideGesture : BaseGesture
    {
        private Vector3[] _points = new Vector3[3];
        private float[] _values = new float[3];
        private float[] _lengths = new float[3];

        [System.Serializable, Tooltip("Linear just adds all 3 vals then divides by 3. Length scales by bone lenght. Custom scales based on the individual components vs the total of the XYZ.")]
        public enum ScaleType
        {
            Linear,
            Length,
            Custom
        }

        [SerializeField]
        private ScaleType _scaleType = ScaleType.Length;

        [SerializeField]
        private Vector3 _customScale = Vector3.one;
        private float _customScaleTotal = 0;

        [SerializeField]
        private Vector2 _inverseLerp = new Vector2(0, 1);
        private float _totalLength;

        protected override void StartTrackingFunction(Hand hand)
        {
            UpdateLengths(hand);
        }

        protected override void StopTrackingFunction()
        {
        }

        private void UpdateLengths(Hand hand)
        {
            _totalLength = 0;
            for (int i = 1; i < hand.Fingers[1].bones.Length; i++)
            {
                Bone b = hand.Fingers[1].bones[i];
                _lengths[i - 1] = Vector3.Distance(b.PrevJoint, b.NextJoint);
                _totalLength += _lengths[i - 1];
            }
            UpdateHandFunction(hand, out _, out _);
        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            result = true;
            float slideVal = 0;

            for (int i = 1; i < hand.Fingers[1].bones.Length; i++)
            {
                Bone b = hand.Fingers[1].bones[i];
                Vector3 nextPos = b.Type == Bone.BoneType.TYPE_DISTAL ? (b.NextJoint + (b.Direction * b.Width)) : b.NextJoint;

                _points[i - 1] = ContactUtils.GetClosestPointOnFiniteLine(hand.Fingers[0].TipPosition, nextPos, b.PrevJoint);
                float dist = Vector3.Distance(_points[i - 1], b.PrevJoint);
                _values[i - 1] = Mathf.InverseLerp(0, Vector3.Distance(nextPos, b.PrevJoint), dist);

            }

            _customScaleTotal = _customScale.x + _customScale.y + _customScale.z;

            for (int i = 0; i < 3; i++)
            {
                switch (_scaleType)
                {
                    case ScaleType.Linear:
                        slideVal += _values[i] / 3f;
                        break;
                    case ScaleType.Length:
                        // Scale the values of the slide depening on the bone length
                        slideVal += _values[i] * (_lengths[i] / _totalLength);
                        break;
                    case ScaleType.Custom:
                        slideVal += _values[i] * (_customScale[i] / _customScaleTotal);
                        break;
                    default:
                        break;
                }
            }

            slideVal = Mathf.InverseLerp(_inverseLerp.x, _inverseLerp.y, slideVal);

            if (hand.IsRight)
            {
                slideVal = 1 - slideVal;
            }
            value = slideVal;
        }
    }
}