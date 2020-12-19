using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace ASCOM.WormFlatPanelCover
{
    class WormUtility
    {
        Thread thd_opencover = null;
        Thread thd_closecover = null;

        /* !!! */
        static bool is_simulation = false;

        WormSerialPortWrapper serial_port = new WormSerialPortWrapper(is_simulation);
        WormFlatPanelWrapper flatpanel = new WormFlatPanelWrapper(is_simulation);

        public Configuration appConf = null;

        string com_port;
        public static int open_angle;
        public static int target_angle;
        public static int cover_speed = 200;
        public static int cover_acceleration = 10;

        private bool operation_done = false;
        //ExeConfigurationFileMap map = new ExeConfigurationFileMap { ExeConfigFilename = "./WormCover.exe.Config" };

        public WormUtility()
        {
            string v1 = ConfigurationManager.AppSettings["Version1"].ToString(); ;
            int ver1;
            ver1 = Convert.ToInt32(v1);

            string openangle = ConfigurationManager.AppSettings["angle"].ToString();
            open_angle = Convert.ToInt32(openangle);

            string targetangle = ConfigurationManager.AppSettings["targetangle"].ToString();
            target_angle = Convert.ToInt32(targetangle);

            com_port = ConfigurationManager.AppSettings["COMport"].ToString();

            show_configuration();

            //  initialize serial port
            bool serial_opened = open_serial_port(com_port);
        }

        public void show_configuration()
        {
            WormLogger.Log("WormCover Configuration/Status");
            WormLogger.Log("------------------------------");
            WormLogger.Log("     COM Port: " + com_port);
            WormLogger.Log("Current Angle: " + open_angle);
            WormLogger.Log(" Target Angle: " + target_angle);
            WormLogger.Log("        Speed: " + cover_speed);
            WormLogger.Log(" Acceleration: " + cover_acceleration);
            WormLogger.Log("---");
        }

        public bool open_serial_port(string serial_port)
        {
            if (!this.serial_port.IsOpen)
            {
                string chuankou = serial_port;

                bool serialok = false;
                try
                {
                    this.serial_port.PortName = chuankou;
                    this.serial_port.Open();
                    this.serial_port.Close();
                    serialok = true;

                }
                catch (InvalidOperationException)
                {
                }
                catch (System.UnauthorizedAccessException)
                {
                }
                catch (System.IO.IOException)
                {
                }
                if (!serialok)
                {
                    WormLogger.Log("ERROR: (open_serial_port) failed to open serial port.");
                    return false;
                }


                this.serial_port.PortName = chuankou;
                this.serial_port.BaudRate = 9600;
                this.serial_port.Open();
                this.serial_port.ReadTimeout = 1000;

                //测试是否有返回值
                this.serial_port.DiscardInBuffer();
                this.serial_port.DiscardOutBuffer();
                byte[] kaishi = new byte[20];
                byte[] jieshou = new byte[20];
                kaishi[0] = 0xB0;
                kaishi[1] = 0x61;
                kaishi[2] = 0x00;
                kaishi[3] = BitConverter.GetBytes(kaishi[0] + kaishi[1] + kaishi[2])[0];

                this.serial_port.Write(kaishi, 0, 4);
                try
                {
                    this.serial_port.Read(jieshou, 0, 3);
                }
                catch (TimeoutException)
                {
                    WormLogger.Log("ERROR: (open_serial_port) checkpoint #1, Incorrect serial port.");
                    this.serial_port.Close();
                    return false;
                }
                kaishi[0] = 0xB0;
                kaishi[1] = 0x62;
                kaishi[2] = 0x00;
                kaishi[3] = 0x00;
                kaishi[4] = 0x00;
                kaishi[5] = 0xFF;
                kaishi[6] = 0x00;
                kaishi[7] = 0x0F;
                kaishi[8] = 0x05;
                kaishi[9] = BitConverter.GetBytes(kaishi[0] + kaishi[1] + kaishi[2] + kaishi[3] + kaishi[4] + kaishi[5] + kaishi[6] + kaishi[7] + kaishi[8])[0];
                this.serial_port.Write(kaishi, 0, 10);
                try
                {
                    this.serial_port.Read(jieshou, 0, 3);
                }
                catch (TimeoutException)
                {
                    WormLogger.Log("ERROR: (open_serial_port) checkpoint #2, Incorrect serial port.");
                    this.serial_port.Close();
                    return false;
                }

                WormLogger.Log("INFO: (open_serial_port) serial port connected.");

                //release motor
                if (!this.serial_port.IsOpen)
                {
                    WormLogger.Log("ERROR: (open_serial_port) checkpoint #3, serial port not opened.");
                    return false;
                }

                this.serial_port.DiscardInBuffer();
                this.serial_port.DiscardOutBuffer();
                kaishi[0] = 0xB0;
                kaishi[1] = 0x61;
                kaishi[2] = 0x03;//00停止。01启动。02急停。03无驱动
                kaishi[3] = BitConverter.GetBytes(kaishi[0] + kaishi[1] + kaishi[2])[0];
                this.serial_port.Write(kaishi, 0, 4);
                try
                {
                    this.serial_port.Read(jieshou, 0, 3);
                }
                catch (TimeoutException)
                {
                    WormLogger.Log("ERROR: (open_serial_port) checkpoint #4, incorrect serial port.");
                    this.serial_port.Close();
                    return false;
                }
            }
            else
            {
                this.serial_port.Close();
            }
            return true;
        }

        //private void button6_Click(object sender, EventArgs e)

        public void open_cover()
        {
            if (open_angle >= target_angle)
            {
                WormLogger.Log("INFO: (open_cover) worm cover already opened.");
                return;
            }

            if (!serial_port.IsOpen)
            {
                WormLogger.Log("ERROR: (open_cover) checkpoint #1, serial port not opened.");
                return;
            }

            WormLogger.Log("INFO: (open_cover) open worm cover...");

            //设置距离
            serial_port.DiscardInBuffer();
            serial_port.DiscardOutBuffer();
            byte[] kaishi = new byte[20];
            byte[] jieshou = new byte[20];

            kaishi[0] = 0xB0;
            kaishi[1] = 0x62;
            kaishi[2] = BitConverter.GetBytes(target_angle * 8)[3];
            kaishi[3] = BitConverter.GetBytes(target_angle * 8)[2];
            kaishi[4] = BitConverter.GetBytes(target_angle * 8)[1];
            kaishi[5] = BitConverter.GetBytes(target_angle * 8)[0];
            kaishi[6] = BitConverter.GetBytes(cover_speed)[1];
            kaishi[7] = BitConverter.GetBytes(cover_speed)[0];
            kaishi[8] = BitConverter.GetBytes(cover_acceleration)[0];
            kaishi[9] = BitConverter.GetBytes(kaishi[0] + kaishi[1] + kaishi[2] + kaishi[3] + kaishi[4] + kaishi[5] + kaishi[6] + kaishi[7] + kaishi[8])[0];
            serial_port.Write(kaishi, 0, 10);
            try
            {
                serial_port.Read(jieshou, 0, 3);
            }
            catch (TimeoutException)
            {
                WormLogger.Log("ERROR: (open_cover) checkpoint #2, invalid serial port.");
                serial_port.Close();
                return;
            }

            ///开始运动
            serial_port.DiscardInBuffer();
            serial_port.DiscardOutBuffer();

            kaishi[0] = 0xB0;
            kaishi[1] = 0x61;
            kaishi[2] = 0x01;//00停止。01启动。02急停。03无驱动
            kaishi[3] = BitConverter.GetBytes(kaishi[0] + kaishi[1] + kaishi[2])[0];
            serial_port.Write(kaishi, 0, 4);
            try
            {
                serial_port.Read(jieshou, 0, 3);
            }
            catch (TimeoutException)
            {
                WormLogger.Log("ERROR: (open_cover) checkpoint #3, invalid serial port.");
                serial_port.Close();
                return;
            }

            thd_opencover = new Thread(OpenRead);
            thd_opencover.IsBackground = true;
            operation_done = false;
            thd_opencover.Start();

            while (operation_done == false)
                System.Threading.Thread.Sleep(1000);

            /*
            Configuration appConf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            appConf.AppSettings.Settings["angle"].Value = get_real_time_angle().ToString();
            appConf.AppSettings.Settings["targetangle"].Value = target_angle.ToString();
            appConf.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            */
        }

        public void close_cover()
        {
            if (open_angle == 0)
            {
                WormLogger.Log("INFO: (close_cover) worm cover already closed.");
                return;
            }

            if (!serial_port.IsOpen)
            {
                WormLogger.Log("ERROR: (close_cove) checkpoint #1, serial port not opened.");
                return;
            }

            WormLogger.Log("INFO: (close_cove) close worm cover...");

            //设置距离
            serial_port.DiscardInBuffer();
            serial_port.DiscardOutBuffer();
            byte[] kaishi = new byte[20];
            byte[] jieshou = new byte[20];
            kaishi[0] = 0xB0;
            kaishi[1] = 0x62;
            kaishi[2] = BitConverter.GetBytes(-open_angle * 8)[3];
            kaishi[3] = BitConverter.GetBytes(-open_angle * 8)[2];
            kaishi[4] = BitConverter.GetBytes(-open_angle * 8)[1];
            kaishi[5] = BitConverter.GetBytes(-open_angle * 8)[0];
            kaishi[6] = BitConverter.GetBytes(cover_speed)[1];
            kaishi[7] = BitConverter.GetBytes(cover_speed)[0];
            kaishi[8] = BitConverter.GetBytes(cover_acceleration)[0];

            kaishi[9] = BitConverter.GetBytes(kaishi[0] + kaishi[1] + kaishi[2] + kaishi[3] + kaishi[4] + kaishi[5] + kaishi[6] + kaishi[7] + kaishi[8])[0];
            serial_port.Write(kaishi, 0, 10);
            try
            {
                serial_port.Read(jieshou, 0, 3);
            }
            catch (TimeoutException)
            {
                WormLogger.Log("ERROR: (close_cove) checkpoint #4, invalid serial port.");
                serial_port.Close();
                return;
            }

            ///开始运动
            serial_port.DiscardInBuffer();
            serial_port.DiscardOutBuffer();

            kaishi[0] = 0xB0;
            kaishi[1] = 0x61;
            kaishi[2] = 0x01;//00停止。01启动。02急停。03无驱动
            kaishi[3] = BitConverter.GetBytes(kaishi[0] + kaishi[1] + kaishi[2])[0];
            serial_port.Write(kaishi, 0, 4);
            try
            {
                serial_port.Read(jieshou, 0, 3);
            }
            catch (TimeoutException)
            {
                WormLogger.Log("ERROR: (close_cove) checkpoint #5, invalid serial port.");
                serial_port.Close();
                return;
            }

            /*
            Configuration appConf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            appConf.AppSettings.Settings["angle"].Value = "0";
            appConf.AppSettings.Settings["targetangle"].Value = target_angle.ToString();
            appConf.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            */

            thd_closecover = new Thread(CloseRead);
            thd_closecover.IsBackground = true;
            operation_done = false;
            thd_closecover.Start();

            while (operation_done == false)
                System.Threading.Thread.Sleep(1000);
        }

        //
        //  
        //
        private int get_real_time_angle()
        {
            //读出步进电机控制数据
            serial_port.DiscardInBuffer();
            serial_port.DiscardOutBuffer();
            byte[] kaishi = new byte[20];
            byte[] jieshou = new byte[20];
            kaishi[0] = 0xB0;
            kaishi[1] = 0x63;
            kaishi[2] = BitConverter.GetBytes(kaishi[0] + kaishi[1])[0];
            serial_port.Write(kaishi, 0, 3);
            try
            {
                System.Threading.Thread.Sleep(200);
                serial_port.Read(jieshou, 0, 11);
            }
            catch (TimeoutException)
            {
                WormLogger.Log("ERROR: (get_real_time_angle) checkpoint #1, invalid serial port.");
                serial_port.Close();
                return -999;
            }

            byte[] mc = new byte[4];
            mc[0] = jieshou[6];
            mc[1] = jieshou[5];
            mc[2] = jieshou[4];
            mc[3] = jieshou[3];
            int iMc = System.BitConverter.ToInt32(mc, 0) / 8;

            return iMc;
        }

        //
        //  Cover open monitoring thread routine
        //
        private void OpenRead()
        {
            byte[] kaishi = new byte[32];
            byte[] jieshou = new byte[32];

            DateTime newTime = DateTime.Now;
            DateTime oldTime = DateTime.Now;
            while (true)
            {
                newTime = DateTime.Now;
                int jd = 0;
                jd = get_real_time_angle();
                WormLogger.Log("INFO: (OpenRead) current position " + jd + " ...");

                Configuration appConf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                appConf.AppSettings.Settings["angle"].Value = jd.ToString();
                appConf.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                if (jd >= target_angle || jd == -999 || newTime > oldTime.AddMinutes(2))
                {
                    Thread.Sleep(4000);

                    //
                    //  Stop motor
                    //
                    serial_port.DiscardInBuffer();
                    serial_port.DiscardOutBuffer();

                    kaishi[0] = 0xB0;
                    kaishi[1] = 0x61;
                    kaishi[2] = 0x00;//00停止。01启动。02急停。03无驱动
                    kaishi[3] = BitConverter.GetBytes(kaishi[0] + kaishi[1] + kaishi[2])[0];
                    serial_port.Write(kaishi, 0, 4);
                    try
                    {
                        serial_port.Read(jieshou, 0, 3);
                    }
                    catch (TimeoutException)
                    {
                        WormLogger.Log("ERROR: (OpenRead) checkpoint #1, invalid serial port.");
                        serial_port.Close();
                    }

                    //
                    //  Stop motor drive
                    //
                    serial_port.DiscardInBuffer();
                    serial_port.DiscardOutBuffer();
                    kaishi[0] = 0xB0;
                    kaishi[1] = 0x61;
                    kaishi[2] = 0x03;//00停止。01启动。02急停。03无驱动
                    kaishi[3] = BitConverter.GetBytes(kaishi[0] + kaishi[1] + kaishi[2])[0];
                    serial_port.Write(kaishi, 0, 4);
                    try
                    {
                        serial_port.Read(jieshou, 0, 3);
                    }
                    catch (TimeoutException)
                    {
                        WormLogger.Log("ERROR: (OpenRead) checkpoint #2, invalid serial port.");
                        serial_port.Close();
                    }
                    break;
                }
            }

            operation_done = true;
        }

        private void CloseRead()
        {
            byte[] kaishi = new byte[32];
            byte[] jieshou = new byte[32];

            DateTime newTime = DateTime.Now;
            DateTime oldTime = DateTime.Now;
            while (true)
            {
                newTime = DateTime.Now;
                int jd_old = target_angle;
                int jd = 0;
                int jd_new = get_real_time_angle();
                jd = jd_old + jd_new;
                WormLogger.Log("INFO: (CloseRead) current position " + jd + " ...");

                Configuration appConf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                appConf.AppSettings.Settings["angle"].Value = jd.ToString();
                appConf.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                if (jd == 0 || jd == -999 || newTime > oldTime.AddMinutes(2))
                {
                    Thread.Sleep(4000);

                    //
                    //  Stop motor
                    //
                    serial_port.DiscardInBuffer();
                    serial_port.DiscardOutBuffer();

                    kaishi[0] = 0xB0;
                    kaishi[1] = 0x61;
                    kaishi[2] = 0x00;//00停止。01启动。02急停。03无驱动
                    kaishi[3] = BitConverter.GetBytes(kaishi[0] + kaishi[1] + kaishi[2])[0];
                    serial_port.Write(kaishi, 0, 4);
                    try
                    {
                        serial_port.Read(jieshou, 0, 3);
                    }
                    catch (TimeoutException)
                    {
                        WormLogger.Log("ERROR: (CloseRead) checkpoint #1, invalid serial port.");
                        serial_port.Close();
                    }

                    //
                    //  Stop motor drive
                    //
                    serial_port.DiscardInBuffer();
                    serial_port.DiscardOutBuffer();
                    kaishi[0] = 0xB0;
                    kaishi[1] = 0x61;
                    kaishi[2] = 0x03;//00停止。01启动。02急停。03无驱动
                    kaishi[3] = BitConverter.GetBytes(kaishi[0] + kaishi[1] + kaishi[2])[0];
                    serial_port.Write(kaishi, 0, 4);
                    try
                    {
                        serial_port.Read(jieshou, 0, 3);
                    }
                    catch (TimeoutException)
                    {
                        WormLogger.Log("ERROR: (CloseRead) checkpoint #2, invalid serial port.");
                        serial_port.Close();
                    }
                    break;
                }
            }

            //
            //  Stop motor
            //
            serial_port.DiscardInBuffer();
            serial_port.DiscardOutBuffer();

            kaishi[0] = 0xB0;
            kaishi[1] = 0x61;
            kaishi[2] = 0x00;//00停止。01启动。02急停。03无驱动
            kaishi[3] = BitConverter.GetBytes(kaishi[0] + kaishi[1] + kaishi[2])[0];
            serial_port.Write(kaishi, 0, 4);
            try
            {
                serial_port.Read(jieshou, 0, 3);
            }
            catch (TimeoutException)
            {
                WormLogger.Log("ERROR: (CloseRead) checkpoint #2, invalid serial port.");
                serial_port.Close();
            }
            //
            //  Stop motor drive
            //
            serial_port.DiscardInBuffer();
            serial_port.DiscardOutBuffer();

            kaishi[0] = 0xB0;
            kaishi[1] = 0x61;
            kaishi[2] = 0x03;//00停止。01启动。02急停。03无驱动
            kaishi[3] = BitConverter.GetBytes(kaishi[0] + kaishi[1] + kaishi[2])[0];
            serial_port.Write(kaishi, 0, 4);
            try
            {
                serial_port.Read(jieshou, 0, 3);
            }
            catch (TimeoutException)
            {
                WormLogger.Log("ERROR: (CloseRead) checkpoint #3, invalid serial port.");
                serial_port.Close();
            }

            operation_done = true;
        }

        public void turnoff_flatpanel()
        {
            WormLogger.Log("INFO: (turnoff_flatpanel) Turning off flat panel.");
            flatpanel.TurnOff();
        }
        public void turnon_flatpanel_high()
        {
            WormLogger.Log("INFO: (turnoff_flatpanel) Turning on flat panel with high brightness.");
            flatpanel.TurnOn(WormFlatPanelWrapper.BRIGHTNESS.HIGH);
        }
        public void turnon_flatpanel_low()
        {
            WormLogger.Log("INFO: (turnoff_flatpanel) Turning on flat panel with low brightness.");
            flatpanel.TurnOn(WormFlatPanelWrapper.BRIGHTNESS.LOW);
        }

    }
}
