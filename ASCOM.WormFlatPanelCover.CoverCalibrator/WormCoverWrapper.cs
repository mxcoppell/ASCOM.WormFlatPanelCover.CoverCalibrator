using System;
using System.Threading;
using System.Windows.Forms;

namespace ASCOM.WormFlatPanelCover
{
    class WormCoverWrapper : WormSerialPortWrapper
    {
        public WormCoverWrapper(CoverCalibrator drv, bool is_simulation) : base(drv, is_simulation) { }

        private byte checkSum(byte[] data, int bytes)
        {
            int checkSum = 0;
            for (int i = 0; i < bytes; i++)
            {
                checkSum += data[i];
            }
            return BitConverter.GetBytes(checkSum)[0];
        }

        public new bool Connect()
        {
            lock (this)
            {
                if (IsOpen)
                    Close();

                bool retval = false;
                try
                {
                    PortName = Driver.comPort;
                    Open(); Close();
                    retval = true;
                }
                catch (InvalidOperationException) { }
                catch (System.UnauthorizedAccessException) { }
                catch (System.IO.IOException) { }
                if (!retval)
                {
                    LogMessage("WormCover", "ERROR: (Connect) failed to open serial port. ({0})", Driver.comPort);
                    MessageBox.Show("未能连接镜头盖设备【" + Driver.comPort + "】", "虫子电动平场镜头盖",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Setup parameters, initialize COM port
                PortName = Driver.comPort;
                BaudRate = 9600;
                Open();
                ReadTimeout = 1000;

                // Test COM port responses.
                DiscardInBuffer();
                DiscardOutBuffer();

                byte[] tx_buffer = new byte[4] { 0xB0, 0x61, 0x00, 0x00 };
                tx_buffer[3] = checkSum(tx_buffer, 3);
                Write(tx_buffer, 0, 4);
                try
                {
                    byte[] rx_buffer = new byte[4];
                    Read(rx_buffer, 0, 3);
                }
                catch (TimeoutException)
                {
                    LogMessage("WormCover", "ERROR: (Connect) Failed on testing COM port responses. Checkpoint #1. ({0})", Driver.comPort);
                    Close();
                    return false;
                }

                tx_buffer = new byte[10] { 0xB0, 0x62, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x0F, 0x05, 0x00 };
                tx_buffer[9] = checkSum(tx_buffer, 9);
                Write(tx_buffer, 0, 10);
                try
                {
                    byte[] rx_buffer = new byte[4];
                    Read(rx_buffer, 0, 3);
                }
                catch (TimeoutException)
                {
                    LogMessage("WormCover", "ERROR: (Connect) Failed on testing COM port responses. Checkpoint #2. ({0})", Driver.comPort);
                    Close();
                    return false;
                }

                if (IsOpen)
                    LogMessage("WormCover", "INFO: (Connect) Serial port connected. ({0})", Driver.comPort);
                else
                    LogMessage("WormCover", "ERROR: (Connect) Serial port not connected. ({0})", Driver.comPort);

                stopCoverMotorDrive();
                return true;
            }
        }

        public bool stopCoverMotor()
        {
            LogMessage("WormCover", "INFO: (stopCoverMotor) Device({0})", Driver.comPort);

            lock (this)
            {
                DiscardInBuffer();
                DiscardOutBuffer();

                byte[] tx_buffer = new byte[4] { 0xB0, 0x61, 0x00, 0x00 };
                tx_buffer[3] = checkSum(tx_buffer, 3);
                Write(tx_buffer, 0, 4);
                try
                {
                    byte[] rx_buffer = new byte[4];
                    Read(rx_buffer, 0, 3);
                }
                catch (TimeoutException)
                {
                    MessageBox.Show("发送镜头盖移动命令失败【" + Driver.comPort + "】", "虫子镜头盖",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogMessage("WormCover", "ERROR: (stopCoverMotor) Failed to send stop cover motor command. ({0})", Driver.comPort);
                    return false;
                }
                return true;
            }
        }

        public bool stopCoverMotorDrive()
        {
            LogMessage("WormCover", "INFO: (stopCoverMotorDrive) Device({0})", Driver.comPort);

            lock (this)
            {
                DiscardInBuffer();
                DiscardOutBuffer();

                byte[] tx_buffer = new byte[4] { 0xB0, 0x61, 0x03, 0x00 };
                tx_buffer[3] = checkSum(tx_buffer, 3);
                tx_buffer[3] = BitConverter.GetBytes(tx_buffer[0] + tx_buffer[1] + tx_buffer[2])[0];
                Write(tx_buffer, 0, 4);
                try
                {
                    byte[] rx_buffer = new byte[4];
                    Read(rx_buffer, 0, 3);
                }
                catch (TimeoutException)
                {
                    MessageBox.Show("取消镜头盖驱动命令失败【" + Driver.comPort + "】", "虫子镜头盖",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogMessage("WormCover", "ERROR: (stopCoverMotorDrive) Failed to void cover motor drive. ({0})", Driver.comPort);
                    return false;
                }
                return true;
            }
        }

        public void setCoverTravelDistance(int distance, int speed, int accel)
        {
            LogMessage("WormCover", "INFO: (setCoverTravelDistance) Distance({0}), Speed({1}), Acceleration({2}, Device({3})",
                distance, speed, accel, Driver.comPort);

            lock (this)
            {
                //  Set cover travel distance
                DiscardInBuffer();
                DiscardOutBuffer();

                byte[] tx_buffer = new byte[10];
                tx_buffer[0] = 0xB0;
                tx_buffer[1] = 0x62;
                tx_buffer[2] = BitConverter.GetBytes(distance * 8)[3];
                tx_buffer[3] = BitConverter.GetBytes(distance * 8)[2];
                tx_buffer[4] = BitConverter.GetBytes(distance * 8)[1];
                tx_buffer[5] = BitConverter.GetBytes(distance * 8)[0];
                tx_buffer[6] = BitConverter.GetBytes(speed)[1];
                tx_buffer[7] = BitConverter.GetBytes(speed)[0];
                tx_buffer[8] = BitConverter.GetBytes(accel)[0];
                tx_buffer[9] = checkSum(tx_buffer, 9);
                Write(tx_buffer, 0, 10);
                try
                {
                    byte[] rx_buffer = new byte[4];
                    Read(rx_buffer, 0, 3);
                }
                catch (TimeoutException)
                {
                    MessageBox.Show("设置镜头盖移动距离命令失败【" + Driver.comPort + "】", "虫子镜头盖",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogMessage("WormCover", "ERROR: (setCoverTravelDistance) Failed to set cover travel distance. ({0})", Driver.comPort);
                    Close();
                    return;
                }
            }
        }

        public void startCoverMotor()
        {
            lock (this)
            {
                DiscardInBuffer();
                DiscardOutBuffer();

                byte[] tx_buffer = new byte[4] { 0xB0, 0x61, 0x01, 0x00 };
                tx_buffer[3] = checkSum(tx_buffer, 3);
                Write(tx_buffer, 0, 4);
                try
                {
                    byte[] rx_buffer = new byte[4];
                    Read(rx_buffer, 0, 3);
                }
                catch (TimeoutException)
                {
                    MessageBox.Show("启动镜头盖马达命令失败【" + Driver.comPort + "】", "虫子镜头盖",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogMessage("WormCover", "ERROR: (startCoverMotor) Failed to start cover motor. ({0})", Driver.comPort);
                    Close();
                    return;
                }
            }
        }

        public bool openCover()
        {
            if (Driver.currentAngle >= Driver.targetAngle)
            {
                LogMessage("WormCover", "INFO: (openCover) worm cover already opened.");
                return false;
            }

            if (!IsOpen)
            {
                LogMessage("WormCover", "ERROR: (openCover) serial port not opened. ({0})", Driver.comPort);
                MessageBox.Show("镜头盖设备【" + Driver.comPort + "】未连接！", "虫子镜头盖", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            LogMessage("WormCover", "INFO: (openCover) Opening Worm cover...");

            setCoverTravelDistance(Driver.targetAngle - Driver.currentAngle, Driver.coverMovingSpeed, Driver.coverMovingAcceleration);
            startCoverMotor();

            return true;
        }

        public bool closeCover()
        {
            if (Driver.currentAngle == 0)
            {
                LogMessage("WormCover", "INFO: (closeCover) Worm cover already closed.");
                return false;
            }

            if (!IsOpen)
            {
                LogMessage("WormCover", "ERROR: (closeCover) Serial port not opened. ({0})", Driver.comPort);
                MessageBox.Show("镜头盖设备【" + Driver.comPort + "】未连接！", "虫子镜头盖", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            LogMessage("WormCover", "INFO: (closeCover) Closing Worm cover...");

            setCoverTravelDistance(-Driver.currentAngle, Driver.coverMovingSpeed, Driver.coverMovingAcceleration);
            startCoverMotor();

            return true;
        }

        public int getCoverRealtimeAngle()
        {
            byte[] rx_buffer = new byte[32];

            lock (this)
            {
                DiscardInBuffer();
                DiscardOutBuffer();

                // Read step motor control data
                byte[] tx_buffer = new byte[3] { 0xB0, 0x63, 0x00 };
                tx_buffer[2] = checkSum(tx_buffer, 2);
                Write(tx_buffer, 0, 3);
                try
                {
                    System.Threading.Thread.Sleep(200);
                    Read(rx_buffer, 0, 11);
                }
                catch (TimeoutException)
                {
                    LogMessage("WormCover", "ERROR: (getCoverRealtimeAngle) Failed to read cover step motor control data. ({0})", Driver.comPort);
                    Close();
                    return -999;
                }

                byte[] calc_angle = new byte[4];
                calc_angle[0] = rx_buffer[6];
                calc_angle[1] = rx_buffer[5];
                calc_angle[2] = rx_buffer[4];
                calc_angle[3] = rx_buffer[3];

                return System.BitConverter.ToInt32(calc_angle, 0) / 8;
            }
        }

        //  Cover opening monitoring thread routine, could be called in a blocking scenario
        public void waitForCoverOpenCompletion()
        {
            DateTime start_time = DateTime.Now;
            while (true)
            {
                int rt_angle = getCoverRealtimeAngle();
                LogMessage("WormCover", "INFO: (Opening) current position " + rt_angle + " ...");

                Driver.currentAngle = rt_angle;
                if (rt_angle > Driver.targetAngle) rt_angle = Driver.targetAngle;
                Driver.WriteProfile();

                if (rt_angle >= Driver.targetAngle || rt_angle == -999 || DateTime.Now > start_time.AddMinutes(2))
                {
                    Thread.Sleep(3000);
                    stopCoverMotor();
                    stopCoverMotorDrive();
                    break;
                }
            }
        }

        //  Cover closing monitoring thread routine, could be called in a blocking scenario
        public void waitForCoverCloseComletion()
        {
            DateTime start_time = DateTime.Now;
            while (true)
            {
                int rt_angle = Driver.targetAngle + getCoverRealtimeAngle();
                LogMessage("WormCover", "(Closing) current position " + rt_angle + " ...");

                Driver.currentAngle = rt_angle;
                if (rt_angle < 0) rt_angle = 0;
                if (rt_angle > Driver.targetAngle) rt_angle = Driver.targetAngle;
                Driver.WriteProfile();

                if (rt_angle == 0 || rt_angle == -999 || DateTime.Now > start_time.AddMinutes(2))
                {
                    Thread.Sleep(3000);
                    stopCoverMotor();
                    stopCoverMotorDrive();
                    break;
                }
            }
        }
    }
}
