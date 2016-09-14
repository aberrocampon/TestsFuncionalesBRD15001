namespace TestFuncionalBRD15001
{
    partial class Form2
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
            this.progressBarCargaFirmware = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // progressBarCargaFirmware
            // 
            this.progressBarCargaFirmware.Location = new System.Drawing.Point(12, 12);
            this.progressBarCargaFirmware.Name = "progressBarCargaFirmware";
            this.progressBarCargaFirmware.Size = new System.Drawing.Size(260, 22);
            this.progressBarCargaFirmware.TabIndex = 0;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 53);
            this.ControlBox = false;
            this.Controls.Add(this.progressBarCargaFirmware);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form2";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Carga de firmware de test al DSP";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ProgressBar progressBarCargaFirmware;
    }
}