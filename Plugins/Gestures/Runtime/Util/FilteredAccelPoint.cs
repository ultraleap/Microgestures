using UnityEngine;

namespace Leap.Unity.GestureMachine
{
    [System.Serializable]
    public class FilteredAccelAxis
    {
        [HideInInspector]
        public OneEuroFilter axisFilter, axisVelFilter, axisAccelFilter;

        private float _resetTime = 0f;

        [System.Serializable]
        public class FilterParams
        {
            [Tooltip("x = fcmin, y = beta, z = cutoff for derivative")]
            public Vector3 pointParams = new Vector3(0.8f, .1f, .1f),
                velParams = new Vector3(5, .05f, 8f),
                accelParams = new Vector3(3, .02f, 4f),
                resetParams = new Vector3(.2f, .1f, .1f);
            public float resetTime;
        }

        public FilterParams parameters;

        private Vector3 _curPointParams, _curVelParams, _curAccelParams;

        public void Awake(FilterParams parameters)
        {
            this.parameters = parameters;
            Awake();
        }

        public void Awake()
        {
            axisFilter = new OneEuroFilter(1, parameters.pointParams.x, parameters.pointParams.y, parameters.pointParams.z);
            axisVelFilter = new OneEuroFilter(1, parameters.velParams.x, parameters.velParams.y, parameters.velParams.z);
            axisAccelFilter = new OneEuroFilter(1, parameters.accelParams.x, parameters.accelParams.y, parameters.accelParams.z);
        }

        public void Update(float axis, float deltaTime, float resetAmount = 0)
        {
            if (_resetTime > 0)
            {
                _resetTime -= deltaTime;
                if (_resetTime <= 0)
                {
                    _resetTime = 0;
                }
                _curPointParams = Vector3.Lerp(parameters.resetParams, parameters.pointParams, Mathf.InverseLerp(parameters.resetTime, 0, _resetTime));
            }
            else
            {
                _curPointParams = parameters.pointParams;
            }

            _curPointParams = Vector3.Lerp(_curPointParams, parameters.resetParams, resetAmount);

            axisFilter.UpdateParams(1, _curPointParams.x, _curPointParams.y, _curPointParams.z);
            axisFilter.Filter(axis, Time.time);

            _curVelParams = Vector3.Lerp(parameters.velParams, parameters.resetParams, resetAmount);

            axisVelFilter.UpdateParams(1, _curVelParams.x, _curVelParams.y, _curVelParams.z);
            axisVelFilter.Filter((axisFilter.currValue - axisFilter.prevValue) / deltaTime, Time.time);

            _curAccelParams = Vector3.Lerp(parameters.accelParams, parameters.resetParams, resetAmount);

            axisAccelFilter.UpdateParams(1, _curAccelParams.x, _curAccelParams.y, _curAccelParams.z);
            axisAccelFilter.Filter((axisVelFilter.currValue - axisVelFilter.prevValue) / deltaTime, Time.time);
        }

        public void Reset()
        {
            _resetTime = parameters.resetTime;
        }
    }

    [System.Serializable]
    public class FilteredAccelPoint
    {
        [HideInInspector]
        public OneEuroFilter<Vector3> pointFilter, velFilter, accelFilter;

        private float _resetTime = 0f;

        [System.Serializable]
        public class FilterParams
        {
            [Tooltip("x = fcmin, y = beta, z = cutoff for derivative")]
            public Vector3 pointParams = new Vector3(0.8f, .1f, .1f),
                velParams = new Vector3(5, .05f, 8f),
                accelParams = new Vector3(3, .02f, 4f),
                resetParams = new Vector3(.2f, .1f, .1f);
            public float resetTime;
        }

        public FilterParams parameters;

        private Vector3 _curPointParams, _curVelParams, _curAccelParams;

        public void Awake(FilterParams parameters)
        {
            this.parameters = parameters;
            Awake();
        }

        public void Awake()
        {
            pointFilter = new OneEuroFilter<Vector3>(1, parameters.pointParams.x, parameters.pointParams.y, parameters.pointParams.z);
            velFilter = new OneEuroFilter<Vector3>(1, parameters.velParams.x, parameters.velParams.y, parameters.velParams.z);
            accelFilter = new OneEuroFilter<Vector3>(1, parameters.accelParams.x, parameters.accelParams.y, parameters.accelParams.z);
        }

        public void Update(Vector3 point, float deltaTime, float resetAmount = 0)
        {
            if (_resetTime > 0)
            {
                _resetTime -= deltaTime;
                if (_resetTime <= 0)
                {
                    _resetTime = 0;
                }
                _curPointParams = Vector3.Lerp(parameters.resetParams, parameters.pointParams, Mathf.InverseLerp(parameters.resetTime, 0, _resetTime));
            }
            else
            {
                _curPointParams = parameters.pointParams;
            }

            _curPointParams = Vector3.Lerp(_curPointParams, parameters.resetParams, resetAmount);

            pointFilter.UpdateParams(1, _curPointParams.x, _curPointParams.y, _curPointParams.z);
            pointFilter.Filter(point, Time.time);

            _curVelParams = Vector3.Lerp(parameters.velParams, parameters.resetParams, resetAmount);

            velFilter.UpdateParams(1, _curVelParams.x, _curVelParams.y, _curVelParams.z);
            velFilter.Filter((pointFilter.currValue - pointFilter.prevValue) / deltaTime, Time.time);

            _curAccelParams = Vector3.Lerp(parameters.accelParams, parameters.resetParams, resetAmount);

            accelFilter.UpdateParams(1, _curAccelParams.x, _curAccelParams.y, _curAccelParams.z);
            accelFilter.Filter((velFilter.currValue - velFilter.prevValue) / deltaTime, Time.time);
        }

        public void Reset()
        {
            _resetTime = parameters.resetTime;
        }
    }
}