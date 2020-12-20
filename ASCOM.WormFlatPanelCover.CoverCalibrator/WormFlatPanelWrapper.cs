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
        public enum BRIGHTNESS { HIGH, LOW }

        public int device_handle = 0;
        List<string> flatpanel_serial_numbers = new List<string>();

        public string[] SerialNumbers
        {
            get { return flatpanel_serial_numbers.ToArray(); }
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
                MessageBox.Show("未能连接平场板设备【" + Driver.lastUsedFlatPanelSerial + "】", "平场板设备", 
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
        }

        public int OpenDeviceHandle(string serialnumber)
        {
            if (serialnumber != "")
            {
                int retval = 1; //  dummy device handle for simulation mode
                if (!IsSimulation)
                    retval = UsbRelayDeviceHelper.OpenWithSerialNumber(serialnumber, serialnumber.Length);
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
            LogMessage("FlatPanel", "Flat panel turned off.");
            return true;
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
                }
                else if (brightness == BRIGHTNESS.LOW)
                {
                    LogMessage("FlatPanel", "Flat panel Relay #1 opened.");
                    LogMessage("FlatPanel", "Flat panel Relay #2 closed.");
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

                int retval = 0;
                if (brightness == BRIGHTNESS.HIGH)
                {
                    retval = UsbRelayDeviceHelper.CloseOneRelayChannel(device_handle, 1);
                    switch (retval)
                    {
                        case 0:
                            LogMessage("FlatPanel", "Flat panel Relay #1 closed.");
                            break;
                        case 1:
                            LogMessage("FlatPanel", "Error occured when closing Flat panel Relay #1.");
                            return false;
                        case 2:
                            LogMessage("FlatPanel", "'Index out of range' when closing Flat panel Relay #1.");
                            return false;
                    }

                    retval = UsbRelayDeviceHelper.OpenOneRelayChannel(device_handle, 2);
                    switch (retval)
                    {
                        case 0:
                            LogMessage("FlatPanel", "Flat panel Relay #2 opened.");
                            break;
                        case 1:
                            LogMessage("FlatPanel", "Error occured when opening Flat panel Relay #2.");
                            return false;
                        case 2:
                            LogMessage("FlatPanel", "'Index out of range' when opening Flat panel Relay #2.");
                            return false;
                    }

                }
                else if (brightness == BRIGHTNESS.LOW)
                {
                    retval = UsbRelayDeviceHelper.OpenOneRelayChannel(device_handle, 1);
                    switch (retval)
                    {
                        case 0:
                            LogMessage("FlatPanel", "Flat panel Relay #1 opened.");
                            break;
                        case 1:
                            LogMessage("FlatPanel", "Error occured when opening Flat panel Relay #1.");
                            return false;
                        case 2:
                            LogMessage("FlatPanel", "'Index out of range' when opening Flat panel Relay #1.");
                            return false;
                    }

                    retval = UsbRelayDeviceHelper.CloseOneRelayChannel(device_handle, 2);
                    switch (retval)
                    {
                        case 0:
                            LogMessage("FlatPanel", "Flat panel Relay #2 closed.");
                            break;
                        case 1:
                            LogMessage("FlatPanel", "Error occured when closing Flat panel Relay #2.");
                            return false;
                        case 2:
                            LogMessage("FlatPanel", "'Index out of range' when closing Flat panel Relay #2.");
                            return false;
                    }
                }
                return true;
            }
        }
    }
}
