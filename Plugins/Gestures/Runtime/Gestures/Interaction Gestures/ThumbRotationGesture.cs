using Leap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public class ThumbRotationGesture : DirectionalGesture
    {
        [Tooltip("Ensures consistent filtering rates.")]
        public int updateRate = 60;
        private float _updateDelta, _lastUpdateTime = 0;
        private bool _cachedResult = false;
        private float _cachedValue = 0f;

        [Tooltip("Rotates the local position by this amount.")]
        public Vector3 upRotation = new Vector3(0, 0, -90);

        [Tooltip("The velocity magnitude of the palm which should reduce the position changes of the filters.")]
        public Vector2 palmVelocityMinMax = new Vector2(0.2f, 0.5f);
        private float _currentVelocityLerp;
        private float _velocityLerpTime = 0.5f;

        [Tooltip("Prevent results from being processed for x amount of time when tracking starts.")]
        public float initialWindupTime = 0.25f;
        private float _intialWindupCurrent = 0f;

        public float interactionCooldown = 0.1f;
        private float _intCooldownCurrent = 0f;

        public FilteredAccelPoint filter = new FilteredAccelPoint();

        //[SerializeField, Tooltip("Used for debugging direction dot products.")]
        //private float _a, _b = -90;

        [Space]
        [SerializeField, Tooltip("Convert right handed, thumb pointing up based directions to another output.")]
        private Direction _leftOutput = Direction.Left;
        [SerializeField, Tooltip("Convert right handed, thumb pointing up based directions to another output.")]
        private Direction _rightOutput = Direction.Right;

        private Vector3 tipPosition;
        private Vector3 interPosition;


        public float deltaAngle, velRotDeltaAngle, deltaCurrentAngle;
        private Quaternion _previousVelRot, _currentRotation = Quaternion.identity, _outputRotation = Quaternion.identity;
        private Vector3 _previousVel;
        private bool _resetFrame = false;
        public float velocityMulti = 15f, flatVelMag, dotOut;
        public Vector3 dot = Vector3.down;
        [Tooltip("V3.up * degrees")]
        public float clockDot = 170, counterClockDot = 205;
        public float clockDotTest = 0.3f, counterClockDotTest = 0.3f;
        public float deltaMinimum = 0.8f;
        [Tooltip("x = left positive, y = right negative")]
        public Vector2 rotationRequirement = new Vector2(18f, -12f);
        public Vector2 rotationCuttoff = new Vector2(18f, -12f);
        public float currentRotation = 0f, highRotationDelta = 20f;
        public int lowFrames = 3;
        private int _lowFrameCount = 0;
        [Tooltip("Number of frames that are needed to count a swipe as a swipe.")]
        public int requiredCount = 2;
        [Tooltip("Maximum number of frames before the high rotation flicks are treated as normal")]
        public int highCountCuttoff = 8;
        private int _currentRequiredCount = 0, _highCount = 0;
        private bool _startedHigh = false;
        public Vector2 thumbVelocityMap = new Vector2(0.014f, 0.026f);
        public Vector2 palmVelocityMap = new Vector2(0.2f, 0.12f);

        private int _rollingIndex = 0;
        private float[] _rollingDelta = new float[30];
        private float[] _rollingAngle = new float[30];
        private float[] _rollingVelocity = new float[30];
        private bool _hasSwiped = false;
        public int inverseFrameCount = 0;
        public Vector2 leftRightAngles = new Vector2();
        public float leftRightAngle = 20f, leftRightVelocity = 0.035f, leftRightLowVelocity = 0.005f;

        public float resetTimer = 0.5f;

        public Action OnFlickReset;

        private void Awake()
        {
            filter.Awake();
        }

        protected override void StartTrackingFunction(Hand hand)
        {
            filter.Reset();
            _intialWindupCurrent = initialWindupTime;
            _resetFrame = true;
            _startedHigh = false;
            currentRotation = 0;
            _lowFrameCount = 0;
            _currentRequiredCount = 0;
        }

        protected override void StopTrackingFunction()
        {
        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            _updateDelta = 1f / updateRate;
            if (Time.time - _lastUpdateTime < _updateDelta)
            {
                result = _cachedResult;
                value = _cachedValue;
                return;
            }

            _lastUpdateTime = Time.time;

            if (_intialWindupCurrent > 0)
            {
                _intialWindupCurrent -= _updateDelta;
            }

            if (_intCooldownCurrent > 0)
            {
                _intCooldownCurrent -= _updateDelta;
            }

            result = false;

            // Hand velocity jitter
            float currentVel = Mathf.InverseLerp(palmVelocityMinMax.x, palmVelocityMinMax.y, hand.PalmVelocity.magnitude);
            _currentVelocityLerp = currentVel > _currentVelocityLerp ? currentVel : Mathf.Lerp(_currentVelocityLerp, currentVel, _updateDelta * (1f / _velocityLerpTime));

            // tip up -75, 95, 0
            // tip down -115, -45, 0
            // inter left 207, 0, -8
            // inter right 22, 0, -5

            Vector3 tipPosition = Quaternion.Euler(upRotation) * ContactUtils.PointToHandLocal(hand, hand.Fingers[0].TipPosition);

            this.tipPosition = ContactUtils.PointToHandLocal(hand, hand.Fingers[0].TipPosition);

            filter.Update(tipPosition, _updateDelta, _currentVelocityLerp);

            Vector3 tipPos = hand.Fingers[0].TipPosition, proxPos = hand.Fingers[0].bones[2].NextJoint;

            // Uncomment some of these to see some more debug Values

            //Debug.DrawRay(tipPos, hand.Direction, Color.cyan, Time.deltaTime / Time.timeScale);
            //Debug.DrawRay(tipPos, Quaternion.AngleAxis(_a, hand.Direction) * hand.PalmNormal, Color.magenta, Time.deltaTime / Time.timeScale);
            //Debug.DrawRay(tipPos, Quaternion.AngleAxis(_b, hand.PalmNormal) * hand.Direction, Color.yellow, Time.deltaTime / Time.timeScale);

            // This one can be used to debug a direction

            //Debug.DrawRay(tipPos, Quaternion.AngleAxis(_a, hand.Direction) * Quaternion.AngleAxis(_b, hand.PalmNormal) * hand.Direction, Color.red, _updateDelta);

            //Debug.DrawRay(tipPos + (Vector3.one * 0.001f), Quaternion.AngleAxis(90f, hand.PalmNormal) * hand.Direction * Mathf.Clamp(upFilter.axisVelFilter.currValue,0,100f), Color.cyan, _updateDelta);
            //Debug.DrawRay(tipPos + (Vector3.one * 0.001f), Quaternion.AngleAxis(90f, hand.PalmNormal) * hand.Direction * -Mathf.Clamp(downFilter.axisVelFilter.currValue, 0, 100f), Color.cyan, _updateDelta);
            //Debug.DrawRay(proxPos + (Vector3.one * 0.001f), Quaternion.AngleAxis(90f, hand.Direction) * Quaternion.AngleAxis(90f, hand.PalmNormal) * hand.Direction * Mathf.Clamp(leftFilter.axisVelFilter.currValue, 0, 100f), Color.yellow, _updateDelta);
            //Debug.DrawRay(proxPos + (Vector3.one * 0.001f), Quaternion.AngleAxis(90f, hand.Direction) * Quaternion.AngleAxis(90f, hand.PalmNormal) * hand.Direction * -Mathf.Clamp(rightFilter.axisVelFilter.currValue, 0, 100f), Color.yellow, _updateDelta);

            Debug.DrawRay(tipPos, filter.accelFilter.currValue, Color.blue, _updateDelta);
            Debug.DrawRay(tipPos, filter.velFilter.currValue, Color.cyan, _updateDelta);

            //Debug.DrawRay(tipPos, Quaternion.AngleAxis(-13.5f, hand.Direction) * Quaternion.AngleAxis(-101.5f, hand.PalmNormal) * hand.Direction, Color.green, _updateDelta);
            //Debug.DrawRay(tipPos, Quaternion.AngleAxis(-20f, hand.Direction) * Quaternion.AngleAxis(-125f, hand.PalmNormal) * hand.Direction * -1f, Color.green, _updateDelta);

            //Debug.DrawRay(proxPos, Quaternion.AngleAxis(60f, hand.Direction) * Quaternion.AngleAxis(-50f, hand.PalmNormal) * hand.Direction, Color.black, _updateDelta);
            //Debug.DrawRay(proxPos, (Quaternion.AngleAxis(60f, hand.Direction) * Quaternion.AngleAxis(-50f, hand.PalmNormal) * hand.Direction) * -1f, Color.black, _updateDelta);

            //Debug.DrawRay(tipPos, Vector3.Lerp(Vector3.left, Vector3.down, 0.2f) * .1f, Color.black, Time.deltaTime / Time.timeScale);
            //Debug.DrawRay(tipPos, Vector3.Lerp(Vector3.right, Vector3.down, 0.1f) * .1f, Color.black, Time.deltaTime / Time.timeScale);
            //Debug.DrawRay(tipPos, Vector3.down * .1f, Color.black, Time.deltaTime / Time.timeScale);

            // (Tip)
            // Take Z from -75, 90, 35 for up/down
            // Take Z from 35, 10, -5 for left/right

            // (inter)
            // Take Z from 35, 10, 15 for left/right

            Vector3 flatVelOutput = Vector3.Scale(filter.velFilter.currValue, new Vector3(1, 1, 0));
            Vector3 flatVelOutputOld = Vector3.Scale(filter.velFilter.prevValue, new Vector3(1, 1, 0));
            Vector3 flatAccelOutput = Vector3.Scale(filter.accelFilter.currValue, new Vector3(1, 1, 0));

            Quaternion velRot = Quaternion.FromToRotation(Vector3.up, flatVelOutput);
            Quaternion velRotPrev = Quaternion.FromToRotation(Vector3.up, flatVelOutputOld);

            Quaternion deltaVelRotation = Quaternion.Inverse(velRot) * _previousVelRot;
            velRotDeltaAngle = EulerToZeroAngle(deltaVelRotation.eulerAngles.z);
            
            float currentAngle = EulerToZeroAngle(velRot.eulerAngles.z);
            float prevAngle = EulerToZeroAngle(velRotPrev.eulerAngles.z);

            Quaternion accelRot = Quaternion.FromToRotation(Vector3.up, flatAccelOutput);

            Vector3 angularVel = GetAngularVelocity(_previousVelRot, velRot);

            if (_resetFrame)
            {
                _previousVelRot = velRot;
                _previousVel = flatVelOutput;
                _resetFrame = false;
            }

            //_currentVel = _currentVel + (angularVel * attractionForce * _updateDelta * (1f - (drag * _updateDelta)));
            //_currentAccel = _currentAccel + (angularAccel * attractionForce * _updateDelta * (1f - (drag * _updateDelta)));

            flatVelMag = (flatVelOutput - flatVelOutputOld).magnitude;

            angularVel *= Mathf.InverseLerp(thumbVelocityMap.x, thumbVelocityMap.y, flatVelMag);
            angularVel *= Mathf.InverseLerp(palmVelocityMap.x, palmVelocityMap.y, hand.PalmVelocity.magnitude);

            Quaternion oldRotation = _currentRotation;
            Quaternion velocityRotation = Quaternion.Euler(angularVel * _updateDelta * velocityMulti);

            _currentRotation *= velocityRotation;
            _currentRotation = Quaternion.Euler(0, 0, _currentRotation.eulerAngles.z);

            Debug.DrawRay(Vector3.right * 0.01f, _currentRotation * Vector3.up, Color.magenta, _updateDelta);

            Quaternion deltaRotation = Quaternion.Inverse(_currentRotation) * oldRotation;
            deltaAngle = EulerToZeroAngle(deltaRotation.eulerAngles.z);
            dot = Quaternion.Euler(0, 0, deltaAngle > 0 ? clockDot : counterClockDot) * Vector3.up;

            dotOut = Vector3.Dot(flatVelOutput.normalized, dot);

            if (deltaAngle > 0)
            {
                // Positive (clockwise)
            }
            else
            {
                // Negative (counter clockwise)
            }

            Debug.DrawRay(tipPosition, dot, Color.black);

            if (_intCooldownCurrent > 0 || _intialWindupCurrent > 0)
            {
                result = _cachedResult;
                value = _cachedValue;
                return;
            }

            value = 0;
            result = false;

            _rollingDelta[_rollingIndex] = velRotDeltaAngle;
            _rollingAngle[_rollingIndex] = EulerToZeroAngle(velRot.eulerAngles.z);
            _rollingVelocity[_rollingIndex] = Vector3.Distance(flatVelOutput, _previousVel);
            _rollingIndex++;
            if (_rollingIndex >= _rollingDelta.Length)
            {
                _rollingIndex = 0;
            }

            //int lowCount = 0;
            //for (int i = 0; i < _rollingDelta.Length - inverseFrameCount; i++)
            //{
            //    int index = (_rollingIndex + _rollingDelta.Length - i) % _rollingDelta.Length;
            //    int index2 = (_rollingIndex + _rollingDelta.Length - (i + inverseFrameCount)) % _rollingDelta.Length;

            //    if (_rollingVelocity[index] < leftRightLowVelocity)
            //    {
            //        lowCount++;
            //    }

            //    if (_rollingVelocity[index] > leftRightVelocity)
            //    {
            //        if (lowCount > lowFrames)
            //        {
            //            if (CheckRangeValue(leftRightAngles.x, leftRightAngle, _rollingAngle[index]))
            //            {
            //                // left angle
            //                if (CheckRangeValue(leftRightAngles.y, leftRightAngle, _rollingAngle[index2]))
            //                {
            //                    result = true;
            //                    resultDirection = Direction.Left;
            //                    value = -1;
            //                }
            //            }
            //            if (CheckRangeValue(leftRightAngles.y, leftRightAngle, _rollingAngle[index]))
            //            {
            //                // right angle
            //                if (CheckRangeValue(leftRightAngles.x, leftRightAngle, _rollingAngle[index2]))
            //                {
            //                    result = true;
            //                    resultDirection = Direction.Right;
            //                    value = 1;
            //                }
            //            }
            //        }
            //    }

            //    if (result)
            //    {
            //        for (int j = 0; j < _rollingDelta.Length; j++)
            //        {
            //            _rollingDelta[j] = 0f;
            //            _rollingAngle[j] = 0f;
            //            _rollingVelocity[j] = 0f;
            //        }
            //        Debug.Log("sharp");
            //        _cachedResult = result;
            //        _cachedValue = value;
            //        _intCooldownCurrent = interactionCooldown;
            //        return;
            //    }
            //}

            if (_startedHigh)
            {
                _highCount++;
            }

            if (!_startedHigh && Mathf.Abs(deltaAngle) > highRotationDelta && flatVelOutputOld.magnitude > thumbVelocityMap.x && _currentRequiredCount == 0 && (CheckRangeValue(leftRightAngles.x, leftRightAngle, prevAngle) || CheckRangeValue(leftRightAngles.y, leftRightAngle, prevAngle)))
            {
                //Debug.Log("high start!");
                _startedHigh = true;
                _highCount = 0;
                _currentRequiredCount = -1;
            }

            if (Mathf.Abs(deltaAngle) > deltaMinimum && (dotOut > (deltaAngle > 0 ? clockDotTest : counterClockDotTest) || (_startedHigh && _highCount < highCountCuttoff)))
            {
                if (((deltaAngle > rotationCuttoff.x || deltaAngle < rotationCuttoff.y) && _startedHigh && _highCount < highCountCuttoff) || ((deltaAngle * .5f) < rotationCuttoff.x && (deltaAngle * .5f) > rotationCuttoff.y))
                {
                    Quaternion oldOutRotation = _outputRotation;
                    Quaternion newRotation = _outputRotation * velocityRotation;
                    newRotation = Quaternion.Euler(0, 0, newRotation.eulerAngles.z);

                    Quaternion deltaOutputRotation = Quaternion.Inverse(newRotation) * oldOutRotation;
                    float deltaOutputAngle = EulerToZeroAngle(deltaOutputRotation.eulerAngles.z);

                    if (deltaOutputAngle > rotationCuttoff.x || deltaOutputAngle < rotationCuttoff.y)
                    {
                        deltaOutputAngle *= .5f;
                    }

                    deltaCurrentAngle = deltaOutputAngle;
                    currentRotation += deltaOutputAngle;
                    _outputRotation = newRotation;

                    _currentRequiredCount++;

                    _lowFrameCount = 0;

                    result = CheckHasSwiped(ratio: 1.25f, countAddition: 1);
                    if (result)
                    {
                        //Debug.Log("early swipe");
                    }
                }
                else
                {
                    //Debug.Log("too high!");
                }
            }
            else if (_currentRequiredCount > 0)
            {
                _lowFrameCount++;
            }

            value = Mathf.InverseLerp(rotationRequirement.y, rotationRequirement.x, Mathf.Abs(currentRotation));
            value = Mathf.Lerp(-1, 1, value);

            if (_lowFrameCount >= lowFrames)
            {
                result = CheckHasSwiped();

                _hasSwiped = false;
                //Debug.Log("Out: "+ currentRotation / _currentRequiredCount + " Rotation: " + currentRotation + " RC: " + _currentRequiredCount);

                _intCooldownCurrent = interactionCooldown;
                _lowFrameCount = 0;
                currentRotation = 0;
                _currentRequiredCount = 0;
                
                _startedHigh = false;
                for (int j = 0; j < _rollingDelta.Length; j++)
                {
                    _rollingDelta[j] = 0f;
                    _rollingAngle[j] = 0f;
                    _rollingVelocity[j] = 0f;
                }
            }

            _cachedResult = result;
            _cachedValue = value;

            transform.rotation = _outputRotation;

            _previousVelRot = velRot;
            _previousVel = flatVelOutput;
        }

        public override void ResetValues()
        {
            _intCooldownCurrent = interactionCooldown;
            _lowFrameCount = 0;
            currentRotation = 0;
            _currentRequiredCount = 0;
            for (int j = 0; j < _rollingDelta.Length; j++)
            {
                _rollingDelta[j] = 0f;
                _rollingAngle[j] = 0f;
                _rollingVelocity[j] = 0f;
            }
        }

        private bool CheckHasSwiped(float ratio = 1.0f, int countAddition = 0)
        {
            if (!_hasSwiped && (currentRotation > rotationRequirement.x * ratio || currentRotation < rotationRequirement.y * ratio) && (_currentRequiredCount > requiredCount + countAddition || (_startedHigh && _highCount < highCountCuttoff)))
            {
                if (_startedHigh)
                {
                    //Debug.Log("completed high!");
                }
                resultDirection = currentRotation > 0 ? ConvertDirection(Direction.Left) : ConvertDirection(Direction.Right);
                _hasSwiped = true;
                return true;
            }
            return false;
        }

        private float EulerToZeroAngle(float input)
        {
            return Mathf.Repeat(input + 180f, 360f) - 180f;
        }

        private bool CheckRangeValue(float value, float range, float test)
        {
            return value - range <= test && test <= value + range;
        }

        private Vector3 GetAngularVelocity(Quaternion foreLastFrameRotation, Quaternion lastFrameRotation)
        {
            var q = lastFrameRotation * Quaternion.Inverse(foreLastFrameRotation);
            if (Mathf.Abs(q.w) > 1023.5f / 1024.0f)
                return new Vector3(0, 0, 0);
            float gain;
            // handle negatives, we could just flip it but this is faster
            if (q.w < 0.0f)
            {
                var angle = Mathf.Acos(-q.w);
                gain = -2.0f * angle / (Mathf.Sin(angle) * Time.deltaTime);
            }
            else
            {
                var angle = Mathf.Acos(q.w);
                gain = 2.0f * angle / (Mathf.Sin(angle) * Time.deltaTime);
            }
            return new Vector3(q.x * gain, q.y * gain, q.z * gain);
        }

        private void ProcessResult(Direction direction, out bool result, out float value)
        {
            resultDirection = direction;
            result = true;
            value = 1;
            _intCooldownCurrent = interactionCooldown;
        }

        private Direction CalculateDirection(int index)
        {
            switch (index)
            {
                case 0: return Direction.Up;
                case 1: return Direction.Down;
                case 2: return Direction.Left;
                case 3: return Direction.Right;
            }
            return Direction.Up;
        }

        private Direction ConvertDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Right:
                    return _rightOutput;
                case Direction.Left:
                    return _leftOutput;
            }
            return direction;
        }

        /// <summary>
        /// Use this function to discern the output of a dot product
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="direction"></param>
        /// <param name="hand"></param>
        private void ProcessDots(string prefix, Vector3 direction, Hand hand)
        {
            string output = prefix;

            //output += $" F/B {Vector3.Dot(hand.Direction, direction)}";
            //output += $" U/D {Vector3.Dot(Quaternion.AngleAxis(_a, hand.Direction) * hand.PalmNormal, direction)}";
            //output += $" L/R {Vector3.Dot(Quaternion.AngleAxis(_b, hand.PalmNormal) * hand.Direction, direction)}";

            // Some nice values that work quite well

            // Tip
            // up (+) -18.5, -103.5
            // down (-) -20 -125
            // left/right (-/+) 100, 92

            // Prox
            // up/down (+/-) -20, -75
            // left/right (-/+) 100, 92

            //output += $" Combo {Vector3.Dot(Quaternion.AngleAxis(_a, hand.Direction) * Quaternion.AngleAxis(_b, hand.PalmNormal) * hand.Direction, hand.Rotation * direction)}";
            //Debug.Log(output);
        }

        private void OnDrawGizmos()
        {
            Debug.DrawRay(Vector3.zero, Quaternion.Euler(0, 0, clockDot) * Vector3.up, Color.red);
            Debug.DrawRay(Vector3.zero, Quaternion.Euler(0, 0, counterClockDot) * Vector3.up, Color.blue);
            if (Application.isPlaying && isTracked)
            {
                //Gizmos.color = Color.gray;
                //Gizmos.DrawSphere(tipFilter.pointFilter.currValue, 0.01f);
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(tipPosition, 0.01f);
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(interPosition, 0.01f);
            }
        }
    }
}