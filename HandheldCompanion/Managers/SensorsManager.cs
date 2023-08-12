﻿using HandheldCompanion.Controllers;
using HandheldCompanion.Devices;
using HandheldCompanion.Sensors;
using HandheldCompanion.Views;
using Nefarius.Utilities.DeviceManagement.PnP;
using System;
using static HandheldCompanion.Utils.DeviceUtils;

namespace HandheldCompanion.Managers
{
    public static class SensorsManager
    {
        private static IMUGyrometer Gyrometer;
        private static IMUAccelerometer Accelerometer;
        private static SerialUSBIMU USBSensor;

        private static SensorFamily sensorFamily;

        public static bool IsInitialized;

        public static event InitializedEventHandler Initialized;
        public delegate void InitializedEventHandler();

        static SensorsManager()
        {
            DeviceManager.UsbDeviceArrived += DeviceManager_UsbDeviceArrived;
            DeviceManager.UsbDeviceRemoved += DeviceManager_UsbDeviceRemoved;

            ControllerManager.ControllerSelected += ControllerManager_ControllerSelected;
            ControllerManager.ControllerUnplugged += ControllerManager_ControllerUnplugged;

            SettingsManager.SettingValueChanged += SettingsManager_SettingValueChanged;
        }

        private static void ControllerManager_ControllerSelected(IController Controller)
        {
            if (Controller.Capabilities.HasFlag(ControllerCapabilities.MotionSensor))
            {
                // hardcoded, dangerous (0: none, 1: internal, 2: external, 3: controller)
                SettingsManager.SetProperty("SensorSelection", 3);
            }
        }

        private static void ControllerManager_ControllerUnplugged(IController Controller)
        {
            if (sensorFamily != SensorFamily.Controller)
                return;

            if (!Controller.Capabilities.HasFlag(ControllerCapabilities.MotionSensor))
                return;

            // restore default sensor
            if (MainWindow.CurrentDevice.Capabilities.HasFlag(DeviceCapabilities.InternalSensor))
                SettingsManager.SetProperty("SensorSelection", 1);
            else if (MainWindow.CurrentDevice.Capabilities.HasFlag(DeviceCapabilities.ExternalSensor))
                SettingsManager.SetProperty("SensorSelection", 2);
            else
                SettingsManager.SetProperty("SensorSelection", 0);
        }

        private static void DeviceManager_UsbDeviceRemoved(PnPDevice device, DeviceEventArgs obj)
        {
            if (USBSensor is null)
                return;

            // If the USB Gyro is unplugged, close serial connection
            USBSensor.Close();
        }

        private static void DeviceManager_UsbDeviceArrived(PnPDevice device, DeviceEventArgs obj)
        {
            // If USB Gyro is plugged, hook into it
            USBSensor = SerialUSBIMU.GetDefault();
        }

        private static void SettingsManager_SettingValueChanged(string name, object value)
        {
            switch (name)
            {
                case "SensorPlacement":
                    {
                        SerialPlacement placement = (SerialPlacement)Convert.ToInt32(value);
                        USBSensor?.SetSensorPlacement(placement);
                    }
                    break;
                case "SensorPlacementUpsideDown":
                    {
                        bool upsidedown = Convert.ToBoolean(value);
                        USBSensor?.SetSensorOrientation(upsidedown);
                    }
                    break;
                case "SensorSelection":
                    {
                        SensorFamily sensorSelection = (SensorFamily)Convert.ToInt32(value);

                        // skip if set already
                        if (sensorFamily == sensorSelection)
                            return;

                        // In case current selection is USG Gyro, close serial connection
                        if (sensorSelection == SensorFamily.SerialUSBIMU)
                            if (USBSensor is not null)
                                USBSensor.Close();

                        sensorFamily = sensorSelection;

                        // Establish serial port connection on selection change to USG Gyro
                        if (sensorSelection == SensorFamily.SerialUSBIMU)
                        {
                            USBSensor = SerialUSBIMU.GetDefault();

                            if (USBSensor is null)
                                break;

                            USBSensor.Open();
                        }

                        // if required, halt gyrometer
                        if (Gyrometer is not null)
                            Gyrometer.StopListening();

                        // if required, halt accelerometer
                        if (Accelerometer is not null)
                            Accelerometer?.StopListening();

                        SetSensorFamily(sensorSelection);
                    }
                    break;
            }
        }

        public static void Start()
        {
            IsInitialized = true;
            Initialized?.Invoke();

            LogManager.LogInformation("{0} has started", "SensorsManager");
        }

        public static void Stop()
        {
            if (!IsInitialized)
                return;

            IsInitialized = false;

            LogManager.LogInformation("{0} has stopped", "SensorsManager");
        }

        public static void Resume(bool update)
        {
            if (Gyrometer is not null)
                Gyrometer.UpdateSensor();

            if (Accelerometer is not null)
                Accelerometer.UpdateSensor();
        }

        public static void UpdateReport(ControllerState controllerState)
        {
            if (sensorFamily == SensorFamily.None)
                return;

            if (Gyrometer is not null)
                controllerState.GyroState.Gyroscope = Gyrometer.GetCurrentReading();

            if (Accelerometer is not null)
                controllerState.GyroState.Accelerometer = Accelerometer.GetCurrentReading();
        }

        public static void SetSensorFamily(SensorFamily sensorFamily)
        {
            // initialize sensors
            var UpdateInterval = TimerManager.GetPeriod();

            Gyrometer = new IMUGyrometer(sensorFamily, UpdateInterval);
            Accelerometer = new IMUAccelerometer(sensorFamily, UpdateInterval);
        }
    }
}
