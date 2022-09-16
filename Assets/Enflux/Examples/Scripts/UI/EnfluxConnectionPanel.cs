// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using System;
using System.Collections;
using System.Linq;
using Enflux.Common.DataTypes;
using Enflux.SDK.Core;
using Enflux.SDK.Core.Devices;
using Enflux.SDK.DataTypes;
using Enflux.SDK.Extensions;
using Enflux.Shim.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Enflux.Examples.UI
{
    /// <summary>
    /// Example UI panel to steam, calibrate, and align Enflux devices.
    /// This demonstrates the API and subscribing to the events of <see cref="EnfluxManager"/>.
    /// </summary>
    public class EnfluxConnectionPanel : MonoBehaviour
    {
        [SerializeField] private EnfluxManager _enfluxManager;

        // Toggle streaming UI
        [SerializeField] private Button _startStreamingAllButton;
        [SerializeField] private Text _startStreamingAllText;
        [SerializeField] private Button _stopStreamingAllButton;
        [SerializeField] private Text _stopStreamingAllText;

        // Alignment calibration UI
        [SerializeField] private Button _resetHeadingButton;
        [SerializeField] private Text _resetHeadingText;
        [SerializeField] private Button _startAlignmentButton;
        [SerializeField] private Text _startAlignmentText;

        // Misc UI
        [SerializeField] private Button _openBluetoothManagerButton;
        [SerializeField] private float _resetHeadingTime = 4.0f;

        public Color NormalButtonColor = Color.white;
        public Color StartStreamingButtonColor = new Color32(0x20, 0xC4, 0xFF, 0xFF);

        private const float ResetUiMessagesTime = 4.0f;

        private IEnumerator _co_resetHeadingCountdown;


        public float ResetHeadingTime
        {
            get { return _resetHeadingTime; }
            set { _resetHeadingTime = Mathf.Max(0f, value); }
        }


        private void Reset()
        {
            _enfluxManager = FindObjectOfType<EnfluxManager>();

            _startStreamingAllButton = gameObject.FindChildComponent<Button>("Button_StartStreamingAll");
            _startStreamingAllText = gameObject.FindChildComponent<Text>("Text_StartStreamingAll");
            _stopStreamingAllButton = gameObject.FindChildComponent<Button>("Button_StopStreamingAll");
            _stopStreamingAllText = gameObject.FindChildComponent<Text>("Text_StopStreamingAll");

            _resetHeadingButton = gameObject.FindChildComponent<Button>("Button_ResetHeading");
            _resetHeadingText = gameObject.FindChildComponent<Text>("Text_ResetHeading");
            _startAlignmentButton = gameObject.FindChildComponent<Button>("Button_StartAlignment");
            _startAlignmentText = gameObject.FindChildComponent<Text>("Text_StartAlignment");

            _openBluetoothManagerButton = gameObject.FindChildComponent<Button>("Button_LaunchBluetoothManager");
        }

        private void OnValidate()
        {
            // Force validation
            ResetHeadingTime = ResetHeadingTime;
        }

        private void OnEnable()
        {
            _enfluxManager = _enfluxManager ?? FindObjectOfType<EnfluxManager>();

            if (_enfluxManager == null)
            {
                Debug.LogError(name + ": EnfluxManager is not assigned and no instance is in the scene!");
                enabled = false;
            }

            SubscribeToEvents();
            UpdateUi();
        }

        private void OnDisable()
        {
            if (_enfluxManager != null)
            {
                UnsubscribeFromEvents();
            }
        }

        private void Start()
        {
            UpdateUi();
        }

        private void Update()
        {
            if (_enfluxManager != null)
            {
                if (_enfluxManager.AlignmentState == AlignmentState.CountingDown)
                {
                    _startAlignmentText.text = string.Format("Stop Alignment\n(Starting in {0:0.00})", _enfluxManager.RemainingAlignmentCountdownTime);
                }
                else if (_enfluxManager.AlignmentState == AlignmentState.Aligning)
                {
                    _startAlignmentText.text = "Stop Alignment (In Progress)";
                }
                if (_enfluxManager.IsAnyDeviceWaitingToStream)
                {
                    UpdateUi();
                }
            }
        }

        private void SubscribeToEvents()
        {
            _enfluxManager.AlignmentStateChanged += EnfluxManagerOnAlignmentStateChanged;
            _enfluxManager.DeviceStateChanged += EnfluxManagerOnDeviceStateChanged;
            _enfluxManager.DeviceReceivedInputCommand += EnfluxManagerOnDeviceReceivedInputCommand;

            _startStreamingAllButton.onClick.AddListener(ToggleStreamingAll);
            _stopStreamingAllButton.onClick.AddListener(ToggleStreamingAll);
            _resetHeadingButton.onClick.AddListener(ResetHeadingButtonOnClick);
            _startAlignmentButton.onClick.AddListener(StartAlignmentButtonOnClick);
            _openBluetoothManagerButton.onClick.AddListener(OpenBluetoothManagerButtonOnClick);
        }

        private void UnsubscribeFromEvents()
        {
            _enfluxManager.AlignmentStateChanged -= EnfluxManagerOnAlignmentStateChanged;
            _enfluxManager.DeviceStateChanged -= EnfluxManagerOnDeviceStateChanged;
            _enfluxManager.DeviceReceivedInputCommand -= EnfluxManagerOnDeviceReceivedInputCommand;

            _startStreamingAllButton.onClick.RemoveListener(ToggleStreamingAll);
            _stopStreamingAllButton.onClick.RemoveListener(ToggleStreamingAll);
            _resetHeadingButton.onClick.RemoveListener(ResetHeadingButtonOnClick);
            _startAlignmentButton.onClick.RemoveListener(StartAlignmentButtonOnClick);
            _openBluetoothManagerButton.onClick.RemoveListener(OpenBluetoothManagerButtonOnClick);
        }

        private void EnfluxManagerOnAlignmentStateChanged(StateChange<AlignmentState> stateChange)
        {
            UpdateUi();
        }

        private void EnfluxManagerOnDeviceStateChanged(OrientationDevice orientationDevice, StateChange<DeviceState> stateChange)
        {
            UpdateUi();
        }

        private void EnfluxManagerOnDeviceReceivedInputCommand(EnfluxDevice enfluxDevice, Notification<InputCommands> notification)
        {
            UpdateUi();
        }

        private void ToggleStreamingAll()
        {
            if (_enfluxManager.IsAnyDeviceStreaming || _enfluxManager.IsAnyDeviceWaitingToStream)
            {
                _enfluxManager.StartScanning(EnfluxDeviceFlags.All);
            }
            else
            {
                var readyDevices = _enfluxManager.GetCurrentDevicesWhere(device => device.IsReady).ToArray();
                var readyDeviceTypes = EnfluxDeviceFlagsUtils.GenerateFlags(readyDevices.Select(device => device.DeviceFlag).ToArray());
                _enfluxManager.StartStreaming(readyDeviceTypes);
            }
        }

        private void ResetHeadingButtonOnClick()
        {
            ToggleResetHeadingCountdown();
        }

        private void StartAlignmentButtonOnClick()
        {
            if (_enfluxManager.AlignmentState == AlignmentState.NotAligning)
            {
                _enfluxManager.StartAlignment();
            }
            else
            {
                _enfluxManager.StopAlignment();
            }
        }

        private void OpenBluetoothManagerButtonOnClick()
        {
            PlatformUtils.LaunchBluetoothManager();
        }

        private void UpdateUi()
        {
            var isAnyDeviceInStreamingSequence = _enfluxManager.IsAnyDeviceWaitingToStream || _enfluxManager.IsAnyDevice(device => device.IsStreamingInitiated);
            if (_enfluxManager.IsAnyDeviceWaitingToStream)
            {
                _stopStreamingAllText.text = string.Format("Stop Streaming All ({0:0.00}s)", _enfluxManager.RemainingWaitingToStreamTime);
                _stopStreamingAllButton.image.color = NormalButtonColor;
            }
            else if (isAnyDeviceInStreamingSequence)
            {
                var numActiveDevices = _enfluxManager.GetCurrentDevicesWhere(device => device.IsActive).Count();
                _stopStreamingAllText.text = string.Format("Stop Streaming All ({0} {1} Active)", numActiveDevices, numActiveDevices > 1 ? "Devices" : "Device");
                _stopStreamingAllButton.image.color = NormalButtonColor;
            }
            else
            {
                var numReadyDevices = _enfluxManager.GetCurrentDevicesWhere(device => device.IsReady).Count();
                _startStreamingAllText.text = string.Format("Start Streaming All ({0} {1} Ready)", numReadyDevices, numReadyDevices > 1 ? "Devices" : "Device");
                _startStreamingAllButton.image.color = StartStreamingButtonColor;
            }
            _startStreamingAllButton.gameObject.SetActive(!isAnyDeviceInStreamingSequence);
            _stopStreamingAllButton.gameObject.SetActive(isAnyDeviceInStreamingSequence);
            _startStreamingAllButton.interactable = _enfluxManager.IsAnyDeviceReady || _enfluxManager.IsAnyDeviceActive;
            _stopStreamingAllButton.interactable = _enfluxManager.IsAnyDeviceReady || _enfluxManager.IsAnyDeviceActive;

            if (_enfluxManager.AlignmentState == AlignmentState.NotAligning)
            {
                _startAlignmentText.text = "Start Alignment";
            }
            _resetHeadingButton.interactable = _enfluxManager.IsAnyDeviceStreaming;
            _startAlignmentButton.interactable = _enfluxManager.IsAnyDeviceStreaming;
        }

        private void SafelyStopCoroutine(IEnumerator coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        // Toggle reset heading countdown on/off
        private void ToggleResetHeadingCountdown()
        {
            if (_co_resetHeadingCountdown == null)
            {
                StartResetHeadingCountdown();
            }
            else
            {
                StopResetHeadingCountdown();
            }
        }

        private void StartResetHeadingCountdown()
        {
            StopResetHeadingCountdown();
            _co_resetHeadingCountdown = Co_ResetHeadingCountdown();
            StartCoroutine(_co_resetHeadingCountdown);
        }

        private void StopResetHeadingCountdown()
        {
            SafelyStopCoroutine(_co_resetHeadingCountdown);
            _co_resetHeadingCountdown = null;
            _resetHeadingText.text = "Reset Heading";
        }

        private IEnumerator Co_ResetHeadingCountdown()
        {
            var time = ResetHeadingTime;
            while (time > 0.0f)
            {
                _resetHeadingText.text = string.Format("Stop Reset Heading\n(Starting in {0:0.00})", time);
                yield return null;
                time -= Time.deltaTime;
            }
            _enfluxManager.ResetFullBodyHeading();
            StopResetHeadingCountdown();
        }
    }
}