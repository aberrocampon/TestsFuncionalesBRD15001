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
    public partial class FormInforme : Form
    {
        public FormInforme()
        {
            InitializeComponent();

            DateTime dateTime = DateTime.UtcNow.Date;
            textBoxFecha.Text = dateTime.ToString("dd/MM/yyyy");
        }
    }
}
