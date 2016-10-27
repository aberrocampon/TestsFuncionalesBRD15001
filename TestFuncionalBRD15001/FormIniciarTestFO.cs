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
    public partial class FormIniciarTestFO : Form
    {
        public bool comenzarTestFO_boton1, comenzarTestFO_boton2;

        public FormIniciarTestFO(string textoLabel, string textoOmitir, string textoTitulo)
        {
            InitializeComponent();

            label1.Text = textoLabel;
            button2.Text = textoOmitir;
            this.Text = textoTitulo;

            comenzarTestFO_boton1 = false;
            comenzarTestFO_boton2 = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comenzarTestFO_boton2 = true;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comenzarTestFO_boton1 = true;
            Close();
        }
    }
}
