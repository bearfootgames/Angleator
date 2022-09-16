// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using Enflux.SDK.Attributes;
using UnityEngine;

namespace Enflux.Examples.Components
{
    /// <summary>
    /// Ping pongs a transform between two local scales.
    /// </summary>
    public class PingPongLocalScale : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _cycleStart;
        [SerializeField, Readonly] private Vector3 _currentValue;
        [SerializeField, Readonly] private Vector3 _initialValue;
        [SerializeField] private Vector3 _targetValue;

        public Transform Transform;
        public bool RandomizeCycleStart = false;
        public float Speed = 1.0f;
        public bool IsPaused;
        public float Time;

        public Vector3 CurrentValue
        {
            get { return _currentValue; }
            private set
            {
                _currentValue = value;
                if (Transform != null)
                {
                    transform.localScale = value;
                }
            }
        }

        public Vector3 InitialValue
        {
            get { return _initialValue; }
            set
            {
                _initialValue = value;
                Update();
            }
        }

        public Vector3 TargetValue
        {
            get { return _targetValue; }
            set
            {
                _targetValue = value;
                Update();
            }
        }

        private void Reset()
        {
            Transform = GetComponent<Transform>();
            TargetValue = Vector3.one;
        }

        private void Start()
        {
            if (RandomizeCycleStart)
            {
                _cycleStart = Random.value;
            }
            if (Transform != null)
            {
                InitialValue = Transform.localScale;
            }
        }

        private void Update()
        {
            if (IsPaused)
            {
                return;
            }
            Time += UnityEngine.Time.deltaTime;
            var t = 0.5f + 0.5f * Mathf.Sin(2.0f * Mathf.PI * _cycleStart - 2f * Mathf.PI * Speed * Time - 0.5f * Mathf.PI);
            CurrentValue = Vector3.Lerp(InitialValue, TargetValue, t);
        }
    }
}