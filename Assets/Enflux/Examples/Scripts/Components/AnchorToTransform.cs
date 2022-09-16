// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using UnityEngine;

namespace Enflux.Examples.Components
{
    /// <summary>
    /// Anchors the attached transform to another.
    /// </summary>
    [ExecuteInEditMode]
    public class AnchorToTransform : MonoBehaviour
    {
        /// <summary>
        /// Callbacks during the Unity update loop.
        /// </summary>
        public enum UpdateModeType
        {
            /// <summary>
            /// Update during MonoBehaviour.Update().
            /// </summary>
            Update,
            /// <summary>
            /// Update during MonoBehaviour.LateUpdate().
            /// </summary>
            LateUpdate
        }

        /// <summary>
        /// Transform to anchor to.
        /// </summary>
        public Transform Anchor;
        /// <summary>
        /// Positional offset from <see cref="Anchor"/>.
        /// </summary>
        public Vector3 Offset;
        /// <summary>
        /// Make this transform look at <see cref="Anchor"/>? 
        /// </summary>
        public bool LookAtAnchor = true;
        /// <summary>
        /// The callback during the Unity update loop to update this transform.
        /// </summary>
        public UpdateModeType UpdateMode = UpdateModeType.Update;


        private void Update()
        {
            if (UpdateMode == UpdateModeType.Update)
            {
                UpdateTransform();
            }
        }

        private void LateUpdate()
        {
            if (UpdateMode == UpdateModeType.LateUpdate)
            {
                UpdateTransform();
            }
        }

        private void UpdateTransform()
        {
            if (Anchor == null)
            {
                return;
            }
            transform.position = Anchor.transform.position + Offset;
            if (LookAtAnchor)
            {
                transform.LookAt(Anchor);
            }
        }
    }
}