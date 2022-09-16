// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using UnityEngine;
using UnityEngine.EventSystems;

namespace Enflux.Examples.Components
{
    [RequireComponent(typeof(EventSystem))]
    public class CalculatePixelDragThreshold : MonoBehaviour
    {
        public Canvas Canvas;
        public float ReferencePixelDragThreshold = 5f;

        private EventSystem _eventSystem;


        private void Reset()
        {
            Canvas = FindObjectOfType<Canvas>();
            ReferencePixelDragThreshold = GetComponent<EventSystem>().pixelDragThreshold;
        }

        private void Awake()
        {
            _eventSystem = GetComponent<EventSystem>();
        }

        private void Start()
        {
            RecalculatePixelDragThreshold();
        }

        public bool RecalculatePixelDragThreshold()
        {
            if (Canvas == null)
            {
                return false;
            }
            _eventSystem.pixelDragThreshold = Mathf.RoundToInt(Screen.dpi / Canvas.referencePixelsPerUnit * ReferencePixelDragThreshold);
            return true;
        }
    }
}