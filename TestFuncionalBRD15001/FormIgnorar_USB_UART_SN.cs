using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestFuncionalBRD15001
{
    public partial class FormIgnorar_USB_UART_SN : Form
    {
        private string sCOM;
        private string sVID;
        private string sPID;
        private bool bIgnorando;

        public FormIgnorar_USB_UART_SN(string sCOM, string sVID, string sPID)
        {
            InitializeComponent();

            this.sCOM = sCOM;
            this.sVID = sVID;
            this.sPID = sPID;

            textBoxCOM.Text = sCOM;
            textBoxVID.Text = sVID;
            textBoxPID.Text = sPID;

            actualiza_estado();
        }

        private void actualiza_estado()
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\UsbFlags", false);
                if(rk.GetValue("IgnoreHWSerNum" + sVID + sPID) == null)
                {
                    textBoxEstadoSN.Text = "No ignorando el Serial Number";
                    bIgnorando = false;
                }
                else
                {
                    textBoxEstadoSN.Text = "Ignorando el Serial Number";
                    bIgnorando = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ignorar Serial Number del puente USB-Serie");
            }
        }

        private void buttonIgnorarSN_Click(object sender, EventArgs e)
        {
            if (bIgnorando) return;

            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\UsbFlags", true);
                rk.SetValue("IgnoreHWSerNum" + sVID + sPID, new byte[] { 1 }, RegistryValueKind.Binary);
                textBoxEstadoSN.Text = "Ignorando el Serial Number";
                bIgnorando = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ignorar Serial Number del puente USB-Serie");
            }
        }

        private void buttonNoIgnorarSN_Click(object sender, EventArgs e)
        {
            if (!bIgnorando) return;

            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\UsbFlags", true);
                rk.DeleteValue("IgnoreHWSerNum" + sVID + sPID);
                textBoxEstadoSN.Text = "No ignorando el Serial Number";
                bIgnorando = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ignorar Serial Number del puente USB-Serie");
            }
        }
    }
}
