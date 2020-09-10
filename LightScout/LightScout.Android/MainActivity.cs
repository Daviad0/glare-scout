using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Util;
using Android.Support.V4.App;
using Android.Content.PM;
using Android;
using System.Linq;

[assembly: UsesFeature("android.hardware.usb.host")]


namespace LightScout.Droid
{
    [Activity(Label = "LightScout", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        static readonly string TAG = typeof(MainActivity).Name;
        const string ACTION_USB_PERMISSION = "com.hoho.android.usbserial.examples.USB_PERMISSION";

        public const string EXTRA_TAG = "PortInfo";
        const int READ_WAIT_MILLIS = 200;
        const int WRITE_WAIT_MILLIS = 200;

        UsbManager usbManager;
        ListView listView;
        TextView progressBarTitle;
        ProgressBar progressBar;

        //UsbSerialPortAdapter adapter;
        BroadcastReceiver detachedReceiver;
        IUsbSerialPort selectedPort;

        Hoho.Android.UsbSerial.Util.SerialInputOutputManager serialIoManager;

        //UsbSerialPort port;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            base.OnCreate(savedInstanceState);
            usbManager = GetSystemService(Context.UsbService) as UsbManager;
            ZXing.Net.Mobile.Forms.Android.Platform.Init();

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            var permissions = new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage, Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation, Manifest.Permission.Bluetooth, Manifest.Permission.BluetoothAdmin, Manifest.Permission.Camera };
            try
            {
                ActivityCompat.RequestPermissions(this, permissions, 1);
            }
            catch(Exception ex)
            {

            }
            
            
        }
        /*protected override async void OnResume()
        {
            base.OnResume();
            var portInfo = Intent.GetParcelableExtra(EXTRA_TAG) as Hoho.Android.UsbSerial.Util.UsbSerialPortInfo;
            if(portInfo != null)
            {
                int vendorId = portInfo.VendorId;
                int deviceId = portInfo.DeviceId;
                int portNumber = portInfo.PortNumber;

                Log.Info(TAG, string.Format("VendorId: {0} DeviceId: {1} PortNumber: {2}", vendorId, deviceId, portNumber));

                var drivers = await MainActivity.FindAllDriversAsync(usbManager);
                var driver = drivers.Where((d) => d.Device.VendorId == vendorId && d.Device.DeviceId == deviceId).FirstOrDefault();
                if (driver == null)
                    throw new Exception("Driver specified in extra tag not found.");

                port = (UsbSerialPort)driver.Ports[portNumber];
                if (port == null)
                {
                    return;
                }
                Log.Info(TAG, "port=" + port);


                serialIoManager = new Hoho.Android.UsbSerial.Util.SerialInputOutputManager((IUsbSerialPort)port)
                {
                    BaudRate = 115200,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Parity = Parity.None,
                };
                serialIoManager.DataReceived += (sender, e) => {
                    RunOnUiThread(() => {
                        UpdateReceivedData(e.Data);
                    });
                };
                serialIoManager.ErrorReceived += (sender, e) => {
                    RunOnUiThread(() => {
                        var intent = new Intent(this, typeof(MainActivity));
                        StartActivity(intent);
                    });
                };

                Log.Info(TAG, "Starting IO manager ..");
                try
                {
                    serialIoManager.Open(usbManager);
                }
                catch (Java.IO.IOException e)
                {
                    return;
                }
                adapter = new UsbSerialPortAdapter(this);
                listView.Adapter = adapter;

                listView.ItemClick += async (sender, e) => {
                    await OnItemClick(sender, e);
                };

                await PopulateListAsync();

                //register the broadcast receivers
                detachedReceiver = new UsbDeviceDetachedReceiver(this);
                RegisterReceiver(detachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));
            }
            
        }
        protected override void OnPause()
        {
            if (serialIoManager != null && serialIoManager.IsOpen)
            {
                Log.Info(TAG, "Stopping IO manager ..");
                try
                {
                    serialIoManager.Close();
                }
                catch (Java.IO.IOException)
                {
                    // ignore
                }
            }
            base.OnPause();

            // unregister the broadcast receivers
            var temp = detachedReceiver; // copy reference for thread safety
            if (temp != null)
                UnregisterReceiver(temp);
        }
        internal static Task<IList<IUsbSerialDriver>> FindAllDriversAsync(UsbManager usbManager)
        {
            // using the default probe table
            // return UsbSerialProber.DefaultProber.FindAllDriversAsync (usbManager);

            // adding a custom driver to the default probe table
            var table = UsbSerialProber.DefaultProbeTable;

            var prober = new UsbSerialProber(table);
            return (Task<IList<IUsbSerialDriver>>)prober.FindAllDrivers(usbManager);
        }

        async Task OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Log.Info(TAG, "Pressed item " + e.Position);
            if (e.Position >= adapter.Count)
            {
                Log.Info(TAG, "Illegal position.");
                return;
            }

            // request user permission to connect to device
            // NOTE: no request is shown to user if permission already granted
            selectedPort = (IUsbSerialPort)adapter.GetItem(e.Position);
            var permissionGranted = await usbManager.RequestPermissionAsync(selectedPort.Driver.Device, this);
            if (permissionGranted)
            {
                // start the SerialConsoleActivity for this device
                var newIntent = new Intent(this, typeof(MainActivity));
                newIntent.PutExtra(MainActivity.EXTRA_TAG, new Hoho.Android.UsbSerial.Util.UsbSerialPortInfo((IUsbSerialPort)selectedPort));
                StartActivity(newIntent);
            }
        }

        async Task PopulateListAsync()
        {

            Log.Info(TAG, "Refreshing device list ...");

            var drivers = await FindAllDriversAsync(usbManager);

            adapter.Clear();
            foreach (var driver in drivers)
            {
                var ports = driver.Ports;
                Log.Info(TAG, string.Format("+ {0}: {1} port{2}", driver, ports.Count, ports.Count == 1 ? string.Empty : "s"));
                foreach (var port in ports)
                    adapter.Add((UsbSerialPort)port);
            }

            adapter.NotifyDataSetChanged();
            Log.Info(TAG, "Done refreshing, " + adapter.Count + " entries found.");
        }*/

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            ZXing.Net.Mobile.Forms.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        /*#region UsbSerialPortAdapter implementation

        class UsbSerialPortAdapter : ArrayAdapter<UsbSerialPort>
        {
            public UsbSerialPortAdapter(Context context)
                : base(context, global::Android.Resource.Layout.SimpleExpandableListItem2)
            {
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var row = convertView;
                if (row == null)
                {
                    var inflater = Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                    row = inflater.Inflate(global::Android.Resource.Layout.SimpleListItem2, null);
                }

                var port = this.GetItem(position);
                var driver = port.GetDriver();
                var device = driver.GetDevice();

                var title = string.Format("Vendor {0} Product {1}",
                    HexDump.ToHexString((short)device.VendorId),
                    HexDump.ToHexString((short)device.ProductId));
                row.FindViewById<TextView>(global::Android.Resource.Id.Text1).Text = title;

                var subtitle = device.Class.SimpleName;
                row.FindViewById<TextView>(global::Android.Resource.Id.Text2).Text = subtitle;

                return row;
            }
        }

        #endregion

        #region UsbDeviceDetachedReceiver implementation

        class UsbDeviceDetachedReceiver
            : BroadcastReceiver
        {
            readonly string TAG = typeof(UsbDeviceDetachedReceiver).Name;
            readonly MainActivity activity;

            public UsbDeviceDetachedReceiver(MainActivity activity)
            {
                this.activity = activity;
            }

            public async override void OnReceive(Context context, Intent intent)
            {
                var device = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;

                Log.Info(TAG, "USB device detached: " + device.DeviceName);

            }
        }

        #endregion

        void WriteData(byte[] data)
        {
            if (serialIoManager.IsOpen)
            {
                port.Write(data, WRITE_WAIT_MILLIS);
            }
        }

        void UpdateReceivedData(byte[] data)
        {
            var message = "Read " + data.Length + " bytes: \n"
                + HexDump.DumpHexString(data) + "\n\n";

        }*/

    }
}