// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using UnityEngine;
using UnityEngine.UI;

namespace Enflux.Examples.UI
{
    /// <summary>
    /// Widget to toggle on/off a GameObject on click.
    /// </summary>
    public class EnfluxToggleWidget : MonoBehaviour
    {
        [SerializeField] private GameObject _toggleTarget;
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private Sprite _showSprite;
        [SerializeField] private Sprite _hideSprite;

        /// <summary>
        /// Does <see cref="_toggleTarget"/> start visible?
        /// </summary>
        public bool StartVisible = true;


        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(ButtonOnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(ButtonOnClick);
        }

        private void Start()
        {
            if (_toggleTarget != null)
            {
                _toggleTarget.SetActive(StartVisible);
            }
        }

        private void ButtonOnClick()
        {
            _toggleTarget.SetActive(!_toggleTarget.activeInHierarchy);
        }

        private void Update()
        {
            if (_toggleTarget != null && _icon != null)
            {
                _icon.sprite = _toggleTarget.activeInHierarchy ? _hideSprite : _showSprite;
            }
        }
    }
}