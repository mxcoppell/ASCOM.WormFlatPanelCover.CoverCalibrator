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

        SimMotorResponder sim_motor = new SimMotorResponder();

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
            LogMessage("SerialPort", "Available COM ports: {0}", string.Join(",", PortNames));
        }

        ~WormSerialPortWrapper()
        {
            Disconnect();
        }

        public new bool Disconnect()
        {
            if (serial_port.IsOpen) serial_port.Close();
            LogMessage("Serial Port", "INFO: (Disconnect) Serial port closed. ({0})", Driver.comPort);
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
                LogMessage("SerialPort", "Port name set: {0}", PortName);
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
                LogMessage("SerialPort", "Baudrate set: {0}", BaudRate);
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
                LogMessage("SerialPort", "Readtimeout set: {0}", read_timeout);
            }
        }
        public bool IsOpen
        {
            get
            {
                bool retval = IsSimulation ? sim_opened : serial_port.IsOpen;
                LogMessage("SerialPort", "Serial port open status: {0}", retval);
                return retval;
            }
        }

        public void Open()
        {
            if (IsSimulation)
            {
                if (PortName == "COM4")
                {
                    sim_opened = false;
                    throw new System.IO.IOException();
                }
                else
                    sim_opened = true;
            }
            else
                serial_port.Open();
            LogMessage("SerialPort", "Serial port opened.");
        }
        public void Close()
        {
            if (IsSimulation)
                sim_opened = false;
            else
                serial_port.Close();
            LogMessage("SerialPort", "Serial port closed.");
        }
        public void DiscardInBuffer()
        {
            if (!IsSimulation)
                serial_port.DiscardInBuffer();
            LogMessage("SerialPort", "Serial port input buffer cleared.");
        }
        public void DiscardOutBuffer()
        {
            if (!IsSimulation)
                serial_port.DiscardOutBuffer();
            LogMessage("SerialPort", "Serial port output buffer cleared.");
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (!IsSimulation)
            {
                serial_port.Write(buffer, offset, count);
            }
            else
            {
                byte[] req_data = new byte[count];
                System.Array.Copy(buffer, req_data, count);
                sim_motor.setLastRequest(req_data);
            }
            LogMessage("SerialPort", "TX >>> {0}", makeByteStr(buffer, count));
        }
        public int Read(byte[] buffer, int offset, int count)
        {
            if (IsSimulation)
            {
                byte[] rsp_data = sim_motor.getResponse();
                System.Array.Copy(rsp_data, buffer, rsp_data.Length);
            }
            else
            {
                serial_port.Read(buffer, offset, count);
            }
            LogMessage("SerialPort", "RX <<< {0}", makeByteStr(buffer, count));
            return 0;
        }

    }
}
