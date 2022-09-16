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
using Enflux.SDK.Utils;
using Enflux.Shim.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Enflux.Examples.UI
{
    /// <summary>
    /// Example UI widget to display an Enflux device.
    /// </summary>
    public class CurrentDeviceWidget : MonoBehaviour
    {
        /// <summary>
        /// UI colors for device states.
        /// </summary>
        [Serializable]
        public struct DeviceStateColors
        {
            public Color Disconnected;
            public Color Connected;
            public Color InitializingStreaming;
            public Color InitializingCalibration;
            public Color Streaming;
            public Color Calibrating;

            /// <summary>
            /// Returns the corresponding color for <paramref name="deviceState"/>.
            /// </summary>
            /// <param name="deviceState"></param>
            /// <returns>The corresponding color for <paramref name="deviceState"/>.</returns>
            public Color ResolveColor(DeviceState deviceState)
            {
                switch (deviceState)
                {
                    case DeviceState.Disconnected:
                        return Disconnected;
                    case DeviceState.Connected:
                        return Connected;
                    case DeviceState.InitializingStreaming:
                        return InitializingStreaming;
                    case DeviceState.InitializingCalibration:
                        return InitializingCalibration;
                    case DeviceState.Streaming:
                        return Streaming;
                    case DeviceState.Calibrating:
                        return Calibrating;
                    default:
                        return new Color();
                }
            }
        }

        /// <summary>
        /// UI colors for device battery levels.
        /// </summary>
        [Serializable]
        public struct DeviceBatteryLevelColors
        {
            public Color Critical;
            public Color Low;
            public Color Normal;

            /// <summary>
            /// Returns the corresponding color for <paramref name="powerInfo"/>.
            /// </summary>
            /// <param name="powerInfo"></param>
            /// <returns>The corresponding color for <paramref name="powerInfo"/>.</returns>
            public Color ResolveColor(PowerInfo powerInfo)
            {
                if (powerInfo.IsBatteryCritical)
                {
                    return Critical;
                }
                if (powerInfo.IsBatteryLow)
                {
                    return Low;
                }
                return Normal;
            }
        }

        /// <summary>
        /// UI colors for button prompt states.
        /// </summary>
        [Serializable]
        public struct ButtonPromptColors
        {
            public Color Normal;
            public Color Prompted;
        }

        [SerializeField] private EnfluxManager _enfluxManager;

        // Device status & notifications UI
        [SerializeField] private Text _deviceNameText;
        [SerializeField] private Text _deviceStateText;
        [SerializeField] private Text _deviceBatteryLevelText;
        [SerializeField] private Image _deviceIconImage;

        // Toggle calibration UI
        [SerializeField] private Button _toggleCalibratingButton;
        [SerializeField] private Text _toggleCalibratingText;

        [SerializeField] private Button _openBluetoothManagerButton;

        public EnfluxDeviceFlags DeviceFlag = EnfluxDeviceFlags.Shirt;

        private const float ResetUiMessagesTime = 4.0f;
        public DeviceStateColors StateColors;
        public DeviceBatteryLevelColors BatteryLevelColors;
        public ButtonPromptColors CalibrationPromptColors;
        public Sprite UnavailableIcon;
        public Sprite AvailableIcon;

        private readonly DeviceDisplayMetadata _displayMetadata = new DeviceDisplayMetadata();

        private IEnumerator _co_promptCalibratingPrompt;
        private IEnumerator _co_updateStateTextUi;


        private void Reset()
        {
            _enfluxManager = FindObjectOfType<EnfluxManager>();

            _deviceNameText = gameObject.FindChildComponent<Text>("Text_DeviceName");
            _deviceStateText = gameObject.FindChildComponent<Text>("Text_DeviceState");
            _deviceBatteryLevelText = gameObject.FindChildComponent<Text>("Text_DeviceBatteryLevel");
            _deviceIconImage = gameObject.FindChildComponent<Image>("Image_DeviceIcon");

            _toggleCalibratingButton = gameObject.FindChildComponent<Button>("Button_ToggleCalibrating");
            _toggleCalibratingText = gameObject.FindChildComponent<Text>("Text_ToggleCalibrating");
            _openBluetoothManagerButton = gameObject.FindChildComponent<Button>("Button_OpenBluetoothManager");

            StateColors.Disconnected = Color.grey;
            StateColors.Connected = Color.white;
            StateColors.InitializingStreaming = Color.white;
            StateColors.InitializingCalibration = Color.white;
            StateColors.Streaming = Color.green;
            StateColors.Calibrating = Color.cyan;

            BatteryLevelColors.Critical = Color.red;
            BatteryLevelColors.Low = Color.yellow;
            BatteryLevelColors.Normal = Color.white;

            CalibrationPromptColors.Normal = new Color32(0xF0, 0xF0, 0xFF, 0xFF);
            CalibrationPromptColors.Prompted = new Color32(0x40, 0xC4, 0xFF, 0xFF);
        }

        private void OnValidate()
        {
            if (DeviceFlag == EnfluxDeviceFlags.None)
            {
                Debug.Log("DeviceFlag is set to None! Defaulting to Shirt.");
                DeviceFlag = EnfluxDeviceFlags.Shirt;
            }
            else if (DeviceFlag.AreMultipleFlagsSet())
            {
                Debug.Log("DeviceFlag contains multiple values! Defaulting to Shirt.");
                DeviceFlag = EnfluxDeviceFlags.Shirt;
            }
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
            var currentDevice = _enfluxManager.GetCurrentDevices(DeviceFlag).First();
            if (_enfluxManager != null)
            {
                var deviceState = currentDevice != null ? currentDevice.State : DeviceState.Disconnected;
                _deviceStateText.color = StateColors.ResolveColor(deviceState);
            }
        }

        private void SubscribeToEvents()
        {
            _enfluxManager.AlignmentStateChanged += EnfluxManagerOnAlignmentStateChanged;
            _enfluxManager.AlignmentReceivedNotification += EnfluxManagerOnAlignmentReceivedNotification;
            _enfluxManager.DeviceStateChanged += EnfluxManagerOnDeviceStateChanged;
            _enfluxManager.DeviceReceivedInputCommand += EnfluxManagerOnReceivedInputCommand;

            _toggleCalibratingButton.onClick.AddListener(ToggleCalibration);
            _openBluetoothManagerButton.onClick.AddListener(OpenBluetoothManagerButtonOnClick);
        }

        private void UnsubscribeFromEvents()
        {
            _enfluxManager.AlignmentStateChanged -= EnfluxManagerOnAlignmentStateChanged;
            _enfluxManager.AlignmentReceivedNotification -= EnfluxManagerOnAlignmentReceivedNotification;
            _enfluxManager.DeviceStateChanged -= EnfluxManagerOnDeviceStateChanged;
            _enfluxManager.DeviceReceivedInputCommand -= EnfluxManagerOnReceivedInputCommand;

            _toggleCalibratingButton.onClick.RemoveListener(ToggleCalibration);
            _openBluetoothManagerButton.onClick.RemoveListener(OpenBluetoothManagerButtonOnClick);
        }

        private void EnfluxManagerOnAlignmentStateChanged(StateChange<AlignmentState> stateChange)
        {
            UpdateUi();
        }

        private void EnfluxManagerOnAlignmentReceivedNotification(AlignmentNotification notification)
        {
            UpdateUi();
            var currentDevice = _enfluxManager.GetCurrentDevices(DeviceFlag).First();
            if (currentDevice != null && currentDevice.State == DeviceState.Streaming)
            {
                SetStatusTextColor(_deviceStateText, notification);
                ShowTemporaryDeviceMessage(notification.Nicify());
            }
        }

        private void EnfluxManagerOnDeviceStateChanged(OrientationDevice device, StateChange<DeviceState> stateChange)
        {
            var enfluxDevice = device as EnfluxDevice;
            if (enfluxDevice == null || enfluxDevice.DeviceFlag != DeviceFlag)
            {
                return;
            }

            UpdateUi();
            var currentDevice = _enfluxManager.GetCurrentDevices(DeviceFlag).First();
            if (currentDevice == null || currentDevice.IsActive)
            {
                StopCalibrationPrompt();
            }
        }

        private void EnfluxManagerOnReceivedInputCommand(EnfluxDevice device, Notification<InputCommands> notification)
        {
            if (device.DeviceFlag != DeviceFlag)
            {
                return;
            }
            if (device.State != DeviceState.Disconnected)
            {
                UpdateUi();
                SetStatusTextColor(_deviceStateText, notification.Value);
                ShowTemporaryDeviceMessage(notification.Message);
            }
        }

        private void ToggleCalibration()
        {
            // Current device should never be null at this point
            var currentDevice = _enfluxManager.GetCurrentDevices(DeviceFlag).First();
            if (!currentDevice.IsCalibratingInitiated)
            {
                // If already prompted, start calibration. Otherwise prompt user to confirm starting calibration.
                if (_displayMetadata.PromptedCalibration)
                {
                    _enfluxManager.StartCalibrating(DeviceFlag);
                }
                else
                {
                    StartCalibrationPrompt();
                }
            }
            else
            {
                _enfluxManager.StartScanning(DeviceFlag);
            }
        }

        private void OpenBluetoothManagerButtonOnClick()
        {
            PlatformUtils.LaunchBluetoothManager();
        }

        private void UpdateUi()
        {
            if (_enfluxManager == null)
            {
                return;
            }

            var device = _enfluxManager.GetCurrentDevices(DeviceFlag).FirstOrDefault();
            var isReady = device != null && device.IsReady;
            var isCalibratingInitiated = device != null && device.IsCalibratingInitiated;
            var state = device != null ? device.State : DeviceState.Disconnected;
            var stateColor = StateColors.ResolveColor(state);
            var isBatteryAvailable = device != null && device.PowerInfo.IsBatteryAvailable;
            var isDeviceAvailable = device != null && (device.IsReady || device.IsActive);
            var icon = isDeviceAvailable ? AvailableIcon : UnavailableIcon;

            if (device != null)
            {
                var batteryText = isBatteryAvailable ? string.Format(" ({0})", device.PowerInfo.BatteryLevel) : "";
                _deviceNameText.text = string.Format("{0}{1}", device.Name.ToUpper(), batteryText);
            }
            else
            {
                _deviceNameText.text = DeviceFlag.Nicify();
            }
            _deviceIconImage.sprite = icon;
            _deviceIconImage.color = stateColor;
            _deviceStateText.color = stateColor;
            _deviceStateText.text = state.Nicify();
            _deviceBatteryLevelText.gameObject.SetActive(isBatteryAvailable);
            _toggleCalibratingButton.gameObject.SetActive(isReady || isCalibratingInitiated);
            _toggleCalibratingButton.enabled = !isCalibratingInitiated;
            _deviceStateText.color = stateColor;
            if (isBatteryAvailable)
            {
                _deviceBatteryLevelText.text = string.Format("{0}% Battery", Mathf.RoundToInt(100 * device.PowerInfo.BatteryLevel));
                _deviceBatteryLevelText.color = BatteryLevelColors.ResolveColor(device.PowerInfo);
            }
            if (isCalibratingInitiated)
            {
                _toggleCalibratingText.text = "Stop";
                _toggleCalibratingButton.image.color = CalibrationPromptColors.Normal;
            }
            else
            {
                if (_displayMetadata.PromptedCalibration)
                {
                    _toggleCalibratingText.text = "Confirm To Calibrate";
                    _toggleCalibratingButton.image.color = CalibrationPromptColors.Prompted;
                }
                else
                {
                    _toggleCalibratingText.text = "Calibrate";
                    _toggleCalibratingButton.image.color = CalibrationPromptColors.Normal;
                }
            }
            _openBluetoothManagerButton.gameObject.SetActive(!isDeviceAvailable);
        }

        private void SafelyStopCoroutine(IEnumerator coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        private void ShowTemporaryDeviceMessage(string message)
        {
            SafelyStopCoroutine(_co_updateStateTextUi);
            _co_updateStateTextUi = Co_ShowTemporaryDeviceMessage(message);

            StartCoroutine(_co_updateStateTextUi);
        }

        private IEnumerator Co_ShowTemporaryDeviceMessage(string message)
        {
            _deviceStateText.text = message;
            yield return new WaitForSeconds(ResetUiMessagesTime);
            var currentDevice = _enfluxManager.GetCurrentDevices(DeviceFlag).FirstOrDefault();
            var state = currentDevice != null ? currentDevice.State : DeviceState.Disconnected;
            _deviceStateText.color = StateColors.ResolveColor(state);
            _deviceStateText.text = state.Nicify();
        }

        private void StartCalibrationPrompt()
        {
            StopCalibrationPrompt();
            _displayMetadata.PromptedCalibration = true;
            _co_promptCalibratingPrompt = Co_CalibrationPrompt();
            StartCoroutine(_co_promptCalibratingPrompt);
            UpdateUi();
        }

        private void StopCalibrationPrompt()
        {
            _displayMetadata.PromptedCalibration = false;
            SafelyStopCoroutine(_co_promptCalibratingPrompt);
            _co_promptCalibratingPrompt = null;
            UpdateUi();
        }

        private IEnumerator Co_CalibrationPrompt()
        {
            yield return new WaitForSeconds(ResetUiMessagesTime);
            StopCalibrationPrompt();
        }

        private void SetStatusTextColor(Text text, AlignmentNotification notification)
        {
            text.color = notification.IsError() ? Color.red : Color.white;
        }

        private void SetStatusTextColor(Text text, InputCommands inputCommand)
        {
            text.color = inputCommand.IsError() ? Color.red : Color.white;
        }
    }
}