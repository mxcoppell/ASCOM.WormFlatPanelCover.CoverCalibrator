using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ASCOM.Utilities;

namespace ASCOM.WormFlatPanelCover
{
    class WormFlatPanelWrapper : WormDeviceWrapperCommon
    {
        public enum BRIGHTNESS { HIGH, LOW }

        public int device_handle = 0;
        List<string> flatpanel_serial_numbers = new List<string>();

        public string TargetSerialNumber { get; set; }

        public string[] SerialNumbers
        {
            get { return flatpanel_serial_numbers.ToArray(); }
        }

        public WormFlatPanelWrapper(TraceLogger logger, bool is_simulation) : base(logger, true)
        {
            if (IsSimulation)
                LogMessage("FlatPanel", "Simulation mode is ON.");

            TargetSerialNumber = "";
            GetSerialNumbers();
            device_handle = OpenDeviceHandle(TargetSerialNumber);
        }
        ~WormFlatPanelWrapper()
        {
            if (!IsSimulation)
            {
                if (device_handle != 0)
                    UsbRelayDeviceHelper.Close(device_handle);
            }
        }

        new bool Connect()
        {
            return true;
        }
        new bool Disconnect()
        {
            return true;
        }

        public void GetSerialNumbers()
        {
            flatpanel_serial_numbers.Clear();

            if (IsSimulation)
            {
                flatpanel_serial_numbers.Add("WormFlatPanel_#1");
                LogMessage("FlatPanel", makeLogStr("Added usb relay device serial number (WormFlatPanel_#1)."));
                flatpanel_serial_numbers.Add("WormFlatPanel_#2");
                LogMessage("FlatPanel", makeLogStr("Added usb relay device serial number (WormFlatPanel_#2)."));
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
                        LogMessage("FlatPanel", makeLogStr("Added usb relay device serial number (" + usb_relay_it.SerialNumber + ")."));
                        IntPtr next_it = usb_relay_it.Next;
                        usb_relay_it = (UsbRelayDeviceHelper.UsbRelayDeviceInfo)Marshal.PtrToStructure(
                            next_it, typeof(UsbRelayDeviceHelper.UsbRelayDeviceInfo));
                    }
                }
            }
            TargetSerialNumber = "";
            if (flatpanel_serial_numbers.Count > 0)
            {
                TargetSerialNumber = flatpanel_serial_numbers.First();
            }
            LogMessage("FlatPanel", makeLogStr("Set default usb relay device serial number (" + TargetSerialNumber + ")."));
        }

        public int OpenDeviceHandle(string serialnumber)
        {
            if (serialnumber != "")
            {
                int retval = 1; //  dummy device handle
                if (!IsSimulation)
                    retval = UsbRelayDeviceHelper.OpenWithSerialNumber(serialnumber, serialnumber.Length);
                LogMessage("FlatPanel", makeLogStr("Got device handle (" + retval + ") with usb relay device serial number (" + serialnumber + ")."));
                return retval;
            }
            LogMessage("FlatPanel", makeLogStr("Empty usb relay device serial number."));
            return 0;
        }
        public bool TurnOff()
        {
            if (!IsSimulation)
            {
                if (device_handle == 0)
                {
                    LogMessage("FlatPanel", makeLogStr("Invalid flat panel device handle."));
                    return false;
                }

                UsbRelayDeviceHelper.CloseOneRelayChannel(device_handle, 1);
                LogMessage("FlatPanel", makeLogStr("Flat panel Relay #1 closed."));
                UsbRelayDeviceHelper.CloseOneRelayChannel(device_handle, 2);
                LogMessage("FlatPanel", makeLogStr("Flat panel Relay #2 closed."));
            }
            else
            {
                LogMessage("FlatPanel", makeLogStr("Flat panel Relay #1 closed."));
                LogMessage("FlatPanel", makeLogStr("Flat panel Relay #2 closed."));
            }
            LogMessage("FlatPanel", makeLogStr("Flat panel turned off."));
            return true;
        }

        public bool TurnOn(BRIGHTNESS brightness)
        {
            LogMessage("FlatPanel", makeLogStr("Turning on flat panel in mode: " + brightness));
            if (IsSimulation)
            {
                if (brightness == BRIGHTNESS.HIGH)
                {
                    LogMessage("FlatPanel", makeLogStr("Flat panel Relay #1 closed."));
                    LogMessage("FlatPanel", makeLogStr("Flat panel Relay #2 opened."));
                }
                else if (brightness == BRIGHTNESS.LOW)
                {
                    LogMessage("FlatPanel", makeLogStr("Flat panel Relay #1 opened."));
                    LogMessage("FlatPanel", makeLogStr("Flat panel Relay #2 closed."));
                }
                return true;
            }
            else
            {
                if (device_handle == 0)
                {
                    LogMessage("FlatPanel", makeLogStr("Invalid flat panel device handle."));
                    return false;
                }

                int retval = 0;
                if (brightness == BRIGHTNESS.HIGH)
                {
                    retval = UsbRelayDeviceHelper.CloseOneRelayChannel(device_handle, 1);
                    switch (retval)
                    {
                        case 0:
                            LogMessage("FlatPanel", makeLogStr("Flat panel Relay #1 closed."));
                            break;
                        case 1:
                            LogMessage("FlatPanel", makeLogStr("Error occured when closing Flat panel Relay #1."));
                            return false;
                        case 2:
                            LogMessage("FlatPanel", makeLogStr("'Index out of range' when closing Flat panel Relay #1."));
                            return false;
                    }

                    retval = UsbRelayDeviceHelper.OpenOneRelayChannel(device_handle, 2);
                    switch (retval)
                    {
                        case 0:
                            LogMessage("FlatPanel", makeLogStr("Flat panel Relay #2 opened."));
                            break;
                        case 1:
                            LogMessage("FlatPanel", makeLogStr("Error occured when opening Flat panel Relay #2."));
                            return false;
                        case 2:
                            LogMessage("FlatPanel", makeLogStr("'Index out of range' when opening Flat panel Relay #2."));
                            return false;
                    }

                }
                else if (brightness == BRIGHTNESS.LOW)
                {
                    retval = UsbRelayDeviceHelper.OpenOneRelayChannel(device_handle, 1);
                    switch (retval)
                    {
                        case 0:
                            LogMessage("FlatPanel", makeLogStr("Flat panel Relay #1 opened."));
                            break;
                        case 1:
                            LogMessage("FlatPanel", makeLogStr("Error occured when opening Flat panel Relay #1."));
                            return false;
                        case 2:
                            LogMessage("FlatPanel", makeLogStr("'Index out of range' when opening Flat panel Relay #1."));
                            return false;
                    }

                    retval = UsbRelayDeviceHelper.CloseOneRelayChannel(device_handle, 2);
                    switch (retval)
                    {
                        case 0:
                            LogMessage("FlatPanel", makeLogStr("Flat panel Relay #2 closed."));
                            break;
                        case 1:
                            LogMessage("FlatPanel", makeLogStr("Error occured when closing Flat panel Relay #2."));
                            return false;
                        case 2:
                            LogMessage("FlatPanel", makeLogStr("'Index out of range' when closing Flat panel Relay #2."));
                            return false;
                    }
                }
                return true;
            }
        }
    }
}
