using Leap;

namespace Leap.Unity.GestureMachine
{
    /// <summary>
    /// Returns true if all child gestures are true
    /// </summary>
    public class GroupGesture : MultiGesture
    {
        protected override void StartTrackingFunction(Hand hand)
        {

        }

        protected override void StopTrackingFunction()
        {

        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            int count = 0;
            foreach (var child in childGestures)
            {
                if (child.result)
                {
                    count++;
                }
            }

            result = count == childGestures.Count;
            value = (float)count / childGestures.Count;
        }
    }
}