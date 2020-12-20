using System.Text;
using ASCOM.Utilities;

namespace ASCOM.WormFlatPanelCover
{
    public class WormDeviceWrapperCommon
    {
        TraceLogger tl;
        public WormDeviceWrapperCommon(CoverCalibrator drv, bool is_simulation)
        {
            Driver = drv;
            tl = Driver.tl;
            IsSimulation = is_simulation;
        }

        private bool simulation = true;

        public CoverCalibrator Driver { get; private set; }

        //  get/set simulation mode
        //      true - simulation console output
        //      false - SerialPort operation
        public bool IsSimulation
        {
            get { return simulation; }
            set { simulation = value; }
        }

        public bool Connect() { return false;  }
        public bool Disconnect() { return false;  }

        protected string makeByteStr(byte[] buffer, int count)
        {
            StringBuilder sb_bytes = new StringBuilder();
            for (int i = 0; i < count; i++)
                sb_bytes.AppendFormat("{0:X2} ", buffer[i]);
            return sb_bytes.ToString().TrimEnd();
        }
        protected string makeLogStr(string value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Simulation[" + IsSimulation + "] " + value);
            return sb.ToString();
        }
        internal void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage(identifier, msg);
        }
    }
}
