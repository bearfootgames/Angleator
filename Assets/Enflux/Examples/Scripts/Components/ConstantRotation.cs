// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using UnityEngine;

namespace Enflux.Examples.Components
{
    /// <summary>
    /// Constantly rotations the attached transform.
    /// </summary>
    public class ConstantRotation : MonoBehaviour
    {
        /// <summary>
        /// Axis of rotation.
        /// </summary>
        public Vector3 Axis = Vector3.up;

        /// <summary>
        /// Rotational speed per second.
        /// </summary>
        public float Speed = 1f;

        /// <summary>
        /// Coordinate space to rotate in.
        /// </summary>
        public Space Space = Space.Self;


        private void Update()
        {
            transform.Rotate(Axis, Speed * Time.deltaTime, Space);
        }
    }
}
