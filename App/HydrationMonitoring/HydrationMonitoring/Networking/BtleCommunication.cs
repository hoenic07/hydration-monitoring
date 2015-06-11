using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace HydrationMonitoring.Networking
{
    internal class BtleCommunication
    {
        //TODO: Test if it works for smaller values (eg. 20ms)
        public static readonly int DELAY_BETWEEN_WRITE_PACKAGES = 70;

        /// <summary>
        /// Reads a value from a BTLE devie from a guid
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="chara"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadValue(DeviceInformation dev, Guid chara)
        {
            var charact = await GetCharacteristics(dev, chara);

            if (charact == null) return null;

            return await ReadValue(charact);
        }

        public static async Task<byte[]> ReadValue(GattCharacteristic characteristics)
        {
            try
            {
                //Read
                if (characteristics.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read))
                {
                    var result = await characteristics.ReadValueAsync(BluetoothCacheMode.Uncached);

                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        byte[] forceData = new byte[result.Value.Length];
                        DataReader.FromBuffer(result.Value).ReadBytes(forceData);
                        return forceData;
                    }
                    else
                    {
                        Debug.WriteLine(result.Status.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Writes a value to a characterisitic
        /// </summary>
        /// <param name="characteristics"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<bool> WriteValue(GattCharacteristic characteristics, byte[] data)
        {
            return await WriteValue(characteristics, new List<byte[]>() { data });
        }

        public static async Task<bool> WriteValue(GattCharacteristic characteristics, List<byte[]> data)
        {
            try
            {
                byte data1 = 01;
                var buffer = new byte[] { data1 }.AsBuffer();
                //Write
                if (characteristics.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write))
                {
                    GattCommunicationStatus result = GattCommunicationStatus.Unreachable;
                    foreach (var item in data)
                    {
                        result = await characteristics.WriteValueAsync(item.AsBuffer(), GattWriteOption.WriteWithResponse);

                        await Task.Delay(TimeSpan.FromMilliseconds(DELAY_BETWEEN_WRITE_PACKAGES));

                        if (result != GattCommunicationStatus.Success) return false;
                    }

                    Debug.WriteLine("BTLE > Write result: " + result);

                    return result == GattCommunicationStatus.Success;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Subscribes for notifications for a characteristic
        /// </summary>
        /// <param name="characteristics"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static async Task SubscribeForNotification(GattCharacteristic characteristics, TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> func)
        {
            try
            {
                //Notify
                if (characteristics.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    var res = await characteristics.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    if (res == GattCommunicationStatus.Success)
                    {
                        characteristics.ValueChanged += func;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Unsubscribes the notification handler
        /// </summary>
        /// <param name="characteristics"></param>
        /// <param name="func"></param>
        public static void UnsubscribeFromNotification(GattCharacteristic characteristics, TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> func)
        {
            try
            {
                characteristics.ValueChanged -= func;
                characteristics = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Gets the characteristic from a device and guid
        /// </summary>
        /// <param name="device"></param>
        /// <param name="charId"></param>
        /// <returns></returns>
        public static async Task<GattCharacteristic> GetCharacteristics(DeviceInformation device, Guid charId)
        {
            try
            {
                //if the device is not valid, return null
                if (device == null) return null;
                var gattDeviceService = await GattDeviceService.FromIdAsync(device.Id);
                var chara = gattDeviceService.GetCharacteristics(charId);

                var l = gattDeviceService.GetAllCharacteristics().ToList();


                //if the service does not provide the characteristic, return null
                if (chara.Count == 0) return null;

                return chara.First();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Gets (a) connected BTLE device(s) with or without a given name
        /// </summary>
        /// <param name="id"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static async Task<DeviceInformation> GetDeviceIdFromUuidAndFilter(Guid id, string text)
        {
            var guid = new Guid("F000AA1004514000B000000000000000");
            var devices = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(guid), new string[] { "System.Devices.ContainerId" });
            if (devices.Count == 0)
            {
                return null;
            }

            var device = devices.FirstOrDefault(a => a.Name.Contains(text));

            return device;
        }

        public static async Task<DeviceInformation[]> GetDevicesIdFromUuidAndFilter(Guid id, string text)
        {
            //var devices = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(id), new string[] { "System.Devices.ContainerId" });
            var devices = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(id), null);
            if (devices.Count == 0)
            {
                return null;
            }

            return devices.ToArray();
        }

        public static async Task<DeviceInformation[]> GetDevicesIdFromUuidAndFilter(Guid id)
        {
            var devices = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(id));
            if (devices.Count == 0)
            {
                return null;
            }

            return devices.ToArray();
        }
    }
}
