using Leap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    public class ThumbFlickGesture : DirectionalGesture
    {
        [Tooltip("Ensures consistent filtering rates.")]
        public int updateRate = 60;
        private float _updateDelta, _lastUpdateTime = 0;
        private bool _cachedResult = false;
        private float _cachedValue = 0f;

        [Tooltip("Rotates the local position by this amount to use the Z position for filters.")]
        public Vector3 upRotation = new Vector3(-75, 95, 3), downRotation = new Vector3(-115, -45, -40),
            leftRotation = new Vector3(220, 0, 8), rightRotation = new Vector3(18, 0, 28);
        [Tooltip("Up, down, left, right")]
        public Vector4 directionScale = Vector4.one;

        [Tooltip("The velocity magnitude of the palm which should reduce the position changes of the filters.")]
        public Vector2 palmVelocityMinMax;
        [SerializeField]
        private float _currentVelocityLerp;
        [SerializeField]
        private float _velocityLerpTime = 0.5f;
        [SerializeField]
        private float _currentPosDelta = 0f, _posDeltaLerp = 0f, _posDeltaLerpTime = 0.1f, _currentSquareVelDelta;

        [SerializeField]
        private Vector2 _deltaRemap = new Vector2(0, 0.0015f);

        [Tooltip("Prevent results from being processed for x amount of time when tracking starts.")]
        public float initialWindupTime = 0f;
        private float _intialWindupCurrent = 0f;

        public float interactionCooldown = 0f;
        private float _intCooldownCurrent = 0f;

        public float interactionTimer = 0f;

        [Tooltip("Requires a low magnitude of acceleration to be reported before another flick can be attempted.")]
        public bool requireLowMagnitudeRepeat = true;
        private bool _magCooldown = false;
        public float magCooldownTime = 0.012f;
        private float _currentMagCooldownTime = 0f;
        private float _prevSquareVelDelta = 1;

        public bool requireFingerCurl = true;
        public float fingerCurlAmount = 0.5f;
        public float fingerCurlLerpTime = 0.1f;
        private float _currentFingerCurl = 0f;

        public FilteredAccelAxis upFilter = new FilteredAccelAxis(), downFilter = new FilteredAccelAxis();
        public FilteredAccelAxis leftFilter = new FilteredAccelAxis(), rightFilter = new FilteredAccelAxis();

        [SerializeField, Tooltip("Used for debugging direction dot products.")]
        private float _a, _b = -90;

        public float[] directions = new float[4];
        private float[] _directionTimes = new float[4];

        public bool upAllowed = false, downAllowed = false, leftAllowed = false, rightAllowed = false;

        [Space]
        [SerializeField, Tooltip("Convert right handed, thumb pointing up based directions to another output.")]
        private Direction _upOutput = Direction.Up;
        [SerializeField, Tooltip("Convert right handed, thumb pointing up based directions to another output.")]
        private Direction _downOutput = Direction.Down, _leftOutput = Direction.Left, _rightOutput = Direction.Right;

        public float velMag = 0.2f, proxVelMag = 0.2f, diffMag = 10f, proxDiffMag = 5f;
        public float magA = 0.5f, magB = 0.25f;

        private float upDownVelAbs, leftRightVelAbs;

        private Vector3 tipPosition;
        private Vector3 interPosition;

        private bool _needReset = false;
        public Action OnFlickReset;

        private void Awake()
        {
            upFilter.Awake();
            downFilter.Awake();
            leftFilter.Awake();
            rightFilter.Awake();
            singleAxis = true;
        }

        protected override void StartTrackingFunction(Hand hand)
        {
            upFilter.Reset();
            downFilter.Reset();
            leftFilter.Reset();
            rightFilter.Reset();
            _intialWindupCurrent = initialWindupTime;
            _magCooldown = false;
            singleAxis = true;
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
            value = 0;

            // Hand velocity jitter
            float currentVel = Mathf.InverseLerp(palmVelocityMinMax.x, palmVelocityMinMax.y, hand.PalmVelocity.magnitude);
            _currentVelocityLerp = currentVel > _currentVelocityLerp ? currentVel : Mathf.Lerp(_currentVelocityLerp, currentVel, _updateDelta * (1f / _velocityLerpTime));

            // tip up -75, 95, 0
            // tip down -115, -45, 0
            // inter left 207, 0, -8
            // inter right 22, 0, -5

            Vector3 tipPosition = ContactUtils.PointToHandLocal(hand, hand.Fingers[0].TipPosition);
            Vector3 tipPositionUp = Quaternion.Euler(upRotation) * tipPosition;
            Vector3 tipPositionDown = Quaternion.Euler(downRotation) * tipPosition;

            // Prox filter is actually using the intermediate, it's a bit more expressive.
            Vector3 interPosition = ContactUtils.PointToHandLocal(hand, hand.Fingers[0].bones[2].NextJoint);
            Vector3 interPositionLeft = Quaternion.Euler(leftRotation) * interPosition;
            Vector3 interPositionRight = Quaternion.Euler(rightRotation) * interPosition;

            this.tipPosition = ContactUtils.PointToHandLocal(hand, hand.Fingers[0].TipPosition);
            this.interPosition = ContactUtils.PointToHandLocal(hand, hand.Fingers[0].bones[2].NextJoint);

            upFilter.Update(tipPositionUp.z * directionScale.x, _updateDelta, _currentVelocityLerp);

            downFilter.Update(tipPositionDown.z * directionScale.y, _updateDelta, _currentVelocityLerp);

            leftFilter.Update(interPositionLeft.z * directionScale.z, _updateDelta, _currentVelocityLerp);

            rightFilter.Update(interPositionRight.z * directionScale.w, _updateDelta, _currentVelocityLerp);

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

            Debug.DrawRay(Vector3.right * 0.01f, Vector3.up * upFilter.axisVelFilter.currValue, Color.cyan, _updateDelta);
            Debug.DrawRay(Vector3.right * 0.02f, Vector3.up * downFilter.axisVelFilter.currValue, Color.blue, _updateDelta);
            Debug.DrawRay(Vector3.right * 0.03f, Vector3.up * leftFilter.axisVelFilter.currValue, Color.yellow, _updateDelta);
            Debug.DrawRay(Vector3.right * 0.04f, Vector3.up * rightFilter.axisVelFilter.currValue, Color.red, _updateDelta);

            Debug.DrawRay(tipPos, Quaternion.AngleAxis(90f, hand.PalmNormal) * hand.Direction * Mathf.Clamp(upFilter.axisAccelFilter.currValue, 0, 100f), Color.blue, _updateDelta);
            Debug.DrawRay(tipPos, Quaternion.AngleAxis(90f, hand.PalmNormal) * hand.Direction * -Mathf.Clamp(downFilter.axisAccelFilter.currValue, 0, 100f), Color.blue, _updateDelta);
            Debug.DrawRay(proxPos, Quaternion.AngleAxis(90f, hand.Direction) * Quaternion.AngleAxis(90f, hand.PalmNormal) * hand.Direction * Mathf.Clamp(leftFilter.axisAccelFilter.currValue, 0, 100f), Color.red, _updateDelta);
            Debug.DrawRay(proxPos, Quaternion.AngleAxis(90f, hand.Direction) * Quaternion.AngleAxis(90f, hand.PalmNormal) * hand.Direction * -Mathf.Clamp(rightFilter.axisAccelFilter.currValue, 0, 100f), Color.red, _updateDelta);

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

            if (upFilter.axisVelFilter.currValue > velMag * .5f || downFilter.axisVelFilter.currValue > velMag * .5f ||
                leftFilter.axisVelFilter.currValue > proxVelMag * .5f || rightFilter.axisVelFilter.currValue > proxVelMag * .5f)
            {
                value = 0.5f;
            }

            float maxUpDownVel = Mathf.Max(upFilter.axisVelFilter.currValue, downFilter.axisVelFilter.currValue);
            float maxUpDownAccel = Mathf.Max(upFilter.axisAccelFilter.currValue, downFilter.axisAccelFilter.currValue);
            float maxLeftRightVel = Mathf.Max(leftFilter.axisVelFilter.currValue, rightFilter.axisVelFilter.currValue);
            float maxLeftRightAccel = Mathf.Max(leftFilter.axisAccelFilter.currValue, rightFilter.axisAccelFilter.currValue);

            upDownVelAbs = Mathf.Abs(maxUpDownVel);
            leftRightVelAbs = Mathf.Abs(maxLeftRightVel);

            Vector2 aPos = new Vector2(maxUpDownVel, maxLeftRightVel);
            Vector2 bPos = new Vector2(Mathf.Max(upFilter.axisVelFilter.prevValue, downFilter.axisVelFilter.prevValue), Mathf.Max(leftFilter.axisVelFilter.prevValue, rightFilter.axisVelFilter.prevValue));

            _currentPosDelta = Mathf.InverseLerp(_deltaRemap.x, _deltaRemap.y, Vector2.Distance(aPos, bPos));
            _posDeltaLerp = Mathf.Lerp(_posDeltaLerp, _currentPosDelta, _updateDelta * (1f / _posDeltaLerpTime));

            _currentSquareVelDelta = _posDeltaLerp * _posDeltaLerp;

            _cachedResult = result;
            _cachedValue = value;

            if (requireFingerCurl)
            {
                _currentFingerCurl = Mathf.Lerp(_currentFingerCurl, hand.GetFingerStrength(1), _updateDelta * (1f / fingerCurlLerpTime));
            }
            else
            {
                _currentFingerCurl = 1f;
            }

            if (_intialWindupCurrent > 0 || _intCooldownCurrent > 0)
            {
                return;
            }

            // Enforce a cooldown if we've got some spicy hand movement
            if (_currentVelocityLerp > 0.5f)
            {
                _magCooldown = true;
                _currentMagCooldownTime = magCooldownTime;
            }

            if (_magCooldown)
            {
                if (_prevSquareVelDelta * magA > _currentSquareVelDelta /* && _prevVelMag * magB > _tipVelAverage*/)
                {
                    _currentMagCooldownTime -= _updateDelta;
                    if (_currentMagCooldownTime < 0)
                    {
                        _magCooldown = false;
                    }
                }
                else
                {
                    if (_currentMagCooldownTime < magCooldownTime)
                    {
                        _currentMagCooldownTime += _updateDelta;
                        if (_currentMagCooldownTime > magCooldownTime)
                        {
                            _currentMagCooldownTime = magCooldownTime;
                        }
                    }
                }
            }

            if (_magCooldown || _currentVelocityLerp > 0.5f || _currentFingerCurl < fingerCurlAmount)
            {
                for (int i = 0; i < _directionTimes.Length; i++)
                {
                    _directionTimes[i] = 0f;
                }
                return;
            }
            else
            {
                if (_needReset)
                {
                    _needReset = false;
                    OnFlickReset?.Invoke();
                }
            }

            for (int i = 0; i < directions.Length; i++)
            {
                directions[i] = -1;
            }

            upAllowed = upFilter.axisVelFilter.currValue > velMag && upFilter.axisAccelFilter.currValue > (upFilter.axisVelFilter.currValue * diffMag) && (upFilter.axisVelFilter.currValue / leftRightVelAbs) > 1.3f;
            downAllowed = downFilter.axisVelFilter.currValue > velMag && downFilter.axisAccelFilter.currValue > (downFilter.axisVelFilter.currValue * diffMag) && (downFilter.axisVelFilter.currValue / leftRightVelAbs) > 1.3f;
            leftAllowed = leftFilter.axisVelFilter.currValue > proxVelMag && leftFilter.axisAccelFilter.currValue > (leftFilter.axisVelFilter.currValue * proxDiffMag) && (upDownVelAbs / leftFilter.axisVelFilter.currValue) < 1f;
            rightAllowed = rightFilter.axisVelFilter.currValue > proxVelMag && rightFilter.axisAccelFilter.currValue > (rightFilter.axisVelFilter.currValue * proxDiffMag) && (upDownVelAbs / rightFilter.axisVelFilter.currValue) < 1f;

            ProcessResultValues((!singleAxis || (singleAxis && singleAxisOption == Axis.UpDown)) && (!singleDirection || (singleDirection && singleDirectionOption == Direction.Up)) && upAllowed, upFilter.axisAccelFilter.currValue, 0);
            ProcessResultValues((!singleAxis || (singleAxis && singleAxisOption == Axis.UpDown)) && (!singleDirection || (singleDirection && singleDirectionOption == Direction.Down)) && downAllowed, downFilter.axisAccelFilter.currValue, 1);
            ProcessResultValues((!singleAxis || (singleAxis && singleAxisOption == Axis.LeftRight)) && (!singleDirection || (singleDirection && singleDirectionOption == Direction.Left)) && leftAllowed, leftFilter.axisAccelFilter.currValue, 2);
            ProcessResultValues((!singleAxis || (singleAxis && singleAxisOption == Axis.LeftRight)) && (!singleDirection || (singleDirection && singleDirectionOption == Direction.Right)) && rightAllowed, rightFilter.axisAccelFilter.currValue, 3);

            for (int i = 0; i < directions.Length; i++)
            {
                if (_directionTimes[i] > interactionTimer)
                {
                    Direction resultdir = ConvertDirection(CalculateDirection(i));
                    if (resultdir != Direction.None)
                    {
                        ProcessResult(resultdir, out result, out value);
                        _cachedResult = result;
                        _cachedValue = value;
                        break;
                    }
                }
            }
        }

        public override void ResetValues()
        {
            _magCooldown = requireLowMagnitudeRepeat;
            _prevSquareVelDelta = _currentSquareVelDelta * 2f;
            _intCooldownCurrent = interactionCooldown;
            for (int i = 0; i < _directionTimes.Length; i++)
            {
                _directionTimes[i] = 0f;
            }
        }

        private void ProcessResultValues(bool allowed, float value, int indA)
        {
            if (allowed)
            {
                if (value > 0)
                {
                    UpdateDirectionTimes(indA, _updateDelta);
                }
                else
                {
                    UpdateDirectionTimes(indA, -_updateDelta);
                }
            }
            else
            {
                UpdateDirectionTimes(indA, -_updateDelta);
            }
        }

        private void UpdateDirectionTimes(int ind, float time)
        {
            _directionTimes[ind] += time;

            if (time < 0)
            {
                if (_directionTimes[ind] < 0)
                {
                    _directionTimes[ind] = 0;
                }
            }
        }

        private void ProcessResult(Direction direction, out bool result, out float value)
        {
            for (int i = 0; i < _directionTimes.Length; i++)
            {
                _directionTimes[i] = 0f;
            }
            resultDirection = direction;
            result = true;
            value = 1;
            _intCooldownCurrent = interactionCooldown;
            _magCooldown = requireLowMagnitudeRepeat;
            _prevSquareVelDelta = _currentSquareVelDelta;
            _currentMagCooldownTime = magCooldownTime;

            _needReset = true;

            upAllowed = false;
            downAllowed = false;
            leftAllowed = false;
            rightAllowed = false;
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
                case Direction.Up:
                    return _upOutput;
                case Direction.Right:
                    return _rightOutput;
                case Direction.Down:
                    return _downOutput;
                case Direction.Left:
                    return _leftOutput;
            }
            return direction;
        }

        protected override void OnValidate()
        {
            singleAxis = true;
            base.OnValidate();
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

            output += $" Combo {Vector3.Dot(Quaternion.AngleAxis(_a, hand.Direction) * Quaternion.AngleAxis(_b, hand.PalmNormal) * hand.Direction, hand.Rotation * direction)}";
            Debug.Log(output);
        }

        private void OnDrawGizmos()
        {
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