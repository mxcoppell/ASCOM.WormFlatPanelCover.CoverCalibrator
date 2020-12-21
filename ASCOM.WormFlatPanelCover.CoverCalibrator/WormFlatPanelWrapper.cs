using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ASCOM.WormFlatPanelCover
{
    class WormFlatPanelWrapper : WormDeviceWrapperCommon
    {
        public enum BRIGHTNESS { HIGH = 2, LOW = 1 }
        public int MaximumBrightness { get { return (int)BRIGHTNESS.HIGH; } }

        public int device_handle = 0;
        List<string> flatpanel_serial_numbers = new List<string>();

        public string[] SerialNumbers
        {
            get { return flatpanel_serial_numbers.ToArray(); }
        }

        public bool IsOpen
        {
            get { return (device_handle != 0); }
        }

        public WormFlatPanelWrapper(CoverCalibrator drv, bool is_simulation) : base(drv, true)
        {
            if (IsSimulation)
                LogMessage("FlatPanel", "Simulation mode is ON.");

            GetSerialNumbers();
        }
        ~WormFlatPanelWrapper()
        {
            Disconnect();
        }

        public new bool Connect()
        {
            Disconnect();
            device_handle = OpenDeviceHandle(Driver.lastUsedFlatPanelSerial);
            if (device_handle == 0) {
                LogMessage("FlatPanel", "Failed to connect flat panel device ({0}).", Driver.lastUsedFlatPanelSerial);
                MessageBox.Show("未能连接平场板设备【" + Driver.lastUsedFlatPanelSerial + "】", "虫子电动平场镜头盖", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            LogMessage("FlatPanel", "Flat panel device connected. ({0})", Driver.lastUsedFlatPanelSerial);
            return true;
        }
        public new bool Disconnect()
        {
            if (!IsSimulation)
            {
                if (device_handle != 0)
                    UsbRelayDeviceHelper.Close(device_handle);
            }
            Driver.currentFlatPanelBrightness = 0;
            LogMessage("FlatPanel", "Flat panel device disconnected. ({0})", Driver.lastUsedFlatPanelSerial);
            return true;
        }

        public void GetSerialNumbers()
        {
            flatpanel_serial_numbers.Clear();

            if (IsSimulation)
            {
                flatpanel_serial_numbers.Add("WormFlatPanel_#1");
                LogMessage("FlatPanel", "Added usb relay device serial number (WormFlatPanel_#1).");
                flatpanel_serial_numbers.Add("WormFlatPanel_#2");
                LogMessage("FlatPanel", "Added usb relay device serial number (WormFlatPanel_#2).");
            }
            else
            {
                if (UsbRelayDeviceHelper.Init() == 0)
                {
                    //  retrieve all usb relay devices
                    UsbRelayDeviceHelper.UsbRelayDeviceInfo usb_relay_it = null;
                    usb_relay_it = UsbRelayDeviceHelper.Enumerate();

                    while (usb_relay_it != null)
                    {
                        flatpanel_serial_numbers.Add(usb_relay_it.SerialNumber);
                        LogMessage("FlatPanel", "Added usb relay device serial number ({0}).", usb_relay_it.SerialNumber);
                        IntPtr next_it = usb_relay_it.Next;
                        usb_relay_it = (UsbRelayDeviceHelper.UsbRelayDeviceInfo)Marshal.PtrToStructure(
                            next_it, typeof(UsbRelayDeviceHelper.UsbRelayDeviceInfo));
                    }
                }
            }
            LogMessage("FlatPanel", "Available flat panels: {0}", string.Join(",", flatpanel_serial_numbers));
        }

        public int OpenDeviceHandle(string serialnumber)
        {
            if (serialnumber != "")
            {
                int retval = 1; //  dummy device handle for simulation mode
                if (!IsSimulation)
                    retval = UsbRelayDeviceHelper.OpenWithSerialNumber(serialnumber, serialnumber.Length);
                else
                {
                    // emulate device failure scenario
                    if (serialnumber == "WormFlatPanel_#1")
                        retval = 0;
                }
                LogMessage("FlatPanel", "Got device handle ({0}) with usb relay device serial number ({1}).", retval, serialnumber);
                return retval;
            }
            LogMessage("FlatPanel", "Empty usb relay device serial number.");
            return 0;
        }
        public bool TurnOff()
        {
            if (!IsSimulation)
            {
                if (device_handle == 0)
                {
                    LogMessage("FlatPanel", "Invalid flat panel device handle.");
                    return false;
                }

                UsbRelayDeviceHelper.CloseOneRelayChannel(device_handle, 1);
                LogMessage("FlatPanel", "Flat panel Relay #1 closed.");
                UsbRelayDeviceHelper.CloseOneRelayChannel(device_handle, 2);
                LogMessage("FlatPanel", "Flat panel Relay #2 closed.");
            }
            else
            {
                LogMessage("FlatPanel", "Flat panel Relay #1 closed.");
                LogMessage("FlatPanel", "Flat panel Relay #2 closed.");
            }
            Driver.currentFlatPanelBrightness = 0;
            LogMessage("FlatPanel", "Flat panel turned off.");
            return true;
        }

        private bool helperOpenChannel(int channel)
        {
            bool retval = false;
            switch (UsbRelayDeviceHelper.OpenOneRelayChannel(device_handle, channel))
            {
                case 0:
                    LogMessage("FlatPanel", "Flat panel Relay #{0} opened.", channel);
                    retval = true;
                    break;
                case 1:
                    LogMessage("FlatPanel", "Error occured when opening Flat panel Relay #{0}.", channel);
                    break;
                case 2:
                    LogMessage("FlatPanel", "'Index out of range' when opening Flat panel Relay #{0}.", channel);
                    break;
            }
            return retval;
        }

        private bool helperCloseChannel(int channel)
        {
            bool retval = false;
            switch (UsbRelayDeviceHelper.CloseOneRelayChannel(device_handle, channel))
            {
                case 0:
                    LogMessage("FlatPanel", "Flat panel Relay #{0} closed.", channel);
                    retval = true;
                    break;
                case 1:
                    LogMessage("FlatPanel", "Error occured when closing Flat panel Relay #{0}.", channel);
                    break;
                case 2:
                    LogMessage("FlatPanel", "'Index out of range' when closing Flat panel Relay #{0}.", channel);
                    break;
            }
            return retval;
        }

        public bool TurnOn(BRIGHTNESS brightness)
        {
            LogMessage("FlatPanel", "Turning on flat panel in mode: {0}", brightness);
            if (IsSimulation)
            {
                if (brightness == BRIGHTNESS.HIGH)
                {
                    LogMessage("FlatPanel", "Flat panel Relay #1 closed.");
                    LogMessage("FlatPanel", "Flat panel Relay #2 opened.");
                    Driver.currentFlatPanelBrightness = 2;
                }
                else if (brightness == BRIGHTNESS.LOW)
                {
                    LogMessage("FlatPanel", "Flat panel Relay #1 opened.");
                    LogMessage("FlatPanel", "Flat panel Relay #2 closed.");
                    Driver.currentFlatPanelBrightness = 1;
                }
                return true;
            }
            else
            {
                if (device_handle == 0)
                {
                    LogMessage("FlatPanel", "Invalid flat panel device handle.");
                    return false;
                }

                if (brightness == BRIGHTNESS.HIGH)
                {
                    if (helperCloseChannel(1) && helperOpenChannel(2))
                        Driver.currentFlatPanelBrightness = 2;
                    else
                    {
                        TurnOff();
                        Driver.currentFlatPanelBrightness = 0;
                    }
                }
                else if (brightness == BRIGHTNESS.LOW)
                {
                    if (helperOpenChannel(1) && helperCloseChannel(2))
                        Driver.currentFlatPanelBrightness = 1;
                    else
                    {
                        TurnOff();
                        Driver.currentFlatPanelBrightness = 0;
                    }
                }
                return true;
            }
        }
    }
}
