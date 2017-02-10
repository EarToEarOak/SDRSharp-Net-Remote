/*
 SDRSharp Net Remote

 http://eartoearoak.com/software/sdrsharp-net-remote

 Copyright 2014 - 2015 Al Brown

 A network remote control plugin for SDRSharp


 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, or (at your option)
 any later version.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace SDRSharp.NetRemote
{
    partial class ControlPanel
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkNetwork = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.checkSerial = new System.Windows.Forms.CheckBox();
            this.comboSerial = new System.Windows.Forms.ComboBox();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.SuspendLayout();
            // 
            // checkNetwork
            // 
            this.checkNetwork.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkNetwork.AutoSize = true;
            this.checkNetwork.Checked = true;
            this.checkNetwork.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkNetwork.Location = new System.Drawing.Point(3, 7);
            this.checkNetwork.Name = "checkNetwork";
            this.checkNetwork.Size = new System.Drawing.Size(66, 17);
            this.checkNetwork.TabIndex = 0;
            this.checkNetwork.Text = "Network";
            this.checkNetwork.UseVisualStyleBackColor = true;
            this.checkNetwork.CheckedChanged += new System.EventHandler(this.CheckChangedNetwork);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.checkNetwork, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.checkSerial, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.comboSerial, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.numPort, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(252, 63);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // checkSerial
            // 
            this.checkSerial.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkSerial.AutoSize = true;
            this.checkSerial.Checked = true;
            this.checkSerial.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkSerial.Location = new System.Drawing.Point(3, 38);
            this.checkSerial.Name = "checkSerial";
            this.checkSerial.Size = new System.Drawing.Size(52, 17);
            this.checkSerial.TabIndex = 1;
            this.checkSerial.Text = "Serial";
            this.checkSerial.UseVisualStyleBackColor = true;
            this.checkSerial.CheckedChanged += new System.EventHandler(this.CheckChangedSerial);
            // 
            // comboSerial
            // 
            this.comboSerial.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboSerial.FormattingEnabled = true;
            this.comboSerial.Location = new System.Drawing.Point(129, 36);
            this.comboSerial.Name = "comboSerial";
            this.comboSerial.Size = new System.Drawing.Size(97, 21);
            this.comboSerial.TabIndex = 2;
            // 
            // numPort
            // 
            this.numPort.Location = new System.Drawing.Point(129, 3);
            this.numPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(120, 20);
            this.numPort.TabIndex = 3;
            this.numPort.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // ControlPanel
            // 
            this.AutoSize = true;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ControlPanel";
            this.Size = new System.Drawing.Size(252, 63);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox checkNetwork;
        private System.Windows.Forms.CheckBox checkSerial;
        private System.Windows.Forms.ComboBox comboSerial;
        private System.Windows.Forms.NumericUpDown numPort;
    }
}
