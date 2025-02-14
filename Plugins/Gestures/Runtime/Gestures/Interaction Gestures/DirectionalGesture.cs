using Leap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public abstract class DirectionalGesture : BaseGesture
    {
        [System.Serializable]
        public enum Direction
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3,
            Forward = 4,
            Backward = 5,
            None = 6
        }

        [System.Serializable]
        public enum Axis
        {
            UpDown = 0,
            LeftRight = 1,
            ForwardBackward = 2,
            None
        }

        public Direction resultDirection;

        public bool singleDirection = false;
        public Direction singleDirectionOption = Direction.Up;

        public bool singleAxis = false;
        public Axis singleAxisOption = Axis.UpDown;

        protected override void ProcessOldResults()
        {
            if (result && (singleAxis || singleDirection))
            {
                if (singleAxis && (((resultDirection == Direction.Left || resultDirection == Direction.Right) && singleAxisOption != Axis.LeftRight) ||
                    ((resultDirection == Direction.Up || resultDirection == Direction.Down) && singleAxisOption != Axis.UpDown) ||
                    ((resultDirection == Direction.Forward || resultDirection == Direction.Backward) && singleAxisOption != Axis.ForwardBackward)))
                {
                    result = false;
                    value = 0;
                }
                if (singleDirection && resultDirection != singleDirectionOption)
                {
                    result = false;
                    value = 0;
                }
            }
            base.ProcessOldResults();
        }
    }
}