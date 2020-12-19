using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace ASCOM.WormFlatPanelCover
{
    class WormSerialPortWrapper : WormDeviceWrapperCommon
    {
        private string[] port_names;
        private string port_name = "COM99";
        private int baud_rate = 9600;
        private int read_timeout = 1000;

        private bool sim_opened = false;

        SerialPort serial_port = new SerialPort();

        public WormSerialPortWrapper(bool is_simulation = true)
        {
            IsSimulation = is_simulation;
            if (IsSimulation)
            {
                PortNames = new string[] { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8" };
            }
            else
            {
                PortNames = SerialPort.GetPortNames();
            }
        }
        ~WormSerialPortWrapper()
        {
            Close();
        }
        public string[] PortNames
        {
            get => port_names;
            set => port_names = value;
        }
        public string PortName
        {
            get { return port_name; }
            set
            {
                port_name = value;
                if (!IsSimulation)
                    serial_port.PortName = port_name;
                WormLogger.Log(makeLogStr("Port name set: " + PortName), true);
            }
        }
        public int BaudRate
        {
            get { return baud_rate; }
            set
            {
                baud_rate = value;
                if (!IsSimulation)
                    serial_port.BaudRate = baud_rate;
                WormLogger.Log(makeLogStr("Baudrate set: " + BaudRate), true);
            }
        }
        public int ReadTimeout
        {
            get { return read_timeout; }
            set
            {
                read_timeout = value;
                if (!IsSimulation)
                    serial_port.ReadTimeout = read_timeout;
                WormLogger.Log(makeLogStr("Readtimeout set: " + read_timeout), true);
            }
        }
        public bool IsOpen
        {
            get
            {
                bool retval = IsSimulation ? sim_opened : serial_port.IsOpen;
                WormLogger.Log(makeLogStr("Serial port open status: " + retval), true);
                return retval;
            }
        }

        public void Open()
        {
            if (IsSimulation)
                sim_opened = true;
            else
                serial_port.Open();
            WormLogger.Log(makeLogStr("Serial port opened."), true);
        }
        public void Close()
        {
            if (IsSimulation)
                sim_opened = false;
            else
                serial_port.Close();
            WormLogger.Log(makeLogStr("Serial port closed."), true);
        }
        public void DiscardInBuffer()
        {
            if (!IsSimulation)
                serial_port.DiscardInBuffer();
            WormLogger.Log(makeLogStr("Serial port input buffer cleared."), true);
        }
        public void DiscardOutBuffer()
        {
            if (!IsSimulation)
                serial_port.DiscardOutBuffer();
            WormLogger.Log(makeLogStr("Serial port output buffer cleared."), true);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (!IsSimulation)
            {
                serial_port.Write(buffer, offset, count);
            }
            WormLogger.Log(makeLogStr("TX >>> " + makeByteStr(buffer, count)), true);
        }
        public int Read(byte[] buffer, int offset, int count)
        {
            if (IsSimulation)
            {
                for (int i = 0x0; i < count; i++)
                    buffer[i] = (byte)(i + 0xA0);
            }
            else
            {
                serial_port.Read(buffer, offset, count);
            }
            WormLogger.Log(makeLogStr("RX <<< " + makeByteStr(buffer, count)), true);
            return 0;
        }

    }
}
