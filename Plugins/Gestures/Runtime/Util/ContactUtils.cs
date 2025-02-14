using Leap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public static class ContactUtils
    {
        public static Vector3 GetClosestPointOnFiniteLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 lineDir = lineEnd - lineStart;
            float lineLength = lineDir.magnitude;
            lineDir.Normalize();
            float projectLength = Mathf.Clamp(Vector3.Dot(point - lineStart, lineDir), 0f, lineLength);
            return lineStart + lineDir * projectLength;
        }

        public static Vector3 PointToHandLocal(Hand hand, Vector3 point)
        {
            return Quaternion.Inverse(hand.Rotation) * (point - hand.PalmPosition);
        }

        public static Vector3 InverseTransformPoint(Vector3 transforPos, Quaternion transformRotation, Vector3 pos)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(transforPos, transformRotation, Vector3.one);
            Matrix4x4 inverse = matrix.inverse;
            return inverse.MultiplyPoint3x4(pos);
        }

        public static Vector3 HandPointToWorld(Hand hand, Vector3 point)
        {
            return (hand.Rotation * point) + hand.PalmPosition;
        }

        public static Bone GetBone(this Hand hand, int index)
        {
            return hand.Fingers[index / 4].bones[index % 4];
        }
    }
}