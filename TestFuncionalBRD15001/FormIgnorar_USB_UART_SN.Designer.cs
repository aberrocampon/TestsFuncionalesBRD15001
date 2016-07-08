namespace TestFuncionalBRD15001
{
    partial class FormIgnorar_USB_UART_SN
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
            this.textBoxCOM = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxVID = new System.Windows.Forms.TextBox();
            this.textBoxPID = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonIgnorarSN = new System.Windows.Forms.Button();
            this.buttonNoIgnorarSN = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxEstadoSN = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxCOM
            // 
            this.textBoxCOM.Location = new System.Drawing.Point(73, 9);
            this.textBoxCOM.Name = "textBoxCOM";
            this.textBoxCOM.ReadOnly = true;
            this.textBoxCOM.Size = new System.Drawing.Size(65, 22);
            this.textBoxCOM.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Puerto";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "VID";
            // 
            // textBoxVID
            // 
            this.textBoxVID.Location = new System.Drawing.Point(73, 37);
            this.textBoxVID.Name = "textBoxVID";
            this.textBoxVID.ReadOnly = true;
            this.textBoxVID.Size = new System.Drawing.Size(65, 22);
            this.textBoxVID.TabIndex = 3;
            // 
            // textBoxPID
            // 
            this.textBoxPID.Location = new System.Drawing.Point(73, 65);
            this.textBoxPID.Name = "textBoxPID";
            this.textBoxPID.ReadOnly = true;
            this.textBoxPID.Size = new System.Drawing.Size(65, 22);
            this.textBoxPID.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "PID";
            // 
            // buttonIgnorarSN
            // 
            this.buttonIgnorarSN.Location = new System.Drawing.Point(154, 9);
            this.buttonIgnorarSN.Name = "buttonIgnorarSN";
            this.buttonIgnorarSN.Size = new System.Drawing.Size(115, 26);
            this.buttonIgnorarSN.TabIndex = 6;
            this.buttonIgnorarSN.Text = "Ignorar SN *";
            this.buttonIgnorarSN.UseVisualStyleBackColor = true;
            this.buttonIgnorarSN.Click += new System.EventHandler(this.buttonIgnorarSN_Click);
            // 
            // buttonNoIgnorarSN
            // 
            this.buttonNoIgnorarSN.Location = new System.Drawing.Point(154, 61);
            this.buttonNoIgnorarSN.Name = "buttonNoIgnorarSN";
            this.buttonNoIgnorarSN.Size = new System.Drawing.Size(115, 26);
            this.buttonNoIgnorarSN.TabIndex = 7;
            this.buttonNoIgnorarSN.Text = "No ignorar SN *";
            this.buttonNoIgnorarSN.UseVisualStyleBackColor = true;
            this.buttonNoIgnorarSN.Click += new System.EventHandler(this.buttonNoIgnorarSN_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 123);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 17);
            this.label6.TabIndex = 10;
            this.label6.Text = "Estado:";
            // 
            // textBoxEstadoSN
            // 
            this.textBoxEstadoSN.Location = new System.Drawing.Point(73, 120);
            this.textBoxEstadoSN.Name = "textBoxEstadoSN";
            this.textBoxEstadoSN.ReadOnly = true;
            this.textBoxEstadoSN.Size = new System.Drawing.Size(196, 22);
            this.textBoxEstadoSN.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 158);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 17);
            this.label4.TabIndex = 14;
            this.label4.Text = "* NOTA:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 175);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(255, 17);
            this.label5.TabIndex = 15;
            this.label5.Text = "Se necesitan privilegios administrativos";
            // 
            // FormIgnorar_USB_UART_SN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 217);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxEstadoSN);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.buttonNoIgnorarSN);
            this.Controls.Add(this.buttonIgnorarSN);
            this.Controls.Add(this.textBoxPID);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxVID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxCOM);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "FormIgnorar_USB_UART_SN";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Ignorar Serial Number";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxCOM;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxVID;
        private System.Windows.Forms.TextBox textBoxPID;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonIgnorarSN;
        private System.Windows.Forms.Button buttonNoIgnorarSN;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxEstadoSN;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}