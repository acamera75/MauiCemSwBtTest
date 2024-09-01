 
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace MauiCemSwBtTest
{
    public partial class ViewModel : ObservableObject
    {
        #region BLE
        private IBluetoothLE ble;
        private IAdapter adapter;

        private Guid MyGuid => new Guid("19b10000-e8f2-537e-4f6c-d104768a1214");

        //private IDevice? device => deviceList?.FirstOrDefault(x=>x.Id == new Guid("19b10000-e8f2-537e-4f6c-d104768a1214"));
        private IDevice? device => deviceList?.FirstOrDefault(x => x.Name == "Arduino" || x.Name == "LED-Portenta-01");

        private List<IDevice> deviceList;

        private ICharacteristic characteristic;

        //public ViewModel()
        //{

        //}

        private async void StartBle()
        {
            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;

            var state = ble.State;
            deviceList = new List<IDevice>();

            adapter.DeviceDiscovered += (s, a) => deviceList.Add(a.Device);

            await adapter.StartScanningForDevicesAsync();

            //try
            //{
            //    await adapter.ConnectToDeviceAsync(device);
            //}
            //catch (DeviceConnectionException ex)
            //{
            //    //specific
            //}
            //catch (Exception ex)
            //{
            //    //generic
            //}
        }


        #endregion

        int count = 0;

        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private bool isOnLed;
        [ObservableProperty] private string bttContent = $"Clicked 0 time";
        [ObservableProperty] private string bttContentLed = $"init";

        [RelayCommand]
        private async Task Test()
        {
            IsBusy = true;

            await Task.Run(() => IncrementCount());

            IsBusy = false;
        }

        [RelayCommand]
        private async Task Led()
        {
            IsBusy = true;

            await Task.Run(() => SwitchLed());

            IsBusy = false;
        }

        private async Task WriteLed()
        {
            if (device != null)
            {
                //// Connetti al dispositivo
                //await adapter.ConnectToDeviceAsync(device);

                if (characteristic == null)
                {
                    // Ottieni la caratteristica a cui vuoi scrivere
                    var ser = await device.GetServiceAsync(MyGuid);
                    characteristic = await ser.GetCharacteristicAsync(MyGuid);

                    characteristic.ValueUpdated += (s, e) =>
                    {
                        IsOnLed = e?.Characteristic?.Value[0] == 0;

                        var led = IsOnLed ? "ON" : "OFF";

                        BttContentLed = $"Now led is {led}";
                    };
                }

                // Scrivi il valore booleano (true o false)

                await characteristic?.WriteAsync([(byte)(IsOnLed ? 0 : 1)]);

                IsOnLed = !IsOnLed;

            }
        }

        private void IncrementCount()
        {
            count++;

            Thread.Sleep(1000);

            if (count == 1)
                BttContent = $"Clicked {count} time";
            else
                BttContent = $"Clicked {count} times";

        }

        private void SwitchLed()
        {
            var names = deviceList?.Select(x => x.Name).ToList();

            if (device != null)
            {
                if (adapter.ConnectedDevices.Count > 0)
                {

                    WriteLed();
                }
                else
                {
                    adapter.ConnectToDeviceAsync(device);
                    Thread.Sleep(500);
                    WriteLed();
                }
            }

            if (ble == null || device == null)
            {
                StartBle();
            }

           
        }
    }
}