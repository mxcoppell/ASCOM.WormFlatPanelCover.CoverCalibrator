using ASCOM.Utilities;
using ASCOM.WormFlatPanelCover;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

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
        }

        private void chkTrace_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button_CoverOperation_Click(object sender, EventArgs e)
        {

        }

        private void textBox_CoverAcceleration_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox_TargetAngle_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox_ComPort_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button_FlatPanelHigh_Click(object sender, EventArgs e)
        {

        }

        private void button_FlatPanelOff_Click(object sender, EventArgs e)
        {

        }

        private void button_FlatPanelLow_Click(object sender, EventArgs e)
        {

        }

        private void button_CloseCover_Click(object sender, EventArgs e)
        {

        }

        private void label_CurrentAngleValue_Click(object sender, EventArgs e)
        {

        }

        private void chkSimulation_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}