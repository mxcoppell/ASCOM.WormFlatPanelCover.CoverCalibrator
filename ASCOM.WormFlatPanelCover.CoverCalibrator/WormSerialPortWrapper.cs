using System.IO.Ports;
using ASCOM.Utilities;

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

        public WormSerialPortWrapper(CoverCalibrator drv, bool is_simulation) : base(drv, is_simulation)
        {
            if (IsSimulation)
            {
                LogMessage("SerialPort", "Simulation mode is ON.");
                PortNames = new string[] { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8" };
            }
            else
            {
                PortNames = SerialPort.GetPortNames();
            }
        }

        ~WormSerialPortWrapper()
        {
            Disconnect();
        }

        new bool Connect()
        {
            return false;
        }
        new bool Disconnect()
        {
            if (serial_port.IsOpen) serial_port.Close();
            return true;
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
                LogMessage("SerialPort", makeLogStr("Port name set: " + PortName));
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
                LogMessage("SerialPort", makeLogStr("Baudrate set: " + BaudRate));
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
                LogMessage("SerialPort", makeLogStr("Readtimeout set: " + read_timeout));
            }
        }
        public bool IsOpen
        {
            get
            {
                bool retval = IsSimulation ? sim_opened : serial_port.IsOpen;
                LogMessage("SerialPort", makeLogStr("Serial port open status: " + retval));
                return retval;
            }
        }

        public void Open()
        {
            if (IsSimulation)
                sim_opened = true;
            else
                serial_port.Open();
            LogMessage("SerialPort", makeLogStr("Serial port opened."));
        }
        public void Close()
        {
            if (IsSimulation)
                sim_opened = false;
            else
                serial_port.Close();
            LogMessage("SerialPort", makeLogStr("Serial port closed."));
        }
        public void DiscardInBuffer()
        {
            if (!IsSimulation)
                serial_port.DiscardInBuffer();
            LogMessage("SerialPort", makeLogStr("Serial port input buffer cleared."));
        }
        public void DiscardOutBuffer()
        {
            if (!IsSimulation)
                serial_port.DiscardOutBuffer();
            LogMessage("SerialPort", makeLogStr("Serial port output buffer cleared."));
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (!IsSimulation)
            {
                serial_port.Write(buffer, offset, count);
            }
            LogMessage("SerialPort", makeLogStr("TX >>> " + makeByteStr(buffer, count)));
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
            LogMessage("SerialPort", makeLogStr("RX <<< " + makeByteStr(buffer, count)));
            return 0;
        }

    }
}
