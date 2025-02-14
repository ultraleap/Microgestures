using Leap;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public class HandMovementGesture : DirectionalGesture
    {
        public float distance = 0.05f;

        private Vector3 _oldPosition, _positionCount = Vector3.zero;

        protected override void StartTrackingFunction(Hand hand)
        {
            _oldPosition = hand.PalmPosition;
            _positionCount = Vector3.zero;
        }

        protected override void StopTrackingFunction()
        {
        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            result = false;
            value = 0;
            if (childGestures[0].result)
            {
                _positionCount += hand.PalmPosition - _oldPosition;
                value = 0.5f;
            }
            else
            {
                int ind = -1;
                float val = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (Mathf.Abs(val) < Mathf.Abs(_positionCount[i]))
                    {
                        ind = i;
                        val = _positionCount[i];
                    }
                }
                if (ind != -1)
                {
                    switch (ind)
                    {
                        case 0:
                            resultDirection = val > 0 ? Direction.Right : Direction.Left;
                            break;
                        case 1:
                            resultDirection = val > 0 ? Direction.Up : Direction.Down;
                            break;
                        case 2:
                            resultDirection = val > 0 ? Direction.Forward : Direction.Backward;
                            break;
                    }
                    value = Mathf.InverseLerp(0, distance, Mathf.Abs(val));
                    result = Mathf.Abs(val) >= distance;
                    if (result)
                    {
                        Debug.Log(resultDirection);
                    }
                }
                _positionCount = Vector3.zero;
            }
            _oldPosition = hand.PalmPosition;
        }
    }
}