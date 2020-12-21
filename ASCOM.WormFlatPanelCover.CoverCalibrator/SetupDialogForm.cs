using ASCOM.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

namespace ASCOM.WormFlatPanelCover
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        CoverCalibrator driver; // driver reference
        TraceLogger tl; // Holder for a reference to the driver's trace logger

        public SetupDialogForm(CoverCalibrator drvr)
        {
            InitializeComponent();

            driver = drvr;
            tl = driver.tl;

            // Initialise current values of user settings from the ASCOM Profile
            InitUI();
        }

        private void cmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            // Place any validation constraint checks here
            // Update the state variables with results from the dialogue
            driver.comPort = (string)comboBox_ComPort.SelectedItem;
            driver.currentAngle = Int32.Parse(label_CurrentAngleValue.Text);
            driver.targetAngle = Int32.Parse(textBox_TargetAngle.Text);
            driver.coverMovingSpeed = Int32.Parse(textBox_CoverMoveSpeed.Text);
            driver.coverMovingAcceleration = Int32.Parse(textBox_CoverAcceleration.Text);
            driver.lastUsedFlatPanelSerial = (string)comboBox_FlatPanels.SelectedItem;
            if (driver.lastUsedFlatPanelSerial == null)
                driver.lastUsedFlatPanelSerial = "";
            tl.Enabled = chkTrace.Checked;
            driver.simulationState = chkSimulation.Checked;
        }

        private void cmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
            Close();
        }

        private void BrowseToAscom(object sender, EventArgs e) // Click on ASCOM logo event handler
        {
            try
            {
                System.Diagnostics.Process.Start("https://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void InitUI()
        {
            chkTrace.Checked = tl.Enabled;
            chkSimulation.Checked = driver.simulationState;

            // set the list of com ports to those that are currently available
            comboBox_ComPort.Items.Clear();
            comboBox_ComPort.Items.AddRange(driver.cover.PortNames);
            // select the current port if possible
            if (comboBox_ComPort.Items.Contains(driver.comPort))
            {
                comboBox_ComPort.SelectedItem = driver.comPort;
            }

            // set the list of flat panel serial numbers that are available
            comboBox_FlatPanels.Items.Clear();
            comboBox_FlatPanels.Items.AddRange(driver.flat_panel.SerialNumbers);
            // select the current port if possible
            if (comboBox_FlatPanels.Items.Contains(driver.lastUsedFlatPanelSerial))
            {
                comboBox_FlatPanels.SelectedItem = driver.lastUsedFlatPanelSerial;
            }

            // set current cover angle
            label_CurrentAngleValue.Text = driver.currentAngle.ToString();
            // set target cover angle
            textBox_TargetAngle.Text = driver.targetAngle.ToString();
            // set cover moving speed
            textBox_CoverMoveSpeed.Text = driver.coverMovingSpeed.ToString();
            // set cover moving acceleration
            textBox_CoverAcceleration.Text = driver.coverMovingAcceleration.ToString();
            // set cover opening progress bar
            progressBar_Cover.Maximum = driver.targetAngle;
            progressBar_Cover.Value = driver.currentAngle;

            // check cover data and set command button status
            thread_UpdateCoverCommands(false);
            // check flat panel data and set command button status
            thread_UpdateFlatPanelCommands(false);
        }

        private void chkTrace_CheckedChanged(object sender, EventArgs e)
        {
            driver.tl.Enabled = chkTrace.Checked;
            driver.WriteProfile();
        }

        private void textBox_CoverAcceleration_TextChanged(object sender, EventArgs e)
        {
            driver.coverMovingAcceleration = Int32.Parse(textBox_CoverAcceleration.Text);
            driver.WriteProfile();
        }

        private void textBox_TargetAngle_TextChanged(object sender, EventArgs e)
        {
            driver.targetAngle = Int32.Parse(textBox_TargetAngle.Text);
            driver.WriteProfile();
            thread_UpdateCoverCommands(false);
        }

        private void comboBox_ComPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            driver.cover.Disconnect();
            driver.comPort = (string)comboBox_ComPort.SelectedItem;
            driver.WriteProfile();
            driver.cover.Connect();
            thread_UpdateCoverCommands(false);
        }

        private void button_FlatPanelHigh_Click(object sender, EventArgs e)
        {
            driver.flat_panel.TurnOn(WormFlatPanelWrapper.BRIGHTNESS.HIGH);

            button_FlatPanelHigh.Enabled = false;
            button_FlatPanelHigh.BackColor = System.Drawing.Color.Chartreuse;
            button_FlatPanelHigh.ForeColor = System.Drawing.SystemColors.ControlDarkDark;

            button_FlatPanelOff.Enabled = true;
            button_FlatPanelOff.ForeColor = System.Drawing.SystemColors.ControlLightLight;

            button_FlatPanelLow.Enabled = true;
            button_FlatPanelLow.BackColor = System.Drawing.SystemColors.ControlDark;
            button_FlatPanelLow.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
        }

        private void button_FlatPanelOff_Click(object sender, EventArgs e)
        {
            driver.flat_panel.TurnOff();

            button_FlatPanelOff.Enabled = false;
            button_FlatPanelOff.ForeColor = System.Drawing.SystemColors.ControlDark;

            button_FlatPanelHigh.Enabled = true;
            button_FlatPanelHigh.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            button_FlatPanelHigh.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;

            button_FlatPanelLow.Enabled = true;
            button_FlatPanelLow.BackColor = System.Drawing.SystemColors.ControlDark;
            button_FlatPanelLow.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
        }

        private void button_FlatPanelLow_Click(object sender, EventArgs e)
        {
            button_FlatPanelLow.Enabled = false;
            button_FlatPanelLow.BackColor = System.Drawing.Color.ForestGreen;
            button_FlatPanelLow.ForeColor = System.Drawing.SystemColors.ControlDarkDark;

            button_FlatPanelOff.Enabled = true;
            button_FlatPanelOff.ForeColor = System.Drawing.SystemColors.ControlLightLight;

            button_FlatPanelHigh.Enabled = true;
            button_FlatPanelHigh.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            button_FlatPanelHigh.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
        }

        private void chkSimulation_CheckedChanged(object sender, EventArgs e)
        {
            driver.simulationState = chkSimulation.Checked;
            driver.WriteProfile();
        }

        private void comboBox_FlatPanels_SelectedIndexChanged(object sender, EventArgs e)
        {
            driver.flat_panel.Disconnect();
            driver.lastUsedFlatPanelSerial = (string)comboBox_FlatPanels.SelectedItem;
            driver.WriteProfile();
            driver.flat_panel.Connect();
            thread_UpdateFlatPanelCommands(false);
        }

        private void textBox_CoverMoveSpeed_TextChanged(object sender, EventArgs e)
        {
            driver.coverMovingSpeed = Int32.Parse(textBox_CoverMoveSpeed.Text);
            driver.WriteProfile();
        }
        private void threadWaitForCoverOpenCompletion()
        {
            DateTime start_time = DateTime.Now;
            while (true)
            {
                int rt_angle = driver.cover.getCoverRealtimeAngle();
                driver.LogMessage("SetupDialog", "INFO: (Opening) current position " + rt_angle + " ...");

                if (rt_angle > driver.targetAngle) rt_angle = driver.targetAngle;
                driver.currentAngle = rt_angle;
                driver.WriteProfile();
                thread_UpdateCoverProgress(true);

                if (rt_angle >= driver.targetAngle || rt_angle == -999 || DateTime.Now > start_time.AddMinutes(2))
                {
                    Thread.Sleep(3000);
                    driver.cover.stopCoverMotor();
                    driver.cover.voidCoverMotherDriver();
                    break;
                }
            }
            thread_SetCoverUIState(true, true);
            thread_UpdateCoverCommands(true);
        }

        private void button_OpenCover_Click(object sender, EventArgs e)
        {
            driver.LogMessage("SetupDialog", "User clicked Open Cover, curr({0}), target({1}).", driver.currentAngle, driver.targetAngle);

            progressBar_Cover.Maximum = driver.targetAngle;
            progressBar_Cover.Value = driver.currentAngle;

            if (driver.cover.openCover())
            {
                thread_SetCoverUIState(false, false);
                Thread thread_opencover = new Thread(threadWaitForCoverOpenCompletion);
                thread_opencover.IsBackground = true;
                thread_opencover.Start();
            }
        }

        private void threadWaitForCoverCloseCompletion()
        {
            DateTime start_time = DateTime.Now;
            while (true)
            {
                int rt_angle = driver.targetAngle + driver.cover.getCoverRealtimeAngle();
                driver.LogMessage("SetupDialog", "(Closing) current position " + rt_angle + " ...");

                if (rt_angle < 0) rt_angle = 0;
                if (rt_angle > driver.targetAngle) rt_angle = driver.targetAngle;
                driver.currentAngle = rt_angle;
                driver.WriteProfile();
                thread_UpdateCoverProgress(true);

                if (rt_angle == 0 || rt_angle == -999 || DateTime.Now > start_time.AddMinutes(2))
                {
                    Thread.Sleep(3000);
                    driver.cover.stopCoverMotor();
                    driver.cover.voidCoverMotherDriver();
                    break;
                }
            }
            thread_SetCoverUIState(true, true);
            thread_UpdateCoverCommands(true);
        }

        private void button_CloseCover_Click(object sender, EventArgs e)
        {
            driver.LogMessage("SetupDialog", "User clicked Close Cover, curr({0}), target({1}).", driver.currentAngle, driver.targetAngle);

            progressBar_Cover.Maximum = driver.targetAngle;
            progressBar_Cover.Value = driver.currentAngle;

            if (driver.cover.closeCover())
            {
                thread_SetCoverUIState(false, false);
                Thread thread_closecover = new Thread(threadWaitForCoverCloseCompletion);
                thread_closecover.IsBackground = true;
                thread_closecover.Start();
            }

        }

        private void thread_UpdateCoverProgress(bool is_otherthread)
        {
            if (is_otherthread)
            {
                MethodInvoker m;
                m = new MethodInvoker(() => progressBar_Cover.Maximum = driver.targetAngle); progressBar_Cover.Invoke(m);
                m = new MethodInvoker(() => progressBar_Cover.Value = driver.currentAngle); progressBar_Cover.Invoke(m);
                m = new MethodInvoker(() => label_CurrentAngleValue.Text = driver.currentAngle.ToString()); label_CurrentAngleValue.Invoke(m);
            }
            else
            {
                progressBar_Cover.Maximum = driver.targetAngle;
                progressBar_Cover.Value = driver.currentAngle;
                label_CurrentAngleValue.Text = driver.currentAngle.ToString();

            }
        }

        private void thread_SetCoverUIState(bool is_otherthread, bool state)
        {
            if (is_otherthread)
            {
                MethodInvoker m;
                m = new MethodInvoker(() => button_OpenCover.Enabled = state); button_OpenCover.Invoke(m);
                m = new MethodInvoker(() => button_CloseCover.Enabled = state); button_CloseCover.Invoke(m);
                m = new MethodInvoker(() => cmdOK.Enabled = state); cmdOK.Invoke(m);
                m = new MethodInvoker(() => cmdCancel.Enabled = state); cmdCancel.Invoke(m);
                m = new MethodInvoker(() => chkSimulation.Enabled = state); chkSimulation.Invoke(m);
                m = new MethodInvoker(() => comboBox_ComPort.Enabled = state); comboBox_ComPort.Invoke(m);
                m = new MethodInvoker(() => textBox_CoverAcceleration.Enabled = state); textBox_CoverAcceleration.Invoke(m);
                m = new MethodInvoker(() => textBox_CoverMoveSpeed.Enabled = state); textBox_CoverMoveSpeed.Invoke(m);
                m = new MethodInvoker(() => textBox_TargetAngle.Enabled = state); textBox_TargetAngle.Invoke(m);
            }
            else
            {
                button_OpenCover.Enabled = state;
                button_CloseCover.Enabled = state;
                cmdOK.Enabled = state;
                cmdCancel.Enabled = state;
                chkSimulation.Enabled = state;
                comboBox_ComPort.Enabled = state;
                textBox_CoverAcceleration.Enabled = state;
                textBox_CoverMoveSpeed.Enabled = state;
                textBox_TargetAngle.Enabled = state;
            }
        }

        private void thread_UpdateCoverCommands(bool is_otherthread)
        {
            if (is_otherthread)
            {
                MethodInvoker m;
                if (!driver.cover.IsOpen)
                {
                    m = new MethodInvoker(() => button_CloseCover.Enabled = false);
                    button_CloseCover.Invoke(m);
                    m = new MethodInvoker(() => button_OpenCover.Enabled = false);
                    button_OpenCover.Invoke(m);
                    return;
                }
                if (driver.currentAngle > 0)
                {
                    m = new MethodInvoker(() => button_CloseCover.Enabled = true);
                    button_CloseCover.Invoke(m);
                }
                else
                {
                    m = new MethodInvoker(() => button_CloseCover.Enabled = false);
                    button_CloseCover.Invoke(m);
                }
                if (driver.currentAngle < driver.targetAngle)
                {
                    m = new MethodInvoker(() => button_OpenCover.Enabled = true);
                    button_OpenCover.Invoke(m);
                }
                else
                {
                    m = new MethodInvoker(() => button_OpenCover.Enabled = false);
                    button_OpenCover.Invoke(m);
                }
            }
            else
            {
                if (!driver.cover.IsOpen)
                {
                    button_CloseCover.Enabled = false;
                    button_OpenCover.Enabled = false;
                    return;
                }
                if (driver.currentAngle > 0)
                    button_CloseCover.Enabled = true;
                else
                    button_CloseCover.Enabled = false;
                if (driver.currentAngle < driver.targetAngle)
                    button_OpenCover.Enabled = true;
                else
                    button_OpenCover.Enabled = false;
            }
        }

        private void thread_UpdateFlatPanelCommands(bool is_otherthread)
        {
            if (is_otherthread)
            {
                MethodInvoker m;
                if (!driver.flat_panel.IsOpen)
                {
                    m = new MethodInvoker(() => button_FlatPanelHigh.Enabled = false);
                    button_FlatPanelHigh.Invoke(m);
                    m = new MethodInvoker(() => button_FlatPanelLow.Enabled = false);
                    button_FlatPanelLow.Invoke(m);
                    m = new MethodInvoker(() => button_FlatPanelOff.Enabled = false);
                    button_FlatPanelOff.Invoke(m);
                    return;
                }
                driver.flat_panel.TurnOff();
                m = new MethodInvoker(() => button_FlatPanelHigh.Enabled = true);
                button_FlatPanelHigh.Invoke(m);
                m = new MethodInvoker(() => button_FlatPanelLow.Enabled = true);
                button_FlatPanelLow.Invoke(m);
                m = new MethodInvoker(() => button_FlatPanelOff.Enabled = false);
                button_FlatPanelOff.Invoke(m);
            }
            else
            {
                if (!driver.flat_panel.IsOpen)
                {
                    button_FlatPanelHigh.Enabled = false;
                    button_FlatPanelLow.Enabled = false;
                    button_FlatPanelOff.Enabled = false;
                    return;
                }
                driver.flat_panel.TurnOff();
                button_FlatPanelHigh.Enabled = true;
                button_FlatPanelLow.Enabled = true;
                button_FlatPanelOff.Enabled = false;
            }
        }
    }
}
