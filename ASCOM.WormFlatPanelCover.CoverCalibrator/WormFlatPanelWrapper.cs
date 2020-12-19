using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ASCOM.WormFlatPanelCover
{
    class WormFlatPanelWrapper : WormDeviceWrapperCommon
    {
        public enum BRIGHTNESS { HIGH, LOW }

        public static int device_handle = 0;
        List<string> flatpanel_serial_numbers = new List<string>();

        private string TargetSerialNumber { get; set; }

        public WormFlatPanelWrapper(bool is_simulation = true)
        {
            IsSimulation = is_simulation;
            TargetSerialNumber = "";
            GetSerialNumbers();
            device_handle = OpenDeviceHandle(TargetSerialNumber);
        }
        /*
        ~FlatPanelWrapper()
        {
            if (device_handle != 0)
                UsbRelayDeviceHelper.Close(device_handle);
        }
        */
        public void GetSerialNumbers()
        {
            flatpanel_serial_numbers.Clear();

            if (IsSimulation)
            {
                flatpanel_serial_numbers.Add("WormFlatPanel_#1");
                WormLogger.Log(makeLogStr("Added usb relay device serial number (WormFlatPanel_#1)."), true);
                flatpanel_serial_numbers.Add("WormFlatPanel_#2");
                WormLogger.Log(makeLogStr("Added usb relay device serial number (WormFlatPanel_#2)."), true);
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
                        WormLogger.Log(makeLogStr("Added usb relay device serial number (" + usb_relay_it.SerialNumber + ")."), true);
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
            WormLogger.Log(makeLogStr("Set default usb relay device serial number (" + TargetSerialNumber + ")."), true);
        }

        public int OpenDeviceHandle(string serialnumber)
        {
            if (serialnumber != "")
            {
                int retval = 1; //  dummy device handle
                if (!IsSimulation)
                    retval = UsbRelayDeviceHelper.OpenWithSerialNumber(serialnumber, serialnumber.Length);
                WormLogger.Log(makeLogStr("Got device handle (" + retval + ") with usb relay device serial number (" + serialnumber + ")."), true);
                return retval;
            }
            WormLogger.Log(makeLogStr("Empty usb relay device serial number."), true);
            return 0;
        }
        public bool TurnOff()
        {
            if (!IsSimulation)
            {
                if (device_handle == 0)
                {
                    WormLogger.Log(makeLogStr("Invalid flat panel device handle."), true);
                    return false;
                }

                UsbRelayDeviceHelper.CloseOneRelayChannel(device_handle, 1);
                WormLogger.Log(makeLogStr("Flat panel Relay #1 closed."), true);
                UsbRelayDeviceHelper.CloseOneRelayChannel(device_handle, 2);
                WormLogger.Log(makeLogStr("Flat panel Relay #2 closed."), true);
            }
            else
            {
                WormLogger.Log(makeLogStr("Flat panel Relay #1 closed."), true);
                WormLogger.Log(makeLogStr("Flat panel Relay #2 closed."), true);
            }
            WormLogger.Log(makeLogStr("Flat panel turned off."), true);
            return true;
        }

        public bool TurnOn(BRIGHTNESS brightness)
        {
            WormLogger.Log(makeLogStr("Turning on flat panel in mode: " + brightness), true);
            if (IsSimulation)
            {
                if (brightness == BRIGHTNESS.HIGH)
                {
                    WormLogger.Log(makeLogStr("Flat panel Relay #1 closed."), true);
                    WormLogger.Log(makeLogStr("Flat panel Relay #2 opened."), true);
                }
                else if (brightness == BRIGHTNESS.LOW)
                {
                    WormLogger.Log(makeLogStr("Flat panel Relay #1 opened."), true);
                    WormLogger.Log(makeLogStr("Flat panel Relay #2 closed."), true);
                }
                return true;
            }
            else
            {
                if (device_handle == 0)
                {
                    WormLogger.Log(makeLogStr("Invalid flat panel device handle."), true);
                    return false;
                }

                int retval = 0;
                if (brightness == BRIGHTNESS.HIGH)
                {
                    retval = UsbRelayDeviceHelper.CloseOneRelayChannel(device_handle, 1);
                    switch (retval)
                    {
                        case 0:
                            WormLogger.Log(makeLogStr("Flat panel Relay #1 closed."), true);
                            break;
                        case 1:
                            WormLogger.Log(makeLogStr("Error occured when closing Flat panel Relay #1."), true);
                            return false;
                        case 2:
                            WormLogger.Log(makeLogStr("'Index out of range' when closing Flat panel Relay #1."), true);
                            return false;
                    }

                    retval = UsbRelayDeviceHelper.OpenOneRelayChannel(device_handle, 2);
                    switch (retval)
                    {
                        case 0:
                            WormLogger.Log(makeLogStr("Flat panel Relay #2 opened."), true);
                            break;
                        case 1:
                            WormLogger.Log(makeLogStr("Error occured when opening Flat panel Relay #2."), true);
                            return false;
                        case 2:
                            WormLogger.Log(makeLogStr("'Index out of range' when opening Flat panel Relay #2."), true);
                            return false;
                    }

                }
                else if (brightness == BRIGHTNESS.LOW)
                {
                    retval = UsbRelayDeviceHelper.OpenOneRelayChannel(device_handle, 1);
                    switch (retval)
                    {
                        case 0:
                            WormLogger.Log(makeLogStr("Flat panel Relay #1 opened."), true);
                            break;
                        case 1:
                            WormLogger.Log(makeLogStr("Error occured when opening Flat panel Relay #1."), true);
                            return false;
                        case 2:
                            WormLogger.Log(makeLogStr("'Index out of range' when opening Flat panel Relay #1."), true);
                            return false;
                    }

                    retval = UsbRelayDeviceHelper.CloseOneRelayChannel(device_handle, 2);
                    switch (retval)
                    {
                        case 0:
                            WormLogger.Log(makeLogStr("Flat panel Relay #2 closed."), true);
                            break;
                        case 1:
                            WormLogger.Log(makeLogStr("Error occured when closing Flat panel Relay #2."), true);
                            return false;
                        case 2:
                            WormLogger.Log(makeLogStr("'Index out of range' when closing Flat panel Relay #2."), true);
                            return false;
                    }
                }
                return true;
            }

        }

    }
}
