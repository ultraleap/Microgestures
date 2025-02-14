using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.GestureMachine
{

    public class RepeatedDirectionGesture : DirectionalGesture
    {
        public int repeatCount = 2;
        public float timeOut = 0.5f;

        public float cooldown = 1f;
        private float _currentCooldown = 0f;

        protected List<(Direction, float)> _directionTimes = new List<(Direction, float)>();
        protected List<int> _counts = new List<int>();

        private void Awake()
        {
            for (int i = 0; i < Enum.GetValues(typeof(Direction)).Length; i++)
            {
                _counts.Add(0);
            }
        }

        protected override void StartTrackingFunction(Hand hand)
        {
        }

        protected override void StopTrackingFunction()
        {
            
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (childGestures.Count > 1)
            {
               childGestures.RemoveRange(1, childGestures.Count - 1);
            }
        }

        private void Update()
        {
            if(_currentCooldown > 0)
            {
                _currentCooldown -= Time.deltaTime;
            }   
        }

        protected override void UpdateHandFunction(Hand hand, out bool result, out float value)
        {
            if(_currentCooldown > 0)
            {
                result = false;
                value = 0;
                return;
            }

            if (childGestures[0].changedThisFrame && childGestures[0].result)
            {
                Direction resultDir = ((DirectionalGesture)childGestures[0]).resultDirection;
                if (!singleDirection || (singleDirection && resultDir == singleDirectionOption))
                {
                    _directionTimes.Add((((DirectionalGesture)childGestures[0]).resultDirection, Time.time));
                }
            }

            for (int i = 0; i < _counts.Count; i++)
            {
                _counts[i] = 0;
            }

            result = false;

            int current, max = 0;
            for (int i = 0; i < _directionTimes.Count; i++)
            {
                if (Time.time - _directionTimes[i].Item2 > timeOut)
                {
                    _directionTimes.RemoveAt(i);
                    i--;
                }
                else
                {
                    _counts[(int)_directionTimes[i].Item1]++;
                    current = _counts[(int)_directionTimes[i].Item1];
                    if(current > max)
                    {
                        max = current;
                        if (current >= repeatCount)
                        {
                            resultDirection = _directionTimes[i].Item1;
                            result = true;
                            _currentCooldown = cooldown;
                        }
                    }
                }
            }

            if (result)
            {
                _directionTimes.Clear();
            }

            value = Mathf.Clamp01((float)max / repeatCount);
        }

        public override void ResetValues()
        {
            _currentCooldown = cooldown;
        }

    }
}