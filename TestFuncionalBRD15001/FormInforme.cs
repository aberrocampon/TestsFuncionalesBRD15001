using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;

namespace TestFuncionalBRD15001
{
    public partial class FormInforme : Form
    {
        private int[] leyenda_resultados_tests;
        private string cad_versiones_hw;
        private SeleccionPlaca seleccionPlaca;
        DateTime dateTime;

        public FormInforme(SeleccionPlaca seleccionPlaca, string informe, int[] leyenda_resultados_tests, string cad_versiones_hw)
        {
            InitializeComponent();

            this.leyenda_resultados_tests = leyenda_resultados_tests;
            this.cad_versiones_hw = cad_versiones_hw;
            this.seleccionPlaca = seleccionPlaca;

            dateTime = DateTime.UtcNow;
            textBoxFecha.Text = dateTime.ToString("HH:mm:ss-dd/MM/yyyy");
            textBoxInforme.Text = informe;

            if(seleccionPlaca == SeleccionPlaca.BRD15003)
            {
                labelFuente4_20mA.Text = "Referencia de la placa BRD15003 (placa de sincronismo):";
                labelFuenteTension.Visible = false;
                textBoxFuenteTension.Visible = false;
            }
        }

        private void buttonGuardar_Click(object sender, EventArgs e)
        {
            if(leyenda_resultados_tests[(int)TipoTest.TEST_LEER_VERSIONES] != 1)
            {
                MessageBox.Show("No se han leido aun todas las versiones de software y hardware", "Guardar Informe", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if ((leyenda_resultados_tests.Contains<int>(-1)) || (leyenda_resultados_tests.Contains<int>(0)))
            {
                string warning = "";

                if ((leyenda_resultados_tests.Contains<int>(-1)) && (leyenda_resultados_tests.Contains<int>(0)))
                    warning = "Existen tests no ejecutados, existen tests con indicacion de fallos\r\n";
                else if (leyenda_resultados_tests.Contains<int>(-1))
                    warning = "Existen tests no ejecutados\r\n";
                else if (leyenda_resultados_tests.Contains<int>(0))
                    warning = "Existen tests con indicacion de fallos\r\n";

                DialogResult res = MessageBox.Show(warning + "¿Desea guardar el informe de todos modos?", "Guardar Informe", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (res == DialogResult.Cancel) return;
            }

            if((textBoxLocalizacion.Text.Length == 0) || (textBoxFuenteTension.Text.Length == 0) || (textBoxFuente4_20mA.Text.Length == 0) ||
                (textBoxIDTecnico.Text.Length == 0) || (textBoxRefBRD15001.Text.Length == 0) || (textBoxRef10030_2001.Text.Length == 0))
            {
                DialogResult res = MessageBox.Show("Campos vacios en el formulario\r\n¿Desea guardar el informe de todos modos?", "Guardar Informe", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if(res == DialogResult.Cancel) return;
            }

            string contenido;
            contenido = "Hora / Fecha (UTC): " + textBoxFecha.Text + "\r\n";
            contenido += "Localizacion:\r\n" + textBoxLocalizacion.Text + "\r\n";
            contenido += "ID/Nombre de técnico:\r\n" + textBoxIDTecnico.Text + "\r\n";
            contenido += "Referencia de la placa 10030_2001 (DSP/FPGA):\r\n" + textBoxRef10030_2001.Text + "\r\n";
            contenido += "Referencia de la placa BRD15001 (placa base o soporte):\r\n" + textBoxRefBRD15001.Text + "\r\n";
            if (seleccionPlaca == SeleccionPlaca.BRD15001)
            {
                contenido += "Fabricante/Modelo o ID de la fuente de corriente 4-20mA (referencia en test canales ADC):\r\n" + textBoxFuente4_20mA.Text + "\r\n";
                contenido += "Fabricante/Modelo o ID de la fuente de tensión (referencia en test canales ADC):\r\n" + textBoxFuenteTension.Text + "\r\n\r\n";
            }
            else
            {
                contenido += "Referencia de la placa BRD15003 (placa de sincronismo):\r\n" + textBoxFuente4_20mA.Text + "\r\n";
            }
            contenido += textBoxInforme.Text; //+ "\r\n";

            //byte[] buffer_byte = Encoding.UTF8.GetBytes(contenido);

            //for(int i=0; i<buffer_byte.Length;i++)
            //{
            //    byte aux = (byte)(i & 0xff);
            //    buffer_byte[i] = (byte)(buffer_byte[i] ^ aux);
            //}

            //string checksum;
            //using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            //{
            //    checksum = BitConverter.ToString(md5.ComputeHash(buffer_byte)).Replace("-", String.Empty);
            //}

            //contenido += "CHECKSUM=" + checksum;

            if (seleccionPlaca == SeleccionPlaca.BRD15001)
            {
                saveFileDialogInforme.FileName = "INF_TESTS_BRD15001_" + cad_versiones_hw + "_" + dateTime.ToString("hh-mm-ss_dd-MM-yyyy");
            }
            else
            {
                saveFileDialogInforme.FileName = "INF_TESTS_BRD15003_" + cad_versiones_hw + "_" + dateTime.ToString("hh-mm-ss_dd-MM-yyyy");
            }
            DialogResult res_dial_guarda_fic = saveFileDialogInforme.ShowDialog();
            
            if(res_dial_guarda_fic == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(saveFileDialogInforme.FileName, false);
                writer.Write(contenido);
                writer.Close();
            }


            ////////////////////////////////////////
            //StreamReader reader = new StreamReader(saveFileDialogInforme.FileName);
            //string comprobar = reader.ReadToEnd();
            //string comprobar_checksum = comprobar.Substring(comprobar.IndexOf("CHECKSUM="));
            //comprobar = comprobar.Substring(0, comprobar.IndexOf("CHECKSUM="));
            //byte[] bb_comprobar = Encoding.UTF8.GetBytes(comprobar);
            //for (int i = 0; i < bb_comprobar.Length; i++)
            //{
            //    byte aux = (byte)(i & 0xff);
            //    bb_comprobar[i] = (byte)(bb_comprobar[i] ^ aux);
            //}
            //string checksum_comprobar;
            //using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            //{
            //    checksum_comprobar = BitConverter.ToString(md5.ComputeHash(bb_comprobar)).Replace("-", String.Empty);
            //}
            //while (true) ;
            ////////////////////////////////////////
        }

        private void buttonInformeCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
