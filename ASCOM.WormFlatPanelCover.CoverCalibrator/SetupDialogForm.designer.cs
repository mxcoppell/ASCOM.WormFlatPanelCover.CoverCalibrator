
namespace ASCOM.WormFlatPanelCover
{
    partial class SetupDialogForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.label_Title = new System.Windows.Forms.Label();
            this.picASCOM = new System.Windows.Forms.PictureBox();
            this.label_ComPort = new System.Windows.Forms.Label();
            this.chkTrace = new System.Windows.Forms.CheckBox();
            this.comboBox_ComPort = new System.Windows.Forms.ComboBox();
            this.label_CurrentAngle = new System.Windows.Forms.Label();
            this.label_TargetAngle = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button_CoverOperation = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox_CoverAcceleration = new System.Windows.Forms.TextBox();
            this.label_CoverAcceleration = new System.Windows.Forms.Label();
            this.label_CoverMoveSpeed = new System.Windows.Forms.Label();
            this.textBox_TargetAngle = new System.Windows.Forms.TextBox();
            this.label_CurrentAngleValue = new System.Windows.Forms.Label();
            this.label_CoverStatus = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.button_FlatPanelLow = new System.Windows.Forms.Button();
            this.comboBox_FlatPanels = new System.Windows.Forms.ComboBox();
            this.label_FlatPanels = new System.Windows.Forms.Label();
            this.button_FlatPanelHigh = new System.Windows.Forms.Button();
            this.button_FlatPanelOff = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdOK.Location = new System.Drawing.Point(192, 395);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(100, 31);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "确定";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdCancel.Location = new System.Drawing.Point(297, 395);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(100, 31);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "取消";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // label_Title
            // 
            this.label_Title.BackColor = System.Drawing.Color.DarkRed;
            this.label_Title.Font = new System.Drawing.Font("Microsoft YaHei UI", 22F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Title.ForeColor = System.Drawing.SystemColors.Menu;
            this.label_Title.Location = new System.Drawing.Point(11, 16);
            this.label_Title.Name = "label_Title";
            this.label_Title.Size = new System.Drawing.Size(287, 36);
            this.label_Title.TabIndex = 2;
            this.label_Title.Text = "虫子电动平场镜头盖";
            // 
            // picASCOM
            // 
            this.picASCOM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picASCOM.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picASCOM.Image = global::ASCOM.WormFlatPanelCover.Properties.Resources.ASCOM;
            this.picASCOM.Location = new System.Drawing.Point(349, 9);
            this.picASCOM.Name = "picASCOM";
            this.picASCOM.Size = new System.Drawing.Size(48, 56);
            this.picASCOM.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picASCOM.TabIndex = 3;
            this.picASCOM.TabStop = false;
            this.picASCOM.Click += new System.EventHandler(this.BrowseToAscom);
            this.picASCOM.DoubleClick += new System.EventHandler(this.BrowseToAscom);
            // 
            // label_ComPort
            // 
            this.label_ComPort.AutoSize = true;
            this.label_ComPort.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label_ComPort.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_ComPort.Location = new System.Drawing.Point(17, 24);
            this.label_ComPort.Name = "label_ComPort";
            this.label_ComPort.Size = new System.Drawing.Size(69, 19);
            this.label_ComPort.TabIndex = 5;
            this.label_ComPort.Text = "设备串口";
            // 
            // chkTrace
            // 
            this.chkTrace.AutoSize = true;
            this.chkTrace.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkTrace.Location = new System.Drawing.Point(10, 409);
            this.chkTrace.Name = "chkTrace";
            this.chkTrace.Size = new System.Drawing.Size(75, 21);
            this.chkTrace.TabIndex = 6;
            this.chkTrace.Text = "打开跟踪";
            this.chkTrace.UseVisualStyleBackColor = true;
            this.chkTrace.CheckedChanged += new System.EventHandler(this.chkTrace_CheckedChanged);
            // 
            // comboBox_ComPort
            // 
            this.comboBox_ComPort.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox_ComPort.FormattingEnabled = true;
            this.comboBox_ComPort.Location = new System.Drawing.Point(89, 19);
            this.comboBox_ComPort.Name = "comboBox_ComPort";
            this.comboBox_ComPort.Size = new System.Drawing.Size(82, 27);
            this.comboBox_ComPort.TabIndex = 7;
            this.comboBox_ComPort.Text = "COM9";
            // 
            // label_CurrentAngle
            // 
            this.label_CurrentAngle.AutoSize = true;
            this.label_CurrentAngle.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label_CurrentAngle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_CurrentAngle.Location = new System.Drawing.Point(16, 72);
            this.label_CurrentAngle.Name = "label_CurrentAngle";
            this.label_CurrentAngle.Size = new System.Drawing.Size(99, 19);
            this.label_CurrentAngle.TabIndex = 8;
            this.label_CurrentAngle.Text = "当前打开角度";
            // 
            // label_TargetAngle
            // 
            this.label_TargetAngle.AutoSize = true;
            this.label_TargetAngle.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label_TargetAngle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_TargetAngle.Location = new System.Drawing.Point(213, 72);
            this.label_TargetAngle.Name = "label_TargetAngle";
            this.label_TargetAngle.Size = new System.Drawing.Size(99, 19);
            this.label_TargetAngle.TabIndex = 9;
            this.label_TargetAngle.Text = "目标打开角度";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.DarkRed;
            this.panel1.Controls.Add(this.label_Title);
            this.panel1.Controls.Add(this.picASCOM);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(406, 74);
            this.panel1.TabIndex = 10;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.panel2.Controls.Add(this.button_CoverOperation);
            this.panel2.Controls.Add(this.textBox2);
            this.panel2.Controls.Add(this.textBox_CoverAcceleration);
            this.panel2.Controls.Add(this.label_CoverAcceleration);
            this.panel2.Controls.Add(this.label_CoverMoveSpeed);
            this.panel2.Controls.Add(this.textBox_TargetAngle);
            this.panel2.Controls.Add(this.label_CurrentAngleValue);
            this.panel2.Controls.Add(this.label_CoverStatus);
            this.panel2.Controls.Add(this.label_ComPort);
            this.panel2.Controls.Add(this.label_TargetAngle);
            this.panel2.Controls.Add(this.comboBox_ComPort);
            this.panel2.Controls.Add(this.label_CurrentAngle);
            this.panel2.Location = new System.Drawing.Point(0, 73);
            this.panel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(406, 200);
            this.panel2.TabIndex = 11;
            // 
            // button_CoverOperation
            // 
            this.button_CoverOperation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.button_CoverOperation.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_CoverOperation.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.button_CoverOperation.Location = new System.Drawing.Point(33, 141);
            this.button_CoverOperation.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_CoverOperation.Name = "button_CoverOperation";
            this.button_CoverOperation.Size = new System.Drawing.Size(335, 47);
            this.button_CoverOperation.TabIndex = 17;
            this.button_CoverOperation.Text = "关闭镜头盖";
            this.button_CoverOperation.UseVisualStyleBackColor = false;
            this.button_CoverOperation.Click += new System.EventHandler(this.button_CoverOperation_Click);
            // 
            // textBox2
            // 
            this.textBox2.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(118, 100);
            this.textBox2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(75, 27);
            this.textBox2.TabIndex = 16;
            this.textBox2.Text = "200";
            // 
            // textBox_CoverAcceleration
            // 
            this.textBox_CoverAcceleration.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_CoverAcceleration.Location = new System.Drawing.Point(314, 100);
            this.textBox_CoverAcceleration.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_CoverAcceleration.Name = "textBox_CoverAcceleration";
            this.textBox_CoverAcceleration.Size = new System.Drawing.Size(75, 27);
            this.textBox_CoverAcceleration.TabIndex = 15;
            this.textBox_CoverAcceleration.Text = "10";
            this.textBox_CoverAcceleration.TextChanged += new System.EventHandler(this.textBox_CoverAcceleration_TextChanged);
            // 
            // label_CoverAcceleration
            // 
            this.label_CoverAcceleration.AutoSize = true;
            this.label_CoverAcceleration.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label_CoverAcceleration.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_CoverAcceleration.Location = new System.Drawing.Point(228, 105);
            this.label_CoverAcceleration.Name = "label_CoverAcceleration";
            this.label_CoverAcceleration.Size = new System.Drawing.Size(84, 19);
            this.label_CoverAcceleration.TabIndex = 13;
            this.label_CoverAcceleration.Text = "运行加速度";
            // 
            // label_CoverMoveSpeed
            // 
            this.label_CoverMoveSpeed.AutoSize = true;
            this.label_CoverMoveSpeed.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label_CoverMoveSpeed.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_CoverMoveSpeed.Location = new System.Drawing.Point(16, 105);
            this.label_CoverMoveSpeed.Name = "label_CoverMoveSpeed";
            this.label_CoverMoveSpeed.Size = new System.Drawing.Size(99, 19);
            this.label_CoverMoveSpeed.TabIndex = 12;
            this.label_CoverMoveSpeed.Text = "打开关闭速度";
            // 
            // textBox_TargetAngle
            // 
            this.textBox_TargetAngle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_TargetAngle.Location = new System.Drawing.Point(314, 67);
            this.textBox_TargetAngle.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_TargetAngle.Name = "textBox_TargetAngle";
            this.textBox_TargetAngle.Size = new System.Drawing.Size(75, 27);
            this.textBox_TargetAngle.TabIndex = 11;
            this.textBox_TargetAngle.Text = "525";
            this.textBox_TargetAngle.TextChanged += new System.EventHandler(this.textBox_TargetAngle_TextChanged);
            // 
            // label_CurrentAngleValue
            // 
            this.label_CurrentAngleValue.BackColor = System.Drawing.Color.DimGray;
            this.label_CurrentAngleValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_CurrentAngleValue.ForeColor = System.Drawing.Color.White;
            this.label_CurrentAngleValue.Location = new System.Drawing.Point(118, 67);
            this.label_CurrentAngleValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_CurrentAngleValue.Name = "label_CurrentAngleValue";
            this.label_CurrentAngleValue.Size = new System.Drawing.Size(75, 27);
            this.label_CurrentAngleValue.TabIndex = 10;
            this.label_CurrentAngleValue.Text = "525";
            this.label_CurrentAngleValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_CoverStatus
            // 
            this.label_CoverStatus.BackColor = System.Drawing.Color.Green;
            this.label_CoverStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_CoverStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_CoverStatus.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label_CoverStatus.Location = new System.Drawing.Point(195, 11);
            this.label_CoverStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_CoverStatus.Name = "label_CoverStatus";
            this.label_CoverStatus.Size = new System.Drawing.Size(194, 40);
            this.label_CoverStatus.TabIndex = 8;
            this.label_CoverStatus.Text = "镜头盖已打开";
            this.label_CoverStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel3.Controls.Add(this.button_FlatPanelLow);
            this.panel3.Controls.Add(this.comboBox_FlatPanels);
            this.panel3.Controls.Add(this.label_FlatPanels);
            this.panel3.Controls.Add(this.button_FlatPanelHigh);
            this.panel3.Controls.Add(this.button_FlatPanelOff);
            this.panel3.Location = new System.Drawing.Point(0, 273);
            this.panel3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(408, 100);
            this.panel3.TabIndex = 12;
            // 
            // button_FlatPanelLow
            // 
            this.button_FlatPanelLow.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_FlatPanelLow.Location = new System.Drawing.Point(272, 51);
            this.button_FlatPanelLow.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_FlatPanelLow.Name = "button_FlatPanelLow";
            this.button_FlatPanelLow.Size = new System.Drawing.Size(115, 34);
            this.button_FlatPanelLow.TabIndex = 15;
            this.button_FlatPanelLow.Text = "平场弱光";
            this.button_FlatPanelLow.UseVisualStyleBackColor = true;
            // 
            // comboBox_FlatPanels
            // 
            this.comboBox_FlatPanels.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox_FlatPanels.FormattingEnabled = true;
            this.comboBox_FlatPanels.Location = new System.Drawing.Point(112, 16);
            this.comboBox_FlatPanels.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBox_FlatPanels.Name = "comboBox_FlatPanels";
            this.comboBox_FlatPanels.Size = new System.Drawing.Size(275, 28);
            this.comboBox_FlatPanels.TabIndex = 13;
            this.comboBox_FlatPanels.Text = "TAKNB";
            // 
            // label_FlatPanels
            // 
            this.label_FlatPanels.AutoSize = true;
            this.label_FlatPanels.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label_FlatPanels.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_FlatPanels.Location = new System.Drawing.Point(23, 21);
            this.label_FlatPanels.Name = "label_FlatPanels";
            this.label_FlatPanels.Size = new System.Drawing.Size(84, 19);
            this.label_FlatPanels.TabIndex = 17;
            this.label_FlatPanels.Text = "平场板设备";
            // 
            // button_FlatPanelHigh
            // 
            this.button_FlatPanelHigh.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.button_FlatPanelHigh.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_FlatPanelHigh.Location = new System.Drawing.Point(20, 51);
            this.button_FlatPanelHigh.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_FlatPanelHigh.Name = "button_FlatPanelHigh";
            this.button_FlatPanelHigh.Size = new System.Drawing.Size(115, 34);
            this.button_FlatPanelHigh.TabIndex = 13;
            this.button_FlatPanelHigh.Text = "平场强光";
            this.button_FlatPanelHigh.UseVisualStyleBackColor = false;
            // 
            // button_FlatPanelOff
            // 
            this.button_FlatPanelOff.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.button_FlatPanelOff.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_FlatPanelOff.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.button_FlatPanelOff.Location = new System.Drawing.Point(146, 51);
            this.button_FlatPanelOff.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_FlatPanelOff.Name = "button_FlatPanelOff";
            this.button_FlatPanelOff.Size = new System.Drawing.Size(115, 34);
            this.button_FlatPanelOff.TabIndex = 14;
            this.button_FlatPanelOff.Text = "关闭平场";
            this.button_FlatPanelOff.UseVisualStyleBackColor = false;
            // 
            // SetupDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 436);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.chkTrace);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupDialogForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "配置虫子电动平场镜头盖";
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label label_Title;
        private System.Windows.Forms.PictureBox picASCOM;
        private System.Windows.Forms.Label label_ComPort;
        private System.Windows.Forms.CheckBox chkTrace;
        private System.Windows.Forms.ComboBox comboBox_ComPort;
        private System.Windows.Forms.Label label_CurrentAngle;
        private System.Windows.Forms.Label label_TargetAngle;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label_CoverStatus;
        private System.Windows.Forms.Label label_CurrentAngleValue;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox_CoverAcceleration;
        private System.Windows.Forms.Label label_CoverAcceleration;
        private System.Windows.Forms.Label label_CoverMoveSpeed;
        private System.Windows.Forms.TextBox textBox_TargetAngle;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label_FlatPanels;
        private System.Windows.Forms.Button button_FlatPanelLow;
        private System.Windows.Forms.ComboBox comboBox_FlatPanels;
        private System.Windows.Forms.Button button_FlatPanelOff;
        private System.Windows.Forms.Button button_FlatPanelHigh;
        private System.Windows.Forms.Button button_CoverOperation;
    }
}