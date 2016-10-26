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
        public bool comenzarTestFO_parte1, comenzarTestFO_parte2;

        public FormIniciarTestFO()
        {
            InitializeComponent();

            comenzarTestFO_parte1 = false;
            comenzarTestFO_parte2 = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comenzarTestFO_parte2 = true;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comenzarTestFO_parte1 = true;
            Close();
        }
    }
}
