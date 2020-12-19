using System.Text;

namespace ASCOM.WormFlatPanelCover
{
    class WormDeviceWrapperCommon
    {
        private bool simulation = true;

        //  get/set simulation mode
        //      true - simulation console output
        //      false - SerialPort operation
        public bool IsSimulation
        {
            get { return simulation; }
            set
            {
                simulation = value;
                WormLogger.Log(makeLogStr("Simulation mode set."), true);
            }
        }

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
            sb.Append("SimMode[" + IsSimulation + "] " + value);
            return sb.ToString();
        }
    }
}
