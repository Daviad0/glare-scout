using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(LightScout.Droid.PeripheralManagement))]
namespace LightScout.Droid
{
    public sealed class ServerManagement
    {
        private static readonly object l1 = new object();
        public static ServerManagement instance = null;
        public static ServerManagement Instance
        {
            get
            {
                lock (l1)
                {
                    if (instance == null)
                    {
                        instance = new ServerManagement();
                    }
                    return instance;
                }

            }
        }

        public static BluetoothGattServer Server;
    }
    public class GattServerCallback : BluetoothGattServerCallback
    {
        
        private ServerManagement ServerManagement = new ServerManagement();
        public override void OnConnectionStateChange(BluetoothDevice device, [GeneratedEnum] ProfileState status, [GeneratedEnum] ProfileState newState)
        {
            base.OnConnectionStateChange(device, status, newState);
        }
        public override void OnCharacteristicReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattCharacteristic characteristic)
        {
            Console.WriteLine("Thingy read");
            // need to somehow pass server here
            if (characteristic.GetValue() != null)
            {
                ServerManagement.Server.SendResponse(device, requestId, GattStatus.Success, Encoding.ASCII.GetBytes("You just gave me " + Encoding.ASCII.GetString(characteristic.GetValue())).ToArray().Length, Encoding.ASCII.GetBytes("You just gave me " + Encoding.ASCII.GetString(characteristic.GetValue())).ToArray());
            }
            else
            {
                ServerManagement.Server.SendResponse(device, requestId, GattStatus.Success, Encoding.ASCII.GetBytes("Feed cookies below NOM NOM NOM!").ToArray().Length, Encoding.ASCII.GetBytes("Feed cookies below NOM NOM NOM!").ToArray());
            }
            
            base.OnCharacteristicReadRequest(device, requestId, offset, characteristic);
        }
        public override void OnCharacteristicWriteRequest(BluetoothDevice device, int requestId, BluetoothGattCharacteristic characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
        {
            Console.WriteLine("Thingy wrtten");
            MessagingCenter.Send("MasterPage", "DataGot", Encoding.ASCII.GetString(value));
            ServerManagement.Server.SendResponse(device, requestId, GattStatus.Success, Encoding.ASCII.GetBytes("Nom Nom Nom, thanks for the Bytes!").ToArray().Length, Encoding.ASCII.GetBytes("Nom Nom Nom, thanks for the Bytes!").ToArray());
            characteristic.SetValue(value);
            base.OnCharacteristicWriteRequest(device, requestId, characteristic, preparedWrite, responseNeeded, offset, value);
        }
    }
    public class AdvertiserCallback : AdvertiseCallback
    {
        public override void OnStartSuccess(AdvertiseSettings settingsInEffect)
        {
            Console.WriteLine(settingsInEffect.IsConnectable);
            base.OnStartSuccess(settingsInEffect);
        }
        public override void OnStartFailure([GeneratedEnum] AdvertiseFailure errorCode)
        {
            Console.WriteLine(errorCode.ToString());
            base.OnStartFailure(errorCode);
        }
    }


    class PeripheralManagement : BLEPeripheral
    {
        private ServerManagement serverManagement = new ServerManagement();
        private AdvertiseCallback callback;
        public void StartAdvertising(string serviceUUID, string serviceName)
        {
            AdvertiseSettings settings = new AdvertiseSettings.Builder().SetConnectable(true).Build();

            BluetoothAdapter.DefaultAdapter.SetName("GBP4.1-"+serviceName);

            ParcelUuid parcelUuid = new ParcelUuid(UUID.FromString(serviceUUID));
            AdvertiseData data = new AdvertiseData.Builder().SetIncludeDeviceName(true).SetIncludeTxPowerLevel(true).Build();

            this.callback = new AdvertiserCallback();
            BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser.StartAdvertising(settings, data, callback);

            BluetoothGattCharacteristic chara = new BluetoothGattCharacteristic(
                UUID.FromString("00000001-0000-1000-8000-00805f9b34fb"),
                GattProperty.Read | GattProperty.Write | GattProperty.Notify,
                GattPermission.Read | GattPermission.Write
            );
            BluetoothGattService service = new BluetoothGattService(
                UUID.FromString("00000862-0000-1000-8000-00805f9b34fb"),
                GattServiceType.Primary
            );
            service.AddCharacteristic(chara);

            BluetoothManager manager = (BluetoothManager)Android.App.Application.Context.GetSystemService(Context.BluetoothService);
            BluetoothGattServer server = manager.OpenGattServer(Android.App.Application.Context, new GattServerCallback());
            ServerManagement.Server = server;
            server.AddService(service);
        }
    }
}