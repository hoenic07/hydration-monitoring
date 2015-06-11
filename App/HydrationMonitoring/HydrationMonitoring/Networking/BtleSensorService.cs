using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.Devices.Sensors;
using Windows.Foundation;

namespace HydrationMonitoring.Networking
{
    public class BtleSensorService
    {
        public DeviceInformation Device { get; private set; }
        private GattCharacteristic _dataChara;
        private GattCharacteristic _configChara;
        private GattCharacteristic _periodChara;

        public event TypedEventHandler<BtleSensorService, BtleAccelerometerReading> BtleAccelerometerReadingUpdated;


        private static BtleSensorService _instance;

        public static BtleSensorService Instance
        {
            get
            {
                if (_instance == null) _instance = new BtleSensorService();
                return _instance;
            }
        }

        private BtleSensorService() { }


        /// <summary>
        /// Connects to a TI SensorTag and registers to notifications of the accelerometer
        /// Also configures the device to send updates every 100ms and values in +/-2g
        /// </summary>
        /// <returns></returns>
        public async Task InitDevice()
        {
            if (Device != null) return;
            var res = await BtleCommunication.GetDeviceIdFromUuidAndFilter(SensorTagServices.Accelerometer, string.Empty);

            if (res != null)
            {
                Device = res;
                _configChara = await BtleCommunication.GetCharacteristics(res, SensorTagCharacteristics.AccelerometerConfig);
                _dataChara = await BtleCommunication.GetCharacteristics(res, SensorTagCharacteristics.AccelerometerData);
                _periodChara = await BtleCommunication.GetCharacteristics(res, SensorTagCharacteristics.AccelerometerPeriod);
                await BtleCommunication.SubscribeForNotification(_dataChara, HandleNotifications);

                await BtleCommunication.WriteValue(_periodChara, new byte[] { 10 });
                await BtleCommunication.WriteValue(_configChara, new byte[] { 01 });

            }

        }

        /// <summary>
        /// Gets raw values from the SensorTag and converts them to accelerometer readings
        /// </summary>
        /// <param name="characteristic"></param>
        /// <param name="args"></param>
        private void HandleNotifications(GattCharacteristic characteristic, GattValueChangedEventArgs args)
        {
            byte[] forceData = new byte[args.CharacteristicValue.Length];
            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(forceData);
            if (BtleAccelerometerReadingUpdated != null)
            {
                BtleAccelerometerReadingUpdated(this, new BtleAccelerometerReading
                {
                    RawX = (sbyte)forceData[0],
                    RawY = (sbyte)forceData[1],
                    RawZ = (sbyte)forceData[2]
                });
            }

            string dataStr1 = string.Empty;
            foreach (var b in forceData)
            {
                dataStr1 += b + ",";
            }

        }

        private sbyte ToCorrectVal(byte val)
        {
            return (sbyte)val;
        }

    }
}
