// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using System.Collections;
using System.Linq;
using Enflux.Common.DataTypes;
using Enflux.SDK.Attributes;
using Enflux.SDK.Core;
using Enflux.SDK.Core.Devices;
using Enflux.SDK.DataTypes;
using Enflux.SDK.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Enflux.Examples.UI
{
    /// <summary>
    ///  Example UI panel to steam, calibrate, and align Enflux devices. 
    ///  This demonstrates the API and subscribing to the events of <see cref="EnfluxManager"/>.
    /// </summary>
    public class EnfluxContextPanel : MonoBehaviour
    {
        private enum Context
        {
            None,
            AlignmentCountdown,
            WaitingToStreamCountdown,
            InitializingStreaming,
            Calibrating
        }

        [SerializeField] private EnfluxManager _enfluxManager;
        [SerializeField, Readonly] private Context _currentContext = Context.None;

        [SerializeField] private Text _headerText;
        [SerializeField] private Text _contextText;
        [SerializeField] private CanvasGroup _parentContainer;
        [SerializeField] private RectTransform _alignmentContainer;
        [SerializeField] private RectTransform _streamingContainer;
        [SerializeField] private RectTransform _calibratingContainer;
        [SerializeField] private Image _alignmentCountdownBar;
        [SerializeField] private Image _streamingCountdownBar;
        [SerializeField] private RectTransform _initializingStreamingProgressWheel;

        private const string AlignmentHeaderMessage = "Waiting To Align";
        private const string CalibratingHeaderMessage = "Calibrating";
        private const string StreamingCountdownHeaderMessage = "Waiting To Stream";
        private const string InitializingStreamingHeaderMessage = "Initializing Streaming";

        private const string AlignmentContextMessage = "Stand still at shoulder width.\nClasp the sides of your thighs with your hands.";
        private const string CalibratingContextMessage = "Fold garment into a ball.\nKeep rotating in every possible direction.";
        private const string StreamingCountdownContextMessage = "Stand still at shoulder width.\nClasp the sides of your thighs with your hands.";
        private const string InitializingStreamingContextMessage = "Stand still at shoulder width.\nClasp the sides of your thighs with your hands.";

        private IEnumerator _co_fade;
        private EnfluxDeviceFlags _contextDevices;


        private Context CurrentContext
        {
            get { return _currentContext; }
            set
            {
                if (_currentContext == value)
                {
                    return;
                }
                _currentContext = value;
                UpdateUi();
            }
        }


        private void Reset()
        {
            _enfluxManager = FindObjectOfType<EnfluxManager>();

            _parentContainer = GetComponent<CanvasGroup>();
            _headerText = gameObject.FindChildComponent<Text>("Text_Header");
            _contextText = gameObject.FindChildComponent<Text>("Text_Context");

            _alignmentContainer = gameObject.FindChildComponent<RectTransform>("Container_Alignment");
            _streamingContainer = gameObject.FindChildComponent<RectTransform>("Container_Streaming");
            _calibratingContainer = gameObject.FindChildComponent<RectTransform>("Container_Calibrating");
            _alignmentCountdownBar = gameObject.FindChildComponent<Image>("Image_AlignmentCountdownBar");
            _streamingCountdownBar = gameObject.FindChildComponent<Image>("Image_StreamingCountdownBar");
            _initializingStreamingProgressWheel = gameObject.FindChildComponent<RectTransform>("ProgressWheel_InitializingStreaming");
        }

        private void OnEnable()
        {
            _enfluxManager = _enfluxManager ?? FindObjectOfType<EnfluxManager>();

            if (_enfluxManager == null)
            {
                Debug.LogError(name + ": EnfluxManager is not assigned and no instance is in the scene!");
                enabled = false;
            }

            HidePanel(false);
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            if (_enfluxManager != null)
            {
                UnsubscribeFromEvents();
            }
        }

        private void Update()
        {
            // Update alignment countdown bar
            if (CurrentContext == Context.AlignmentCountdown)
            {
                var normalizedProgress = Mathf.Clamp01((_enfluxManager.AlignmentCountdownTime - _enfluxManager.RemainingAlignmentCountdownTime) /
                                                       _enfluxManager.AlignmentCountdownTime);
                if (_enfluxManager.IsAnyDeviceStreaming)
                {
                    _alignmentCountdownBar.fillAmount = normalizedProgress;
                }
            }
            // Update streaming countdown bar
            else if (CurrentContext == Context.WaitingToStreamCountdown)
            {
                var normalizedProgress = Mathf.Clamp01((_enfluxManager.TotalWaitingToStreamTime - _enfluxManager.RemainingWaitingToStreamTime) /
                                                       _enfluxManager.TotalWaitingToStreamTime);
                if (_enfluxManager.IsAnyDeviceWaitingToStream)
                {
                    _streamingCountdownBar.fillAmount = normalizedProgress;
                }
            }
            else if (CurrentContext == Context.InitializingStreaming)
            {
                _streamingCountdownBar.fillAmount = 1.0f;
            }
        }

        private void SubscribeToEvents()
        {
            _enfluxManager.AlignmentStateChanged += EnfluxManagerOnAlignmentStateChanged;
            _enfluxManager.DeviceStateChanged += EnfluxManagerOnDeviceStateChanged;
            _enfluxManager.DeviceReceivedInputCommand += EnfluxManagerOnDeviceReceivedInputCommand;
        }

        private void UnsubscribeFromEvents()
        {
            _enfluxManager.AlignmentStateChanged -= EnfluxManagerOnAlignmentStateChanged;
            _enfluxManager.DeviceStateChanged -= EnfluxManagerOnDeviceStateChanged;
            _enfluxManager.DeviceReceivedInputCommand -= EnfluxManagerOnDeviceReceivedInputCommand;
        }

        private void EnfluxManagerOnAlignmentStateChanged(StateChange<AlignmentState> stateChange)
        {
            UpdateContext();
            UpdateUi();
        }

        private void EnfluxManagerOnDeviceStateChanged(OrientationDevice device, StateChange<DeviceState> stateChange)
        {
            UpdateContext();
            UpdateUi();
        }

        private void EnfluxManagerOnDeviceReceivedInputCommand(EnfluxDevice enfluxDevice, Notification<InputCommands> notification)
        {
            UpdateContext();
            UpdateUi();
        }

        private void UpdateContext()
        {
            var calibratingDevices = _enfluxManager.GetCurrentDevicesWhere(device => device.IsCalibratingInitiated).ToArray();
            var streamingDevices = _enfluxManager.GetCurrentDevicesWhere(device => device.IsStreamingInitiated).ToArray();
            var initializingStreamingDevices = _enfluxManager.GetCurrentDevicesWhere(device => device.State == DeviceState.InitializingStreaming).ToArray();
            var waitingToStreamDeviceFlags = _enfluxManager.DevicesWaitingToStream;
            var waitingToStreamDevices = _enfluxManager.GetCurrentDevicesWhere(device => waitingToStreamDeviceFlags.ContainsFlag(device.DeviceFlag)).ToArray();
            var isAligning = _enfluxManager.AlignmentState != AlignmentState.NotAligning;

            if (calibratingDevices.Any())
            {
                _contextDevices = EnfluxDevice.GenerateFlags(calibratingDevices);
                CurrentContext = Context.Calibrating;
            }
            else if (initializingStreamingDevices.Any())
            {
                _contextDevices = EnfluxDevice.GenerateFlags(initializingStreamingDevices);
                CurrentContext = Context.InitializingStreaming;
            }
            else if (waitingToStreamDevices.Any())
            {
                _contextDevices = EnfluxDevice.GenerateFlags(waitingToStreamDevices);
                CurrentContext = Context.WaitingToStreamCountdown;
            }
            else if (isAligning)
            {
                _contextDevices = EnfluxDevice.GenerateFlags(streamingDevices);
                CurrentContext = Context.AlignmentCountdown;
            }
            else
            {
                CurrentContext = Context.None;
            }
        }

        private void UpdateUi()
        {
            var headerMessageSuffix = string.Format(" ({0})", _contextDevices);
            _alignmentContainer.gameObject.SetActive(false);
            _streamingContainer.gameObject.SetActive(false);
            _calibratingContainer.gameObject.SetActive(false);
            _initializingStreamingProgressWheel.gameObject.SetActive(false);

            switch (_currentContext)
            {
                case Context.AlignmentCountdown:
                    _headerText.text = AlignmentHeaderMessage + headerMessageSuffix;
                    _contextText.text = AlignmentContextMessage;
                    _alignmentContainer.gameObject.SetActive(true);
                    break;

                case Context.WaitingToStreamCountdown:
                    _headerText.text = StreamingCountdownHeaderMessage + headerMessageSuffix;
                    _contextText.text = StreamingCountdownContextMessage;
                    _streamingContainer.gameObject.SetActive(true);
                    break;

                case Context.InitializingStreaming:
                    _headerText.text = InitializingStreamingHeaderMessage + headerMessageSuffix;
                    _contextText.text = InitializingStreamingContextMessage;
                    _streamingContainer.gameObject.SetActive(true);
                    _initializingStreamingProgressWheel.gameObject.SetActive(true);
                    break;

                case Context.Calibrating:
                    _headerText.text = CalibratingHeaderMessage + headerMessageSuffix;
                    _contextText.text = CalibratingContextMessage;
                    _calibratingContainer.gameObject.SetActive(true);
                    break;
            }

            if (CurrentContext == Context.None)
            {
                HidePanel();
            }
            else
            {
                _alignmentCountdownBar.fillAmount = 0.0f;
                _streamingCountdownBar.fillAmount = 0.0f;
                ShowPanel();
            }
        }

        private void ShowPanel(bool animate = true)
        {
            if (_parentContainer != null)
            {
                if (animate)
                {
                    FadePanel(1.0f);
                }
                else
                {
                    _parentContainer.alpha = 1.0f;
                }
                _parentContainer.interactable = true;
                _parentContainer.blocksRaycasts = true;
            }
        }

        private void HidePanel(bool animate = true)
        {
            if (_parentContainer != null)
            {
                if (animate)
                {
                    FadePanel(0.0f);
                }
                else
                {
                    _parentContainer.alpha = 0.0f;
                }
                _parentContainer.interactable = false;
                _parentContainer.blocksRaycasts = false;
            }
        }

        private void FadePanel(float alpha)
        {
            if (_co_fade != null)
            {
                StopCoroutine(_co_fade);
                _co_fade = null;
            }
            _co_fade = Co_Fade(alpha);
            StartCoroutine(_co_fade);
        }

        private IEnumerator Co_Fade(float alpha)
        {
            var initialAlpha = _parentContainer.alpha;
            const float totalTime = 0.25f;
            var time = totalTime;
            while (time > 0f)
            {
                if (_parentContainer != null)
                {
                    var t = Mathf.Clamp01(1.0f - time / totalTime);
                    _parentContainer.alpha = Mathf.Lerp(initialAlpha, alpha, t);
                }
                yield return null;
                time -= Time.deltaTime;
            }
            if (_parentContainer != null)
            {
                _parentContainer.alpha = alpha;
            }
            _co_fade = null;
        }
    }
}