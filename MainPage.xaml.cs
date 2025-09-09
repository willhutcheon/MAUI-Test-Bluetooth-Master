using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel;

namespace MAUI_Test_Bluetooth
{
    public partial class MainPage : ContentPage
    {
        private readonly IAdapter _bluetoothAdapter;
        private readonly IBluetoothLE _bluetoothLE;
        public ObservableCollection<IDevice> Devices { get; set; } = new();
        public MainPage()
        {
            InitializeComponent();
            _bluetoothLE = CrossBluetoothLE.Current;
            _bluetoothAdapter = CrossBluetoothLE.Current.Adapter;
            DevicesListView.ItemsSource = Devices;
        }
        //private async void OnScanClicked(object sender, EventArgs e)
        //{
        //    // Check and request Bluetooth permissions
        //    var permissionStatus = await RequestBluetoothPermissions();
        //    if (permissionStatus != PermissionStatus.Granted)
        //    {
        //        await DisplayAlert("Permission Denied", "Bluetooth permissions are required to scan for devices", "OK");
        //        return;
        //    }
        //    if (!_bluetoothLE.IsOn)
        //    {
        //        await DisplayAlert("Bluetooth Off", "Please enable Bluetooth", "OK");
        //        return;
        //    }
        //    Devices.Clear();
        //    _bluetoothAdapter.DeviceDiscovered += (s, a) =>
        //    {
        //        if (!Devices.Contains(a.Device))
        //        {
        //            MainThread.BeginInvokeOnMainThread(() =>
        //            {
        //                Devices.Add(a.Device);
        //            });
        //        }
        //    };
        //    try
        //    {
        //        await _bluetoothAdapter.StartScanningForDevicesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Scan Error", $"Failed to start scanning: {ex.Message}", "OK");
        //    }
        //}
        private async void OnScanClicked(object sender, EventArgs e)
        {
            // Disable the button while scanning
            ScanButton.IsEnabled = false;
            ScanButton.Text = "Scanning...";
            try
            {
                // Run your scan logic
                var permissionStatus = await RequestBluetoothPermissions();
                if (permissionStatus != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Denied", "Bluetooth permissions are required", "OK");
                    return;
                }
                if (!_bluetoothLE.IsOn)
                {
                    await DisplayAlert("Bluetooth Off", "Please enable Bluetooth", "OK");
                    return;
                }
                Devices.Clear();
                _bluetoothAdapter.DeviceDiscovered += (s, a) =>
                {
                    // Ensure the device has a non-null name and matches the desired prefix
                    if (!string.IsNullOrEmpty(a.Device.Name) && a.Device.Name.StartsWith("PUB"))
                    {
                        if (!Devices.Contains(a.Device))
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                Devices.Add(a.Device);
                            });
                        }
                    }
                };
                await _bluetoothAdapter.StartScanningForDevicesAsync();
                await DisplayAlert("Scan Complete", $"{Devices.Count} devices found.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to scan: {ex.Message}", "OK");
            }
            finally
            {
                // Re-enable after scanning
                ScanButton.IsEnabled = true;
                ScanButton.Text = "Scan";
            }
        }
        private async Task<PermissionStatus> RequestBluetoothPermissions()
        {
            try
            {
#if ANDROID
                // For Android 12+ (API 31+), we need BLUETOOTH_SCAN and BLUETOOTH_CONNECT
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
                {
                    //var scanPermission = await Permissions.RequestAsync<MAUI_Test_Bluetooth.Platforms.Android.BluetoothScanPermission>();
                    var scanPermission = await Permissions.RequestAsync<Platforms.Android.Permissions.BluetoothScanPermission>();
                    if (scanPermission != PermissionStatus.Granted)
                        return scanPermission;
                    var connectPermission = await Permissions.RequestAsync<Platforms.Android.Permissions.BluetoothConnectPermission>();
                    if (connectPermission != PermissionStatus.Granted)
                        return connectPermission;
                }
                else
                {
                    // For older Android versions, we need location permissions
                    var locationPermission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (locationPermission != PermissionStatus.Granted)
                        return locationPermission;
                }
#endif
                return PermissionStatus.Granted;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Permission Error", $"Failed to request permissions: {ex.Message}", "OK");
                return PermissionStatus.Denied;
            }
        }
        private void OnClearClicked(object sender, EventArgs e)
        {
            Devices.Clear();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Stop scanning when leaving the page
            if (_bluetoothAdapter.IsScanning)
            {
                //_bluetoothAdapter.StopScanningForDevices();
            }
        }
        private void OnKeypadButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                KeypadDisplay.Text += btn.Text;
            }
        }
        private void OnClearKeypadClicked(object sender, EventArgs e)
        {
            KeypadDisplay.Text = string.Empty;
        }
        private void OnBackspaceClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(KeypadDisplay.Text))
            {
                KeypadDisplay.Text = KeypadDisplay.Text.Substring(0, KeypadDisplay.Text.Length - 1);
            }
        }
    }
}