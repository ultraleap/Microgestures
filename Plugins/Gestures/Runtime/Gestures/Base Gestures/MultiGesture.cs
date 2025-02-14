using Leap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    /// <summary>
    /// Includes an option to hold the last element, without having to recode it each time
    /// </summary>
    public abstract class MultiGesture : BaseGesture
    {
        public bool holdIfLastTrue = false;

        protected override void ProcessOldResults()
        {
            if (holdIfLastTrue && _oldResult && childGestures[childGestures.Count - 1].result)
            {
                result = true;
                value = 1;
            }
            base.ProcessOldResults();
        }
    }
}