using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using FTD2XX_NET;
using System.IO;
using System.Deployment.Application;

namespace TestFuncionalBRD15001
{

    enum TipoTest
    {
        NO_TEST = -1,
        TEST_NTCS = 0,
        TEST_SPVS = 1,
        TEST_ADCS = 2,
        TEST_IO = 3,
        TEST_TXRXOPTICOS = 4,
        TEST_TURBINAS = 5,
        TEST_RS422 = 6,
        TEST_LEER_VERSIONES = 7,
        TEST_CAN = 8,
        TEST_SRAM = 9,
        TEST_FRAM = 10,
        TEST_RTC = 11,
        TEST_NUMERO_TOTAL = 12,
    }

    public partial class Form1 : Form
    {
        private int[] leyenda_resultados_tests = new int[(int)TipoTest.TEST_NUMERO_TOTAL];
        private int comando_actual = 0;
        private int contador_comandos = 0;
        private int timeout_esperarespuesta = 0;
        private int output, outputleds;
        private int respuesta_output_ok, respuesta_outputleds_ok, respuesta_disparos_ok, respuesta_dutyturbina_ok;
        private int respuesta_enabletx_rs422_ok, respuesta_enablerx_rs422_ok;
        private int input;
        private TipoTest test = TipoTest.NO_TEST;
        private int contador_test = 0;
        private string buffer_tx_rs422_test = "";
        private string[] resistenciasNTC;
        private double[] refNTCs = { -1.0, -1.0, -1.0, -1.0, -1.0 };
        private int leeNTC;
        private string cadena_USB_UART_VID = "";
        private string cadena_USB_UART_PID = "";
        private string cadena_USB_UART_SN = "";
        private string cadena_versiones = "";
        private string cadena_ver_fpga = "";
        private string cadena_ver_firmware = "";
        private string cadena_id_dna = "";
        private string cadena_dsp_partid = "";
        private string cadena_dsp_classid = "";
        private string cadena_dsp_revid = "";
        private string buffer_rx = "";
        private string buffer_tx = "";
        private string buffer_tx_rs422 = "";
        private string buffer_rx_rs422 = "";
        private double temperatura1, temperatura2, ref_temp1 = -1.0, ref_temp2 = -1.0;
        private double vin, ref_vin = -1.0, vdsp, vfo, vreles, v3_3v, vbat, ref_vbat = -1.0;
        private double iin, ref_iin = -1.0;
        private int dutyturbina1, dutyturbina2, dutyturbina3;
        private double rpm1, rpm2, rpm3;
        private int disparos = 7, errores;
        private int enabletx_rs422 = 0, enablerx_rs422;
        private const int num_samples = 20;
        private int[,] voltaje = new int[16, num_samples];
        private int canaladc = 0;
        private int canatx, canarx, canbtx, canbrx;
        private int respuesta_canatx_ok, respuesta_canbtx_ok;
        private bool con_cable_can;
        private int respuesta_test_sram;
        private int respuesta_test_fram;
        private int respuesta_leertc;
        private int respuesta_escrtc;
        private DateTime hora_inicio_rtc_enhora;
        private bool estado_en_hora;
        private string cadena_hora_dsp = "";
        private int timeout_secuencia_test;
        private int periodo_peticion_ping;
        private double[] medias_adcs = new double[16];
        private double[] medias_adcs_vi = new double[16];
        private int canal_adc_seleccionado;
        private RadioButton radioButtonADC_seleccionado;

        // Resultados de tests para generar informes
        // Canales ADC
        private double[] test_adcs_referencias = new double[16];
        private double[] test_adcs_tolerancias = new double[16];
        private double[] test_adcs_medias = new double[16];
        private int[] test_adcs_ok_fallo = new int[16]; // -1 => no test superado, 0 => fallo, 1 => ok
        // Supervisores de Tension/corriente/temperatura
        private double test_spv_temp1_med, test_spv_temp1_ref;
        private double test_spv_temp2_med;
        private double test_spv_vin_med, test_spv_vin_ref, test_spv_vin_tol;
        private double test_spv_iin_med, test_spv_iin_ref, test_spv_iin_tol;
        private double test_spv_vdsp_med, test_spv_vdsp_ref, test_spv_vdsp_tol;
        private double test_spv_vfo_med, test_spv_vfo_ref, test_spv_vfo_tol;
        private double test_spv_vreles_med, test_spv_vreles_ref, test_spv_vreles_tol;
        private double test_spv_v3_3v_med, test_spv_v3_3v_ref, test_spv_v3_3v_tol;
        private double test_spv_vbat_med, test_spv_vbat_ref, test_spv_vbat_tol;
        private int test_spv_ok_fallo_temp1, test_spv_ok_fallo_temp2, test_spv_ok_fallo_vin, test_spv_ok_fallo_iin;
        private int test_spv_ok_fallo_vdsp, test_spv_ok_fallo_vfo, test_spv_ok_fallo_vreles, test_spv_ok_fallo_v3_3v, test_spv_ok_fallo_vbat;
        // Medida de resistencias NTC
        private double[] test_ntcs_referencias = new double[5];
        private double[] test_ntcs_tolerancias = new double[5];
        private double[] test_ntcs_medidas = new double[5];
        private int[] test_ntcs_ok_fallo = new int[5]; // -1 => no test superado, 0 => fallo, 1 => ok
        // RS-422
        private int test_rs422_loop_rxdis_txdis_ok_fallo;
        private int test_rs422_loop_rxdis_txena_ok_fallo;
        private int test_rs422_loop_rxena_txdis_ok_fallo;
        private int test_rs422_loop_rxena_txena_ok_fallo;
        // CAN
        private int test_cana_loop_ok_fallo;
        private int test_canb_loop_ok_fallo;
        private int test_cana_canb_ok_fallo;
        //SRAM
        private int test_sram_tiempo_test;
        // FRAM
        private int test_fram_escritura1_ok_fallo;
        private int test_fram_tiempo_test_escritura1;
        private int test_fram_escritura2_ok_fallo;
        private int test_fram_tiempo_test_escritura2;
        private int test_fram_bloqueo_ok_fallo;
        private int test_fram_tiempo_test_bloqueo;
        private int test_fram_desbloqueo_ok_fallo;
        private int test_fram_tiempo_test_desbloqueo;
        // control velocidad turbinas
        private int test_ctrlturbina1_ok_fallo;
        private double test_ctrlturbina1_pulsos_sec_0;
        private double test_ctrlturbina1_pulsos_sec_50;
        private int test_ctrlturbina2_ok_fallo;
        private double test_ctrlturbina2_pulsos_sec_0;
        private double test_ctrlturbina2_pulsos_sec_50;
        private int test_ctrlturbina3_ok_fallo;
        private double test_ctrlturbina3_pulsos_sec_0;
        private double test_ctrlturbina3_pulsos_sec_50;
        // Entradas / salidas digitales
        private int[] test_io_reles_entradas_ok_fallo = new int[16];
        private int[] test_io_leds_entradas_ok_fallo = new int[5];
        // Transmisores / receptores opticos
        private int[] test_fo_disp_error_ok_fallo = new int[6];
        private int[] test_fo_disp_ovt_ok_fallo = new int[3];
        private int test_fo_bot_s_rxsynch_ok_fallo;
        // rtc
        private int test_rtc_puesta_en_hora_ok_fallo;
        private int test_rtc_en_hora_ok_fallo;
        private int test_rtc_duracion_en_hora;


        // Marcas de ok fallo de tests
        private PictureBox[] marcasADCs;
        private PictureBox[] marcasSupervisores;
        private PictureBox[] marcasNTCs;
        private PictureBox[] marcasReles;
        private PictureBox[] marcasInputs;
        private PictureBox[] marcasLEDs;
        private PictureBox[] marcasDisparos;
        private PictureBox[] marcasErroresOpticos;
        private PictureBox[] marcasTurbinas;

        // tablas de asignacion de canales ADC en el ComboBox
        int[] canalesADCcombo = { 12, 8, 4, 0, 13, 9, 5, 1, 2, 6, 10, 14, 3, 7, 11, 15 };
        // dimensiones fisicas de las medidas de cada canal ADC
        string[] dimension_ref_adc = {"V", "V", "mA", "V", "V", "V", "mA", "V",
                                          "mA", "V", "mA", "V", "mA", "V", "mA", "V"};

        // bandera para llamar a reset informes desde el timer2 a peticion de la isr de recepcion de puerto serie
        private bool flag_cambia_versiones = false;

        private const string VER_SOFTWARE = "1.1.0.ND";
        private string sLabelVersionSoftware = "V" + VER_SOFTWARE;

        public Form1()
        {
            InitializeComponent();

            //serialPort1.Open();
            botonConectarDesconectar.Text = "Conectar";
            actualiza_COMs();

            periodo_peticion_ping = 0;
            output = 0;
            comando_actual = 0;
            contador_comandos = 0;
            timeout_esperarespuesta = 0;
            enabletx_rs422 = 0;
            enablerx_rs422 = 0;
            disparos = 7;
            canatx = 0;
            canarx = 0;
            canbtx = 0;
            canbrx = 0;
            resistenciasNTC = new string[5];
            for (int i = 0; i < 5; i++)
            {
                resistenciasNTC[i] = "0";
            }
            leeNTC = 0;

            // canal seleccionado por defecto
            radioButtonADC_seleccionado = radioButtonADCA0;
            comboBoxCanalADCseleccionado.SelectedIndex = 0;

            // inicializa marcas de fallo/ok tests
            marcasADCs = new PictureBox[]{
                                    pictureBoxCanalA0, pictureBoxCanalB0, pictureBoxCanalA1, pictureBoxCanalB1,
                                    pictureBoxCanalA2, pictureBoxCanalB2, pictureBoxCanalA3, pictureBoxCanalB3,
                                    pictureBoxCanalA4, pictureBoxCanalB4, pictureBoxCanalA5, pictureBoxCanalB5,
                                    pictureBoxCanalA6, pictureBoxCanalB6, pictureBoxCanalA7, pictureBoxCanalB7};
            marcasSupervisores = new PictureBox[]{pictureBoxTemp1, pictureBoxTemp2, pictureBoxVin, pictureBoxIin,
                                               pictureBoxVdsp, pictureBoxVfibopt, pictureBoxVreles,
                                               pictureBoxV3_3V, pictureBoxVbat };
            marcasNTCs = new PictureBox[]{pictureBoxNTC0, pictureBoxNTC1, pictureBoxNTC2, pictureBoxNTC3,
                                       pictureBoxNTC4 };
            marcasReles = new PictureBox[]{ pictureBoxRele0, pictureBoxRele1, pictureBoxRele2, pictureBoxRele3,
                                    pictureBoxRele4, pictureBoxRele5, pictureBoxRele6, pictureBoxRele7,
                                    pictureBoxRele8, pictureBoxRele9, pictureBoxRele10, pictureBoxRele11,
                                    pictureBoxRele12, pictureBoxRele13, pictureBoxRele14, pictureBoxRele15};
            marcasInputs = new PictureBox[]{ pictureBoxInput0, pictureBoxInput1, pictureBoxInput2, pictureBoxInput3,
                                    pictureBoxInput4, pictureBoxInput5, pictureBoxInput6, pictureBoxInput7,
                                    pictureBoxInput8, pictureBoxInput9, pictureBoxInput10, pictureBoxInput11,
                                    pictureBoxInput12, pictureBoxInput13, pictureBoxInput14, pictureBoxInput15};
            marcasLEDs = new PictureBox[] { pictureBoxLed0, pictureBoxLed1, pictureBoxLed2, pictureBoxLed3, pictureBoxLed4 };
            marcasDisparos = new PictureBox[]{ pictureBoxDisparoTopR, pictureBoxDisparoBotR,
                                    pictureBoxDisparoTopS, pictureBoxDisparoBotS,
                                    pictureBoxDisparoTopT, pictureBoxDisparoBotT };
            marcasErroresOpticos = new PictureBox[]{ pictureBoxErrorOvtR, pictureBoxErrorOvtS, pictureBoxErrorOvtT,
                                    pictureBoxErrorTopR, pictureBoxErrorTopS, pictureBoxErrorTopT,
                                    pictureBoxErrorBotR, pictureBoxErrorBotS, pictureBoxErrorBotT};
            marcasTurbinas = new PictureBox[] { pictureBoxTurbina1, pictureBoxTurbina2, pictureBoxTurbina3 };

            leyenda_resultados_tests[(int)TipoTest.TEST_LEER_VERSIONES] = -1;
            reset_informes_tests(true);

            try
            {
                Version version = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                sLabelVersionSoftware = "V" + version;
            }
            catch(Exception)
            {
            }

            labelVersion.Text = sLabelVersionSoftware;

            //buttonIgnorarSN.Visible = false;
        }

        private void panelCanalADC_Click(object sender, EventArgs e)
        {
            Panel pn = sender as Panel;
            if (pn != null)
            {
                string tag = pn.Tag.ToString();
                int canal = Int32.Parse(tag);
                int indiceComboBoxCanalesADC = Array.IndexOf(canalesADCcombo, canal);
                comboBoxCanalADCseleccionado.SelectedIndex = indiceComboBoxCanalesADC;
            }
        }

        private void actualiza_marcas_leyenda_resultados_tests()
        {
            PictureBox[] marcasLeyendaResultadosTests = {pictureBoxResultadoNTCs, pictureBoxResultadoSPVs,
                pictureBoxResultadoADCs, pictureBoxResultadoIO, pictureBoxResultadoTXRXOpt, pictureBoxResultadoTurbinas,
                pictureBoxResultadoRS422, pictureBoxLeerVersiones, pictureBoxResultadoCAN, pictureBoxResultadoSRAM,
                pictureBoxResultadoFRAM, pictureBoxResultadoRTC};

            for (int i = 0; i < leyenda_resultados_tests.Length; i++)
            {
                if (leyenda_resultados_tests[i] == -1) marcasLeyendaResultadosTests[i].Image = TestFuncionalBRD15001.Properties.Resources.gnome_help;
                else if (leyenda_resultados_tests[i] == 0) marcasLeyendaResultadosTests[i].Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                else marcasLeyendaResultadosTests[i].Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
            }
        }

        private void buttonPonHora_Click(object sender, EventArgs e)
        {
            if ((test == TipoTest.NO_TEST) && serialPort1.IsOpen)
            {
                test = TipoTest.TEST_RTC;
                contador_test = 0;
                timer1.Enabled = true;
            }
        }

        private void actualiza_COMs()
        {
            string[] nombresCOMs = SerialPort.GetPortNames();

            comboBoxCOMs.Items.Clear();
            foreach (string nombre_com in nombresCOMs)
            {
                comboBoxCOMs.Items.Add(nombre_com);
            }

            if (comboBoxCOMs.Items.Count > 0)
                comboBoxCOMs.SelectedIndex = 0;
            else
            {
                comboBoxCOMs.SelectedIndex = -1;
                comboBoxCOMs.Text = "";
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string nombreCOM = comboBoxCOMs.GetItemText(comboBoxCOMs.SelectedItem);

            if ((cadena_USB_UART_VID.Length == 0) || (cadena_USB_UART_PID.Length == 0))
            {
                if (test != TipoTest.NO_TEST)
                {
                    timer1.Enabled = false;
                    DialogResult res = MessageBox.Show("¿Desea cancelar el test actual?", "Ignorar Serial Number", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (res == DialogResult.No)
                    {
                        timer1.Enabled = true;
                        return;
                    }
                    else test = TipoTest.NO_TEST;
                }

                try
                {
                    serialPort1.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ignorar Serial Number");
                    return;
                }
                botonConectarDesconectar.Text = "Conectar";

                lee_pid_vid_serial_usb_uart(nombreCOM);
            }

            if ((cadena_USB_UART_VID.Length != 0) && (cadena_USB_UART_PID.Length != 0))
            {
                FormIgnorar_USB_UART_SN formIgnorar = new FormIgnorar_USB_UART_SN(nombreCOM, cadena_USB_UART_VID, cadena_USB_UART_PID);
                formIgnorar.ShowDialog();
            }
        }

        private string genera_informe()
        {
            string informe = "";

            informe = "****************************************************************\r\n\r\n";

            informe += "********************* VERSIONES *********************\r\n\r\n";

            informe += "********** Versiones software: **********\r\n";
            informe += "* Versión de software PC/Windows: " + sLabelVersionSoftware + "\r\n";
            informe += "* Version de diseño lógico en FPGA: " + ((cadena_ver_fpga.Length != 0)? cadena_ver_fpga:"NO CONOCIDO") + "\r\n";
            informe += "* Versión Firmware de test DSP: " + ((cadena_ver_firmware.Length != 0)?cadena_ver_firmware:"NO CONOCIDO") + "\r\n\r\n";

            informe += "********** Versiones hardware: **********\r\n";
            informe += "* USB-SERIE VID: " + ((cadena_USB_UART_VID.Length!=0)? cadena_USB_UART_VID : "NO CONOCIDO") + "\r\n";
            informe += "* USB-SERIE PID: " + ((cadena_USB_UART_PID.Length != 0) ? cadena_USB_UART_PID : "NO CONOCIDO") + "\r\n";
            informe += "* USB-SERIE SN: " + ((cadena_USB_UART_SN.Length != 0) ? cadena_USB_UART_SN : "NO CONOCIDO") + "\r\n";
            informe += "* ID unico de la FPGA (DNA): " + ((cadena_id_dna.Length != 0) ? cadena_id_dna : "NO CONOCIDO") + "\r\n";
            informe += "* DSP PARTID = " + ((cadena_dsp_partid.Length != 0) ? cadena_dsp_partid : "NO CONOCIDO") + "\r\n";
            informe += "* DSP CLASSID = " + ((cadena_dsp_classid.Length != 0) ? cadena_dsp_classid : "NO CONOCIDO") + "\r\n";
            informe += "* DSP REVID = " + ((cadena_dsp_revid.Length != 0) ? cadena_dsp_revid : "NO CONOCIDO") + "\r\n\r\n";

            informe += "******************* RESULTADOS *******************\r\n\r\n";

            informe += "********** Resultado global: **********\r\n";
            if ((leyenda_resultados_tests.Contains<int>(-1)) && (leyenda_resultados_tests.Contains<int>(0)))
                informe += "* Existen tests no ejecutados, existen tests con indicacion de fallos\r\n";
            else if (leyenda_resultados_tests.Contains<int>(-1))
                informe += "* Existen tests no ejecutados\r\n";
            else if (leyenda_resultados_tests.Contains<int>(0))
                informe += "* Existen tests con indicacion de fallos\r\n";
            else
            {
                bool res_ok = true;

                foreach (int res in leyenda_resultados_tests)
                    if (res != 1) res_ok = false;

                if (res_ok)
                    informe += "* Todos los tests ejecutados y OK\r\n";
                else
                    informe += "* ERROR: resultados no coherentes\r\n";
            }

            informe += "\r\n";

            informe += "********** Tests de supervisores de tension/corriente/temperatura: **********\r\n";
            informe += "**** Sensor de temperatura 1: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_spv_ok_fallo_temp1 == 1) informe += "OK\r\n";
            else if(test_spv_ok_fallo_temp1 == 0) informe += "FALLO\r\n";
            else if (test_spv_ok_fallo_temp1 == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_spv_ok_fallo_temp1 != -1)
            {
                informe += "* Medida = " + test_spv_temp1_med.ToString(System.Globalization.CultureInfo.InvariantCulture) + "ºC\r\n";
                double auxmin = test_spv_temp1_ref - 1.0;
                double auxmax = test_spv_temp1_ref + 15.0;
                informe += "* Referencia = " + test_spv_temp1_ref.ToString(System.Globalization.CultureInfo.InvariantCulture) + 
                    "ºC. (Rango válido de medida: " + auxmin.ToString(System.Globalization.CultureInfo.InvariantCulture) + "ºC a " + 
                    auxmax.ToString(System.Globalization.CultureInfo.InvariantCulture) + "ºC)\r\n";
                //informe += "* Tolerancia = " + test_spv_temp1_tol.ToString() + "%\r\n";
            }
            informe += "**** Sensor de temperatura 2: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_spv_ok_fallo_temp2 == 1) informe += "OK\r\n";
            else if (test_spv_ok_fallo_temp2 == 0) informe += "FALLO\r\n";
            else if (test_spv_ok_fallo_temp2 == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_spv_ok_fallo_temp2 != -1)
            {
                informe += "* Medida = " + test_spv_temp2_med.ToString(System.Globalization.CultureInfo.InvariantCulture) + "ºC\r\n";
                double auxmin = test_spv_temp1_ref - 1.0;
                double auxmax = test_spv_temp1_ref + 15.0;
                informe += "* Referencia = " + test_spv_temp1_ref.ToString(System.Globalization.CultureInfo.InvariantCulture) + 
                    "ºC. (Rango válido de medida: " + auxmin.ToString(System.Globalization.CultureInfo.InvariantCulture) + "ºC a " +
                    auxmax.ToString(System.Globalization.CultureInfo.InvariantCulture) + "ºC)\r\n";
                //informe += "* Tolerancia = " + test_spv_temp2_tol.ToString() + "%\r\n";
            }
            informe += "**** Sensor de tension de alimentacion (Vin): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_spv_ok_fallo_vin == 1) informe += "OK\r\n";
            else if (test_spv_ok_fallo_vin == 0) informe += "FALLO\r\n";
            else if (test_spv_ok_fallo_vin == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_spv_ok_fallo_vin != -1)
            {
                informe += "* Medida = " + test_spv_vin_med.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V\r\n";
                informe += "* Referencia = " + test_spv_vin_ref.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V\r\n";
                informe += "* Tolerancia = " + test_spv_vin_tol.ToString(System.Globalization.CultureInfo.InvariantCulture) + "%\r\n";
            }
            informe += "**** Sensor de corriente de alimentacion (Iin): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_spv_ok_fallo_iin == 1) informe += "OK\r\n";
            else if (test_spv_ok_fallo_iin == 0) informe += "FALLO\r\n";
            else if (test_spv_ok_fallo_iin == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_spv_ok_fallo_iin != -1)
            {
                informe += "* Medida = " + test_spv_iin_med.ToString(System.Globalization.CultureInfo.InvariantCulture) + "A\r\n";
                informe += "* Referencia = " + test_spv_iin_ref.ToString(System.Globalization.CultureInfo.InvariantCulture) + "A\r\n";
                informe += "* Tolerancia = " + test_spv_iin_tol.ToString(System.Globalization.CultureInfo.InvariantCulture) + "%\r\n";
            }
            informe += "**** Sensor de tension de alimentacion a la placa DSP (Vdsp): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_spv_ok_fallo_vdsp == 1) informe += "OK\r\n";
            else if (test_spv_ok_fallo_vdsp == 0) informe += "FALLO\r\n";
            else if (test_spv_ok_fallo_vdsp == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_spv_ok_fallo_vdsp != -1)
            {
                informe += "* Medida = " + test_spv_vdsp_med.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V\r\n";
                informe += "* Referencia = " + test_spv_vdsp_ref.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V\r\n";
                informe += "* Tolerancia = " + test_spv_vdsp_tol.ToString(System.Globalization.CultureInfo.InvariantCulture) + "%\r\n";
            }
            informe += "**** Sensor de tension de alimentacion a transmisores fib optica (Vfo): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_spv_ok_fallo_vfo == 1) informe += "OK\r\n";
            else if (test_spv_ok_fallo_vfo == 0) informe += "FALLO\r\n";
            else if (test_spv_ok_fallo_vfo == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_spv_ok_fallo_vfo != -1)
            {
                informe += "* Medida = " + test_spv_vfo_med.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V\r\n";
                informe += "* Referencia = " + test_spv_vfo_ref.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V\r\n";
                informe += "* Tolerancia = " + test_spv_vfo_tol.ToString(System.Globalization.CultureInfo.InvariantCulture) + "%\r\n";
            }
            informe += "**** Sensor de tension de alimentacion a reles (Vreles): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_spv_ok_fallo_vreles == 1) informe += "OK\r\n";
            else if (test_spv_ok_fallo_vreles == 0) informe += "FALLO\r\n";
            else if (test_spv_ok_fallo_vreles == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_spv_ok_fallo_vreles != -1)
            {
                informe += "* Medida = " + test_spv_vreles_med.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V\r\n";
                informe += "* Referencia = " + test_spv_vreles_ref.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V\r\n";
                informe += "* Tolerancia = " + test_spv_vreles_tol.ToString(System.Globalization.CultureInfo.InvariantCulture) + "%\r\n";
            }
            informe += "**** Sensor de tension de alimentacion de 3.3V (V_3.3V): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_spv_ok_fallo_v3_3v == 1) informe += "OK\r\n";
            else if (test_spv_ok_fallo_v3_3v == 0) informe += "FALLO\r\n";
            else if (test_spv_ok_fallo_v3_3v == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_spv_ok_fallo_v3_3v != -1)
            {
                informe += "* Medida = " + test_spv_v3_3v_med.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V\r\n";
                informe += "* Referencia = " + test_spv_v3_3v_ref.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V\r\n";
                informe += "* Tolerancia = " + test_spv_v3_3v_tol.ToString(System.Globalization.CultureInfo.InvariantCulture) + "%\r\n";
            }
            informe += "**** Sensor de tension de bateria (Vbat): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_spv_ok_fallo_vbat == 1) informe += "OK\r\n";
            else if (test_spv_ok_fallo_vbat == 0) informe += "FALLO\r\n";
            else if (test_spv_ok_fallo_vbat == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_spv_ok_fallo_vbat != -1)
            {
                informe += "* Medida = " + test_spv_vbat_med.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V\r\n";
                informe += "* Referencia = " + test_spv_vbat_ref.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V\r\n";
                informe += "* Tolerancia = " + test_spv_vbat_tol.ToString(System.Globalization.CultureInfo.InvariantCulture) + "%\r\n";
            }

            informe += "\r\n";

            informe += "********** Tests de puerto de comunicacion RS-422 (CON12): **********\r\n";
            informe += "**** Test de transmision en bucle - RX deshabilitada - TX deshabilitada: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_rs422_loop_rxdis_txdis_ok_fallo == 1) informe += "OK\r\n";
            else if (test_rs422_loop_rxdis_txdis_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_rs422_loop_rxdis_txdis_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";
            informe += "**** Test de transmision en bucle - RX deshabilitada - TX habilitada: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_rs422_loop_rxdis_txena_ok_fallo == 1) informe += "OK\r\n";
            else if (test_rs422_loop_rxdis_txena_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_rs422_loop_rxdis_txena_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";
            informe += "**** Test de transmision en bucle - RX habilitada - TX deshabilitada: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_rs422_loop_rxena_txdis_ok_fallo == 1) informe += "OK\r\n";
            else if (test_rs422_loop_rxena_txdis_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_rs422_loop_rxena_txdis_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";
            informe += "**** Test de transmision en bucle - RX habilitada - TX habilitada: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_rs422_loop_rxena_txena_ok_fallo == 1) informe += "OK\r\n";
            else if (test_rs422_loop_rxena_txena_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_rs422_loop_rxena_txena_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";

            informe += "\r\n";

            informe += "********** Tests de puertos de comunicacion CAN (CON7 (A) y CON13 (B)): **********\r\n";
            informe += "**** Test de nivel recesivo/dominante CAN A en bucle: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_cana_loop_ok_fallo == 1) informe += "OK\r\n";
            else if (test_cana_loop_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_cana_loop_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";
            informe += "**** Test de nivel recesivo/dominante CAN B en bucle: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_canb_loop_ok_fallo == 1) informe += "OK\r\n";
            else if (test_canb_loop_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_canb_loop_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";
            informe += "**** Test de nivel recesivo/dominante CAN A y CAN B interconectados: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_cana_canb_ok_fallo == 1) informe += "OK\r\n";
            else if (test_cana_canb_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_cana_canb_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";

            informe += "\r\n";

            informe += "********** Test de escritura/lectura de banco de memoria SRAM: **********\r\n";
            informe += "* RESULTADO: ";
            if (leyenda_resultados_tests[(int)TipoTest.TEST_SRAM] == 1) informe += "OK\r\n";
            else if (leyenda_resultados_tests[(int)TipoTest.TEST_SRAM] == 0) informe += "FALLO\r\n";
            else if (leyenda_resultados_tests[(int)TipoTest.TEST_SRAM] == -1) informe += "TEST NO EJECUTADO\r\n";
            if (leyenda_resultados_tests[(int)TipoTest.TEST_SRAM] != -1)
            {
                double tiempo = 100.0*((double)test_sram_tiempo_test);
                informe += "* Tiempo de test = " + tiempo.ToString(System.Globalization.CultureInfo.InvariantCulture) + " milisengundos\r\n";
            }

            informe += "\r\n";

            informe += "********** Test de escritura/lectura/protección/desprotección escritura de banco de memoria FRAM: **********\r\n";
            informe += "**** Test de escritura 1: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_fram_escritura1_ok_fallo == 1) informe += "OK\r\n";
            else if (test_fram_escritura1_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_fram_escritura1_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_fram_escritura1_ok_fallo != -1)
            {
                double tiempo = 100.0 * ((double)test_fram_tiempo_test_escritura1);
                informe += "* Tiempo de test = " + tiempo.ToString(System.Globalization.CultureInfo.InvariantCulture) + " milisengundos\r\n";
            }
            informe += "**** Test de escritura 2: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_fram_escritura2_ok_fallo == 1) informe += "OK\r\n";
            else if (test_fram_escritura2_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_fram_escritura2_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_fram_escritura2_ok_fallo != -1)
            {
                double tiempo = 100.0 * ((double)test_fram_tiempo_test_escritura2);
                informe += "* Tiempo de test = " + tiempo.ToString(System.Globalization.CultureInfo.InvariantCulture) + " milisengundos\r\n";
            }
            informe += "**** Test de protección de escritura: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_fram_bloqueo_ok_fallo == 1) informe += "OK\r\n";
            else if (test_fram_bloqueo_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_fram_bloqueo_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_fram_bloqueo_ok_fallo != -1)
            {
                double tiempo = 100.0 * ((double)test_fram_tiempo_test_bloqueo);
                informe += "* Tiempo de test = " + tiempo.ToString(System.Globalization.CultureInfo.InvariantCulture) + " milisengundos\r\n";
            }
            informe += "**** Test de desprotección de escritura: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_fram_desbloqueo_ok_fallo == 1) informe += "OK\r\n";
            else if (test_fram_desbloqueo_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_fram_desbloqueo_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_fram_desbloqueo_ok_fallo != -1)
            {
                double tiempo = 100.0 * ((double)test_fram_tiempo_test_desbloqueo);
                informe += "* Tiempo de test = " + tiempo.ToString(System.Globalization.CultureInfo.InvariantCulture) + " milisengundos\r\n";
            }

            informe += "\r\n";

            informe += "********** Test de sensado de termistores NTC: **********\r\n";
            informe += "**** Medida de resistencia NTC 1 (CON27): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_ntcs_ok_fallo[0] == 1) informe += "OK\r\n";
            else if (test_ntcs_ok_fallo[0] == 0) informe += "FALLO\r\n";
            else if (test_ntcs_ok_fallo[0] == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_ntcs_ok_fallo[0] != -1)
            {
                informe += "* Medida = " + test_ntcs_medidas[0].ToString(System.Globalization.CultureInfo.InvariantCulture) + " Ohmios\r\n";
                informe += "* Referencia = " + test_ntcs_referencias[0].ToString(System.Globalization.CultureInfo.InvariantCulture) + " Ohmios\r\n";
                informe += "* Tolerancia = " + test_ntcs_tolerancias[0].ToString(System.Globalization.CultureInfo.InvariantCulture) + "%\r\n";
            }
            informe += "**** Medida de resistencia NTC 2 (CON28): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_ntcs_ok_fallo[1] == 1) informe += "OK\r\n";
            else if (test_ntcs_ok_fallo[1] == 0) informe += "FALLO\r\n";
            else if (test_ntcs_ok_fallo[1] == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_ntcs_ok_fallo[1] != -1)
            {
                informe += "* Medida = " + test_ntcs_medidas[1].ToString(System.Globalization.CultureInfo.InvariantCulture) + " Ohmios\r\n";
                informe += "* Referencia = " + test_ntcs_referencias[1].ToString(System.Globalization.CultureInfo.InvariantCulture) + " Ohmios\r\n";
                informe += "* Tolerancia = " + test_ntcs_tolerancias[1].ToString(System.Globalization.CultureInfo.InvariantCulture) + "%\r\n";
            }
            informe += "**** Medida de resistencia NTC 3 (CON29): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_ntcs_ok_fallo[2] == 1) informe += "OK\r\n";
            else if (test_ntcs_ok_fallo[2] == 0) informe += "FALLO\r\n";
            else if (test_ntcs_ok_fallo[2] == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_ntcs_ok_fallo[2] != -1)
            {
                informe += "* Medida = " + test_ntcs_medidas[2].ToString(System.Globalization.CultureInfo.InvariantCulture) + " Ohmios\r\n";
                informe += "* Referencia = " + test_ntcs_referencias[2].ToString(System.Globalization.CultureInfo.InvariantCulture) + " Ohmios\r\n";
                informe += "* Tolerancia = " + test_ntcs_tolerancias[2].ToString(System.Globalization.CultureInfo.InvariantCulture) + "%\r\n";
            }
            informe += "**** Medida de resistencia NTC 4 (CON30): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_ntcs_ok_fallo[3] == 1) informe += "OK\r\n";
            else if (test_ntcs_ok_fallo[3] == 0) informe += "FALLO\r\n";
            else if (test_ntcs_ok_fallo[3] == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_ntcs_ok_fallo[3] != -1)
            {
                informe += "* Medida = " + test_ntcs_medidas[3].ToString(System.Globalization.CultureInfo.InvariantCulture) + " Ohmios\r\n";
                informe += "* Referencia = " + test_ntcs_referencias[3].ToString(System.Globalization.CultureInfo.InvariantCulture) + " Ohmios\r\n";
                informe += "* Tolerancia = " + test_ntcs_tolerancias[3].ToString(System.Globalization.CultureInfo.InvariantCulture) + "%\r\n";
            }
            informe += "**** Medida de resistencia NTC 5 (CON31): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_ntcs_ok_fallo[4] == 1) informe += "OK\r\n";
            else if (test_ntcs_ok_fallo[4] == 0) informe += "FALLO\r\n";
            else if (test_ntcs_ok_fallo[4] == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_ntcs_ok_fallo[4] != -1)
            {
                informe += "* Medida = " + test_ntcs_medidas[4].ToString(System.Globalization.CultureInfo.InvariantCulture) + " Ohmios\r\n";
                informe += "* Referencia = " + test_ntcs_referencias[4].ToString(System.Globalization.CultureInfo.InvariantCulture) + " Ohmios\r\n";
                informe += "* Tolerancia = " + test_ntcs_tolerancias[4].ToString(System.Globalization.CultureInfo.InvariantCulture) + "%\r\n";
            }

            informe += "\r\n";

            informe += "********** Test interfaces para control de velocidad de turbinas: **********\r\n";
            informe += "**** Test de control de turbina 1 (CON46): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_ctrlturbina1_ok_fallo == 1) informe += "OK\r\n";
            else if (test_ctrlturbina1_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_ctrlturbina1_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_ctrlturbina1_pulsos_sec_0 != -1)
            {
                informe += "* Medida de pulsos/s con duty cycle 0% : " + test_ctrlturbina1_pulsos_sec_0.ToString(System.Globalization.CultureInfo.InvariantCulture) + " pulsos/segundo\r\n";
            }
            if (test_ctrlturbina1_pulsos_sec_50 != -1)
            {
                informe += "* Medida de pulsos/s con duty cycle 50% : " + test_ctrlturbina1_pulsos_sec_50.ToString(System.Globalization.CultureInfo.InvariantCulture) + " pulsos/segundo\r\n";
            }
            informe += "**** Test de control de turbina 2 (CON48): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_ctrlturbina2_ok_fallo == 1) informe += "OK\r\n";
            else if (test_ctrlturbina2_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_ctrlturbina2_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_ctrlturbina2_pulsos_sec_0 != -1)
            {
                informe += "* Medida de pulsos/s con duty cycle 0% : " + test_ctrlturbina2_pulsos_sec_0.ToString(System.Globalization.CultureInfo.InvariantCulture) + " pulsos/segundo\r\n";
            }
            if (test_ctrlturbina2_pulsos_sec_50 != -1)
            {
                informe += "* Medida de pulsos/s con duty cycle 50% : " + test_ctrlturbina2_pulsos_sec_50.ToString(System.Globalization.CultureInfo.InvariantCulture) + " pulsos/segundo\r\n";
            }
            informe += "**** Test de control de turbina 3 (CON49): ****\r\n";
            informe += "* RESULTADO: ";
            if (test_ctrlturbina3_ok_fallo == 1) informe += "OK\r\n";
            else if (test_ctrlturbina3_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_ctrlturbina3_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";
            if (test_ctrlturbina3_pulsos_sec_0 != -1)
            {
                informe += "* Medida de pulsos/s con duty cycle 0% : " + test_ctrlturbina3_pulsos_sec_0.ToString(System.Globalization.CultureInfo.InvariantCulture) + " pulsos/segundo\r\n";
            }
            if (test_ctrlturbina3_pulsos_sec_50 != -1)
            {
                informe += "* Medida de pulsos/s con duty cycle 50% : " + test_ctrlturbina3_pulsos_sec_50.ToString(System.Globalization.CultureInfo.InvariantCulture) + " pulsos/segundo\r\n";
            }

            informe += "\r\n";

            informe += "********** Tests de reles, salidas optoacopladas y entradas digitales: **********\r\n";
            informe += "**** Test de bucle rele-entrada digital: ****\r\n";
            for(int i=0; i<16; i++)
            {
                informe += "* RESULTADO bucle rele" + i + "-entrada" + i + ": ";
                if (test_io_reles_entradas_ok_fallo[i] == 1) informe += "OK\r\n";
                else if (test_io_reles_entradas_ok_fallo[i] == 0) informe += "FALLO\r\n";
                else if (test_io_reles_entradas_ok_fallo[i] == -1) informe += "TEST NO EJECUTADO\r\n";
            }
            informe += "**** Test de bucle salida optoacoplada-entrada digital: ****\r\n";
            for (int i = 0; i < 5; i++)
            {
                informe += "* RESULTADO bucle salida optoacoplada" + i + "-entrada" + i + ": ";
                if (test_io_leds_entradas_ok_fallo[i] == 1) informe += "OK\r\n";
                else if (test_io_leds_entradas_ok_fallo[i] == 0) informe += "FALLO\r\n";
                else if (test_io_leds_entradas_ok_fallo[i] == -1) informe += "TEST NO EJECUTADO\r\n";
            }

            informe += "\r\n";

            informe += "********** Tests de transmisores y receptores ópticos: **********\r\n";
            informe += "**** Tests de bucles transmisores de disparos-receptores de error de driver: ****\r\n";
            informe += "* RESULTADO bucle XT1-XR1 (Disparo/Error TOP R): ";
            if (test_fo_disp_error_ok_fallo[0] == 1) informe += "OK\r\n";
            else if (test_fo_disp_error_ok_fallo[0] == 0) informe += "FALLO\r\n";
            else if (test_fo_disp_error_ok_fallo[0] == -1) informe += "TEST NO EJECUTADO\r\n";
            informe += "* RESULTADO bucle XT3-XR3 (Disparo/Error BOT R): ";
            if (test_fo_disp_error_ok_fallo[3] == 1) informe += "OK\r\n";
            else if (test_fo_disp_error_ok_fallo[3] == 0) informe += "FALLO\r\n";
            else if (test_fo_disp_error_ok_fallo[3] == -1) informe += "TEST NO EJECUTADO\r\n";
            informe += "* RESULTADO bucle XT5-XR6 (Disparo/Error TOP S): ";
            if (test_fo_disp_error_ok_fallo[1] == 1) informe += "OK\r\n";
            else if (test_fo_disp_error_ok_fallo[1] == 0) informe += "FALLO\r\n";
            else if (test_fo_disp_error_ok_fallo[1] == -1) informe += "TEST NO EJECUTADO\r\n";
            informe += "* RESULTADO bucle XT7-XR8 (Disparo/Error BOT S): ";
            if (test_fo_disp_error_ok_fallo[4] == 1) informe += "OK\r\n";
            else if (test_fo_disp_error_ok_fallo[4] == 0) informe += "FALLO\r\n";
            else if (test_fo_disp_error_ok_fallo[4] == -1) informe += "TEST NO EJECUTADO\r\n";
            informe += "* RESULTADO bucle XT9-XR11 (Disparo/Error TOP T): ";
            if (test_fo_disp_error_ok_fallo[2] == 1) informe += "OK\r\n";
            else if (test_fo_disp_error_ok_fallo[2] == 0) informe += "FALLO\r\n";
            else if (test_fo_disp_error_ok_fallo[2] == -1) informe += "TEST NO EJECUTADO\r\n";
            informe += "* RESULTADO bucle XT11-XR13 (Disparo/Error BOT T): ";
            if (test_fo_disp_error_ok_fallo[5] == 1) informe += "OK\r\n";
            else if (test_fo_disp_error_ok_fallo[5] == 0) informe += "FALLO\r\n";
            else if (test_fo_disp_error_ok_fallo[5] == -1) informe += "TEST NO EJECUTADO\r\n";

            informe += "**** Tests de bucles transmisores de disparos-receptores de error de sobretemperatura/sincronismo: ****\r\n";
            informe += "* RESULTADO bucle XT1-XR5 (Disparo TOP R/Error Sobretemperatura R): ";
            if (test_fo_disp_ovt_ok_fallo[0] == 1) informe += "OK\r\n";
            else if (test_fo_disp_ovt_ok_fallo[0] == 0) informe += "FALLO\r\n";
            else if (test_fo_disp_ovt_ok_fallo[0] == -1) informe += "TEST NO EJECUTADO\r\n";
            informe += "* RESULTADO bucle XT3-XR10 (Disparo BOT R/Error Sobretemperatura S): ";
            if (test_fo_disp_ovt_ok_fallo[1] == 1) informe += "OK\r\n";
            else if (test_fo_disp_ovt_ok_fallo[1] == 0) informe += "FALLO\r\n";
            else if (test_fo_disp_ovt_ok_fallo[1] == -1) informe += "TEST NO EJECUTADO\r\n";
            informe += "* RESULTADO bucle XT5-XR15 (Disparo TOP S/Error Sobretemperatura T): ";
            if (test_fo_disp_ovt_ok_fallo[2] == 1) informe += "OK\r\n";
            else if (test_fo_disp_ovt_ok_fallo[2] == 0) informe += "FALLO\r\n";
            else if (test_fo_disp_ovt_ok_fallo[2] == -1) informe += "TEST NO EJECUTADO\r\n";
            informe += "* RESULTADO bucle XT7-XR16 (Disparo BOT S/Receptor de sincronismo): ";
            if (test_fo_bot_s_rxsynch_ok_fallo == 1) informe += "OK\r\n";
            else if (test_fo_bot_s_rxsynch_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_fo_bot_s_rxsynch_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";

            informe += "\r\n";

            informe += "********** Tests de canales ADC del DSP: **********\r\n";
            
            for (int i = 0; i < 16; i++ )
            {
                informe += "**** ";
                informe += comboBoxCanalADCseleccionado.Items[i];
                informe += " ****\r\n";

                informe += "* RESULTADO: ";
                int j = canalesADCcombo[i];
                if (test_adcs_ok_fallo[j] == 1) informe += "OK\r\n";
                else if (test_adcs_ok_fallo[j] == 0) informe += "FALLO\r\n";
                else if (test_adcs_ok_fallo[j] == -1) informe += "TEST NO EJECUTADO\r\n";
                if (test_adcs_ok_fallo[j] != -1)
                {
                    informe += "* Medida = " + test_adcs_medias[j].ToString(System.Globalization.CultureInfo.InvariantCulture) + dimension_ref_adc[j] + "\r\n";
                    informe += "* Referencia = " + test_adcs_referencias[j].ToString(System.Globalization.CultureInfo.InvariantCulture) + dimension_ref_adc[j] + "\r\n";
                    informe += "* Tolerancia = " + test_adcs_tolerancias[j].ToString(System.Globalization.CultureInfo.InvariantCulture) + "%\r\n";
                }
            }

            informe += "\r\n";

            informe += "********** Test del Real Time Clock (RTC): **********\r\n";
            informe += "**** Test de puesta en hora: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_rtc_puesta_en_hora_ok_fallo == 1) informe += "OK\r\n";
            else if (test_rtc_puesta_en_hora_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_rtc_puesta_en_hora_ok_fallo == -1) informe += "TEST NO EJECUTADO\r\n";
            
            informe += "**** Test de estado en hora: ****\r\n";
            informe += "* RESULTADO: ";
            if (test_rtc_en_hora_ok_fallo == 1) informe += "OK\r\n";
            else if (test_rtc_en_hora_ok_fallo == 0) informe += "FALLO\r\n";
            else if (test_rtc_en_hora_ok_fallo == -1) informe += "INDETERMINADO (TIEMPO EN HORA INFERIOR A 5 SEGUNDOS)\r\n";
            informe += "* Tiempo en hora = " + test_rtc_duracion_en_hora + " segundos\r\n";

            informe += "\r\n";

            informe += "\r\n*****************************************************************";

            return informe;
        }

        private void buttonInforme_Click(object sender, EventArgs e)
        {
            string informe = genera_informe();
            FormInforme fi = new FormInforme(informe, leyenda_resultados_tests, cadena_USB_UART_VID + cadena_USB_UART_PID + "_" + cadena_USB_UART_SN + "_" + cadena_id_dna);
            fi.ShowDialog();
            fi.Close();
        }

        private void checkBoxEnableTX_RS422_CheckedChanged(object sender, EventArgs e)
        {
            if (test != TipoTest.TEST_RS422)
            {
                CheckBox cb = (CheckBox)sender;

                if (cb.Checked)
                    enabletx_rs422 = 1;
                else
                    enabletx_rs422 = 0;
            }

        }

        private void checkBoxEnableRX_RS422_CheckedChanged(object sender, EventArgs e)
        {
            if (test != TipoTest.TEST_RS422)
            {
                CheckBox cb = (CheckBox)sender;

                if (cb.Checked)
                    enablerx_rs422 = 1;
                else
                    enablerx_rs422 = 0;
            }

        }

        private void textBoxRS422_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (test != TipoTest.TEST_RS422)
            {
                e.Handled = true;
                buffer_tx_rs422 += e.KeyChar;
            }

            //if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            //{
            //e.Handled = true;
            //}

            // only allow one decimal point
            //if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            //{
            //e.Handled = true;
            //}
        }

        private void checkBoxCANATX_CheckedChanged(object sender, EventArgs e)
        {
            if (test != TipoTest.TEST_CAN)
            {
                if (checkBoxCANATX.Checked)
                {
                    canatx = 1;
                }
                else
                {
                    canatx = 0;
                }
            }
        }

        private void checkBoxCANBTX_CheckedChanged(object sender, EventArgs e)
        {
            if (test != TipoTest.TEST_CAN)
            {
                if (checkBoxCANBTX.Checked)
                {
                    canbtx = 1;
                }
                else
                {
                    canbtx = 0;
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if ((test == TipoTest.NO_TEST) && serialPort1.IsOpen)
            {
                leyenda_resultados_tests[(int)TipoTest.TEST_CAN] = -1;
                actualiza_marcas_leyenda_resultados_tests();
                test = TipoTest.TEST_CAN;
                contador_test = 0;
                timer1.Enabled = true;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if ((test == TipoTest.NO_TEST) && serialPort1.IsOpen)
            {
                leyenda_resultados_tests[(int)TipoTest.TEST_SRAM] = -1;
                actualiza_marcas_leyenda_resultados_tests();
                test = TipoTest.TEST_SRAM;
                contador_test = 0;
                timer1.Enabled = true;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if ((test == TipoTest.NO_TEST) && serialPort1.IsOpen)
            {
                leyenda_resultados_tests[(int)TipoTest.TEST_FRAM] = -1;
                actualiza_marcas_leyenda_resultados_tests();
                test = TipoTest.TEST_FRAM;
                contador_test = 0;
                timer1.Enabled = true;
            }
        }

        private void reset_informes_tests(bool borra_versiones)
        {
            // inicializa resultados de tests para informes
            // adcs
            for (int i = 0; i < 16; i++) test_adcs_ok_fallo[i] = -1;
            // supervisores
            test_spv_ok_fallo_temp1 = -1;
            test_spv_ok_fallo_temp2 = -1;
            test_spv_ok_fallo_vin = -1;
            test_spv_ok_fallo_iin = -1;
            test_spv_ok_fallo_vdsp = -1;
            test_spv_ok_fallo_vfo = -1;
            test_spv_ok_fallo_vreles = -1;
            test_spv_ok_fallo_v3_3v = -1;
            test_spv_ok_fallo_vbat = -1;
            // RS-422
            test_rs422_loop_rxdis_txdis_ok_fallo = -1;
            test_rs422_loop_rxdis_txena_ok_fallo = -1;
            test_rs422_loop_rxena_txdis_ok_fallo = -1;
            test_rs422_loop_rxena_txena_ok_fallo = -1;
            // CAN
            test_cana_loop_ok_fallo = -1;
            test_canb_loop_ok_fallo = -1;
            test_cana_canb_ok_fallo = -1;
            // SRAM
            test_sram_tiempo_test = -1;
            // FRAM
            test_fram_escritura1_ok_fallo = -1;
            test_fram_escritura2_ok_fallo = -1;
            test_fram_bloqueo_ok_fallo = -1;
            test_fram_desbloqueo_ok_fallo = -1;
            test_fram_tiempo_test_escritura1 = -1;
            test_fram_tiempo_test_escritura2 = -1;
            test_fram_tiempo_test_bloqueo = -1;
            test_fram_tiempo_test_desbloqueo = -1;
            // control velocidad turbinas
            test_ctrlturbina1_ok_fallo = -1;
            test_ctrlturbina1_pulsos_sec_0 = -1;
            test_ctrlturbina1_pulsos_sec_50 = -1;
            test_ctrlturbina2_ok_fallo = -1;
            test_ctrlturbina2_pulsos_sec_0 = -1;
            test_ctrlturbina2_pulsos_sec_50 = -1;
            test_ctrlturbina3_ok_fallo = -1;
            test_ctrlturbina3_pulsos_sec_0 = -1;
            test_ctrlturbina3_pulsos_sec_50 = -1;
            // Entradas / salidas digitales
            for (int i = 0; i < 16; i++)
            {
                test_io_reles_entradas_ok_fallo[i] = -1;
            }
            for (int i = 0; i < 5; i++)
            {
                test_io_leds_entradas_ok_fallo[i] = -1;
            }
            // Transmisores / receptores opticos
            for (int i = 0; i < 6; i++)
            {
                test_fo_disp_error_ok_fallo[i] = -1;
            }
            for (int i = 0; i < 3; i++)
            {
                test_fo_disp_ovt_ok_fallo[i] = -1;
            }
            test_fo_bot_s_rxsynch_ok_fallo = -1;
            // medida de NTCs
            for(int i=0; i<5; i++)
            {
                test_ntcs_ok_fallo[i] = -1;
            }
            // rtc
            estado_en_hora = false;
            test_rtc_puesta_en_hora_ok_fallo = -1;
            test_rtc_en_hora_ok_fallo = 0;
            test_rtc_duracion_en_hora = 0;

            for (int i = 0; i < marcasADCs.Length; i++)
            {
                marcasADCs[i].Visible = false;
            }
            for (int i = 0; i < marcasSupervisores.Length; i++)
            {
                marcasSupervisores[i].Visible = false;
            }
            for (int i = 0; i < marcasNTCs.Length; i++)
            {
                marcasNTCs[i].Visible = false;
            }
            for (int i = 0; i < marcasReles.Length; i++)
            {
                marcasReles[i].Visible = false;
            }
            for (int i = 0; i < marcasInputs.Length; i++)
            {
                marcasInputs[i].Visible = false;
            }
            for (int i = 0; i < marcasLEDs.Length; i++)
            {
                marcasLEDs[i].Visible = false;
            }
            for (int i = 0; i < marcasDisparos.Length; i++)
            {
                marcasDisparos[i].Visible = false;
            }
            for (int i = 0; i < marcasErroresOpticos.Length; i++)
            {
                marcasErroresOpticos[i].Visible = false;
            }
            for (int i = 0; i < marcasTurbinas.Length; i++)
            {
                marcasTurbinas[i].Visible = false;
            }
            pictureBoxSynchSD.Visible = false;
            pictureBoxRS422.Visible = false;
            pictureBoxCAN.Visible = false;
            pictureBoxSRAM.Visible = false;
            pictureBoxFRAMEscritura1.Visible = false;
            pictureBoxFRAMEscritura2.Visible = false;
            pictureBoxFRAMBloqueo.Visible = false;
            pictureBoxFRAMDesbloqueo.Visible = false;
            pictureBoxPuestaEnHoraRTC.Visible = false;
            pictureBoxEnHoraRTC.Visible = false;

            if (borra_versiones)
            {
                //cadena_USB_UART_ID = "";
                //cadena_USB_UART_SN = "";
                cadena_versiones = "";
                cadena_ver_fpga = "";
                cadena_ver_firmware = "";
                cadena_id_dna = "";
                cadena_dsp_partid = "";
                cadena_dsp_classid = "";
                cadena_dsp_revid = "";
            }

            // leyenda de resultados
            for (int i = 0; i < leyenda_resultados_tests.Length; i++)
            {
                if (i != ((int)TipoTest.TEST_LEER_VERSIONES))
                    leyenda_resultados_tests[i] = -1;
            }
            //if(!flag_cambia_versiones) leyenda_resultados_tests[(int)TipoTest.TEST_LEER_VERSIONES] = -1;
            flag_cambia_versiones = false;

            actualiza_marcas_leyenda_resultados_tests();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (test != TipoTest.NO_TEST)
            {
                timer1.Enabled = false;
                DialogResult res = MessageBox.Show("¿Desea cancelar el test actual?", "Reset informe de resultados", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.No)
                {
                    timer1.Enabled = true;
                    return;
                }
                else test = TipoTest.NO_TEST;
            }

            reset_informes_tests(false);
        }

        private void numericUpDownTurbina1_ValueChanged(object sender, EventArgs e)
        {
            if (test != TipoTest.TEST_TURBINAS)
            {
                dutyturbina1 = (int)numericUpDownTurbina1.Value;
            }
        }

        private void inicializa_ref_adc_por_defecto(int canal)
        {
            double[] ref_adc_por_defecto = { 4.0, 4.0, 20.0, 4.0, 4.0, 4.0, 20.0, 4.0,
                                             20.0, 4.0, 20.0, 4.0, 20, 4.0, 20.0, 4.0};

            double[] tol_adc_por_defecto = { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0,
                                             1.0, 1.0, 0.5, 1.0, 1.0, 1.0, 0.5, 1.0};

            textBoxRefInADC.Text = ref_adc_por_defecto[canal].ToString(System.Globalization.CultureInfo.InvariantCulture);
            labelDimRefADC.Text = dimension_ref_adc[canal];
            textBoxToleranciaADC.Text = tol_adc_por_defecto[canal].ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private void radioButtonADC_CheckedChanged(object sender, EventArgs e)
        {
            //int[] indiceComboBoxCanalesADC = { 3, 7, 8, 12, 2, 6, 9, 13, 1, 5, 10, 14, 0, 4, 11, 15 };

            RadioButton rb = (RadioButton)sender;

            if (rb != radioButtonADC_seleccionado)
            {
                radioButtonADC_seleccionado.Checked = false;
                radioButtonADC_seleccionado = rb;

                string tag = rb.Tag.ToString();
                int canal = Int32.Parse(tag);
                int indiceComboBoxCanalesADC = Array.IndexOf(canalesADCcombo, canal);
                comboBoxCanalADCseleccionado.SelectedIndex = indiceComboBoxCanalesADC;
            }
        }

        private void comboBoxCanalADCseleccionado_SelectedIndexChanged(object sender, EventArgs e)
        {
            RadioButton[] radioButtonsCanalesADC = { radioButtonADCA0, radioButtonADCB0, radioButtonADCA1, radioButtonADCB1,
                                                     radioButtonADCA2, radioButtonADCB2, radioButtonADCA3, radioButtonADCB3,
                                                     radioButtonADCA4, radioButtonADCB4, radioButtonADCA5, radioButtonADCB5,
                                                     radioButtonADCA6, radioButtonADCB6, radioButtonADCA7, radioButtonADCB7};

            GroupBox[] groupBoxesCanalesADC = { groupBoxADC0, groupBoxADC1, groupBoxADC2, groupBoxADC3,
                                                groupBoxADC4, groupBoxADC5, groupBoxADC6, groupBoxADC7,
                                                groupBoxADC8, groupBoxADC9, groupBoxADC10, groupBoxADC11,
                                                groupBoxADC12, groupBoxADC13, groupBoxADC14, groupBoxADC15};

            ComboBox cb = (ComboBox)sender;
            int canal_anterior = canal_adc_seleccionado;
            canal_adc_seleccionado = canalesADCcombo[cb.SelectedIndex];
            radioButtonsCanalesADC[canal_adc_seleccionado].Checked = true;
            groupBoxesCanalesADC[canal_anterior].BackColor = groupBoxesCanalesADC[canal_adc_seleccionado].BackColor;
            groupBoxesCanalesADC[canal_adc_seleccionado].BackColor = SystemColors.Info;
            inicializa_ref_adc_por_defecto(canal_adc_seleccionado);
        }

        private void testADC()
        {
            double referencia, tolerancia, media;

            test_adcs_ok_fallo[canal_adc_seleccionado] = -1;
            marcasADCs[canal_adc_seleccionado].Visible = false;

            referencia = double.Parse(textBoxRefInADC.Text, System.Globalization.CultureInfo.InvariantCulture);
            tolerancia = double.Parse(textBoxToleranciaADC.Text, System.Globalization.CultureInfo.InvariantCulture);
            media = medias_adcs_vi[canal_adc_seleccionado];

            test_adcs_referencias[canal_adc_seleccionado] = referencia;
            test_adcs_tolerancias[canal_adc_seleccionado] = tolerancia;
            test_adcs_medias[canal_adc_seleccionado] = media;

            if ((media >= (referencia * (1 - tolerancia / 100.0))) && (media <= (referencia * (1 + tolerancia / 100.0))))
            {
                test_adcs_ok_fallo[canal_adc_seleccionado] = 1;
                marcasADCs[canal_adc_seleccionado].Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
            }
            else
            {
                test_adcs_ok_fallo[canal_adc_seleccionado] = 0;
                marcasADCs[canal_adc_seleccionado].Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
            }

            marcasADCs[canal_adc_seleccionado].Visible = true;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen) return;

            if ((textBoxRefInADC.Text.Length == 0) || (textBoxToleranciaADC.Text.Length == 0))
            {
                MessageBox.Show("Este test compara las medidas de canales ADCs (tension o corriente)" +
                    " con valores definidos como referecia. Antes de ejecutar el test deben" +
                    " definirse los valores de referencia.", "Test de canales ADC");
                return;
            }

            leyenda_resultados_tests[(int)TipoTest.TEST_ADCS] = -1;
            actualiza_marcas_leyenda_resultados_tests();

            testADC();

            for (int i = 0, contador_oks = 0; i < 16; i++)
            {
                if (test_adcs_ok_fallo[i] == 0)
                {
                    leyenda_resultados_tests[(int)TipoTest.TEST_ADCS] = 0;
                    break;
                }

                if (test_adcs_ok_fallo[i] == 1) contador_oks++;

                if ((i == 15) && (contador_oks == 16))
                    leyenda_resultados_tests[(int)TipoTest.TEST_ADCS] = 1;
            }

            actualiza_marcas_leyenda_resultados_tests();
        }

        private void numericUpDownTurbina2_ValueChanged(object sender, EventArgs e)
        {
            if (test != TipoTest.TEST_TURBINAS)
            {
                dutyturbina2 = (int)numericUpDownTurbina2.Value;
            }
        }

        private void lee_pid_vid_serial_usb_uart(string nombreCOM)
        {
            UInt32 ftdiDeviceCount = 0;
            FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;
            string s;
            string cadena_USB_UART_VID_anterior = cadena_USB_UART_VID;
            string cadena_USB_UART_PID_anterior = cadena_USB_UART_PID;
            string cadena_USB_UART_SN_anterior = cadena_USB_UART_SN;

            cadena_USB_UART_VID = "";
            cadena_USB_UART_PID = "";
            cadena_USB_UART_SN = "";

            // Create new instance of the FTDI device class
            FTDI myFtdiDevice = new FTDI();

            // Determine the number of FTDI devices connected to the machine
            ftStatus = myFtdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);
            // Check status
            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                //Console.WriteLine("Number of FTDI devices: " + ftdiDeviceCount.ToString());
                //Console.WriteLine("");
            }
            else
            {
                // Wait for a key press
                //Console.WriteLine("Failed to get number of devices (error " + ftStatus.ToString() + ")");
                //Console.ReadKey();
                return;
            }

            if (ftdiDeviceCount == 0)
            {
                // Wait for a key press
                //Console.WriteLine("Failed to get number of devices (error " + ftStatus.ToString() + ")");
                //Console.ReadKey();
                return;
            }

            // Allocate storage for device info list
            //FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];
            // Populate our device list
            //ftStatus = myFtdiDevice.GetDeviceList(ftdiDeviceList);

            for (uint i = 0; i < ftdiDeviceCount; i++)
            {
                myFtdiDevice.OpenByIndex(i);
                myFtdiDevice.GetCOMPort(out s);
                uint id = 0;


                if (s == nombreCOM)
                {
                    myFtdiDevice.GetDeviceID(ref id);
                    cadena_USB_UART_VID = string.Format("{0:x04}", (id>>16));//ftdiDeviceList[i].ID);
                    cadena_USB_UART_PID = string.Format("{0:x04}", (id & 0xffff));
                    //cadena_USB_UART_SN = ftdiDeviceList[i].SerialNumber.ToString();
                    myFtdiDevice.GetSerialNumber(out cadena_USB_UART_SN);

                    //myFtdiDevice.ResetDevice();
                    myFtdiDevice.Close();
                    
                    break;
                }

                myFtdiDevice.Close();
            }

            if ((cadena_USB_UART_PID_anterior != cadena_USB_UART_PID) || (cadena_USB_UART_SN_anterior != cadena_USB_UART_SN) || 
                ((cadena_USB_UART_VID_anterior != cadena_USB_UART_VID)))
            {
                reset_informes_tests(true);
            }

        }

        private void botonConectarDesconectar_Click(object sender, EventArgs e)
        {
            if (botonConectarDesconectar.Text == "Conectar")
            {
                string nombreCOM = comboBoxCOMs.GetItemText(comboBoxCOMs.SelectedItem);
                try
                {
                    lee_pid_vid_serial_usb_uart(nombreCOM);

                    serialPort1.PortName = nombreCOM;
                    serialPort1.Open();

                    buffer_tx += "ping\r";
                    contador_comandos++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error apertura USB-Serie");
                    return;
                }
                botonConectarDesconectar.Text = "Desconectar";
            }
            else
            {
                if (test != TipoTest.NO_TEST)
                {
                    timer1.Enabled = false;
                    DialogResult res = MessageBox.Show("¿Desea cancelar el test actual?", "Desconexión USB-Serie", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (res == DialogResult.No)
                    {
                        timer1.Enabled = true;
                        return;
                    }
                    else test = TipoTest.NO_TEST;
                }

                try
                {
                    serialPort1.Close();
                    serialPort2.Close();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error cierre USB-Serie");
                    return;
                }
                botonConectarDesconectar.Text = "Conectar";

                contador_comandos = 0;
                timeout_esperarespuesta = 0;
                comando_actual = 0;
            }
        }

        private void comboBoxCOMs_DropDown(object sender, EventArgs e)
        {
            actualiza_COMs();
        }

        private void numericUpDownTurbina3_ValueChanged(object sender, EventArgs e)
        {
            if (test != TipoTest.TEST_TURBINAS)
            {
                dutyturbina3 = (int)numericUpDownTurbina3.Value;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if ((test == TipoTest.NO_TEST) && serialPort1.IsOpen)
            {
                leyenda_resultados_tests[(int)TipoTest.TEST_RS422] = -1;
                actualiza_marcas_leyenda_resultados_tests();
                test = TipoTest.TEST_RS422;
                contador_test = 0;
                buffer_tx_rs422 = "";
                timer1.Enabled = true;
            }
        }

        private void radioButtonDispTopR_CheckedChanged(object sender, EventArgs e)
        {
            if (test != TipoTest.TEST_TXRXOPTICOS)
            {
                RadioButton rb = (RadioButton)sender;

                if (rb.Checked) disparos |= 1;
                else disparos &= ~1;

            }
        }

        private void checkBoxLedOut_CheckedChanged(object sender, EventArgs e)
        {
            if (test != TipoTest.TEST_IO)
            {
                CheckBox cb = (CheckBox)sender;

                string tag = cb.Tag.ToString();
                int nOutput = Int32.Parse(tag);

                if (cb.Checked)
                {
                    outputleds |= 1 << nOutput;
                }
                else
                {
                    outputleds &= ~(1 << nOutput);
                }

                //serialPort1.WriteLine("output " + output + "\r");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if ((test == TipoTest.NO_TEST) && serialPort1.IsOpen)
            {
                leyenda_resultados_tests[(int)TipoTest.TEST_TURBINAS] = -1;
                actualiza_marcas_leyenda_resultados_tests();

                test = TipoTest.TEST_TURBINAS;
                contador_test = 0;
                timer1.Enabled = true;
            }
        }

        private void buttonTestTxRxOpticos_Click(object sender, EventArgs e)
        {
            if ((test == TipoTest.NO_TEST) && serialPort1.IsOpen)
            {
                leyenda_resultados_tests[(int)TipoTest.TEST_TXRXOPTICOS] = -1;
                actualiza_marcas_leyenda_resultados_tests();

                test = TipoTest.TEST_TXRXOPTICOS;
                contador_test = 0;
                timer1.Enabled = true;
            }
        }

        private void radioButtonDispTopS_CheckedChanged(object sender, EventArgs e)
        {
            if (test != TipoTest.TEST_TXRXOPTICOS)
            {
                RadioButton rb = (RadioButton)sender;

                if (rb.Checked) disparos |= 2;
                else disparos &= ~2;
            }
        }

        private void panelADC_Paint(object sender, PaintEventArgs e)
        {
            Panel pn = (Panel)sender;
            string tag = pn.Tag.ToString();
            int canal = Int32.Parse(tag);
            int sizex = pn.Size.Width;
            int sizey = pn.Size.Height;

            Pen pen = new Pen(Color.FromArgb(255, 0, 200, 0));

            for (int i = 1; i < num_samples; i++)
            {
                int x0 = (i - 1) * sizex / (num_samples - 1);
                int x1 = i * sizex / (num_samples - 1);
                int y0 = sizey - 1 - (sizey * voltaje[canal, i - 1] / 4096);
                int y1 = sizey - 1 - (sizey * voltaje[canal, i] / 4096);
                e.Graphics.DrawLine(pen, x0, y0, x1, y1);
            }
        }

        private void radioButtonDispTopT_CheckedChanged(object sender, EventArgs e)
        {
            if (test != TipoTest.TEST_TXRXOPTICOS)
            {
                RadioButton rb = (RadioButton)sender;

                if (rb.Checked) disparos |= 4;
                else disparos &= ~4;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((test == TipoTest.NO_TEST) && serialPort1.IsOpen)
            {
                leyenda_resultados_tests[(int)TipoTest.TEST_IO] = -1;
                actualiza_marcas_leyenda_resultados_tests();

                test = TipoTest.TEST_IO;
                contador_test = 0;
                timer1.Enabled = true;
            }

        }

        private void testSPV()
        {

            test_spv_temp1_med = temperatura1;
            test_spv_temp1_ref = ref_temp1;
            //test_spv_temp1_tol = double.Parse(textBoxToleranciaTemp1.Text, System.Globalization.CultureInfo.InvariantCulture);

            double aux = test_spv_temp1_med - test_spv_temp1_ref;
            if (!((aux >= -1.0) && (aux <= 15.0)))
            {
                pictureBoxTemp1.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                test_spv_ok_fallo_temp1 = 0;
            }
            else
            {
                pictureBoxTemp1.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                test_spv_ok_fallo_temp1 = 1;
            }

            test_spv_temp2_med = temperatura2;
            //test_spv_temp2_ref = ref_temp2;
            //test_spv_temp2_tol = double.Parse(textBoxToleranciaTemp2.Text, System.Globalization.CultureInfo.InvariantCulture);

            aux = test_spv_temp2_med - test_spv_temp1_ref;
            if (!((aux >= -1.0) && (aux <= 15.0)))
            {
                pictureBoxTemp2.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                test_spv_ok_fallo_temp2 = 0;
            }
            else
            {
                pictureBoxTemp2.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                test_spv_ok_fallo_temp2 = 1;
            }

            test_spv_vin_med = vin;
            test_spv_vin_ref = ref_vin;
            test_spv_vin_tol = double.Parse(textBoxToleranciaVin.Text, System.Globalization.CultureInfo.InvariantCulture);

            if ((Math.Abs(test_spv_vin_med - test_spv_vin_ref) / test_spv_vin_ref) > (test_spv_vin_tol / 100.0))
            {
                pictureBoxVin.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                test_spv_ok_fallo_vin = 0;
            }
            else
            {
                pictureBoxVin.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                test_spv_ok_fallo_vin = 1;
            }

            test_spv_iin_med = iin;
            test_spv_iin_ref = ref_iin;
            test_spv_iin_tol = double.Parse(textBoxToleranciaIin.Text, System.Globalization.CultureInfo.InvariantCulture);

            if ((Math.Abs(test_spv_iin_med - test_spv_iin_ref) / test_spv_iin_ref) > (test_spv_iin_tol / 100.0))
            {
                pictureBoxIin.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                test_spv_ok_fallo_iin = 0;
            }
            else
            {
                pictureBoxIin.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                test_spv_ok_fallo_iin = 1;
            }

            test_spv_vdsp_med = vdsp;
            test_spv_vdsp_ref = 5.0;
            test_spv_vdsp_tol = double.Parse(textBoxToleranciaVdsp.Text, System.Globalization.CultureInfo.InvariantCulture);

            if ((Math.Abs(test_spv_vdsp_med - 5.0) / 5.0) > (test_spv_vdsp_tol / 100.0))
            {
                pictureBoxVdsp.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                test_spv_ok_fallo_vdsp = 0;
            }
            else
            {
                pictureBoxVdsp.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                test_spv_ok_fallo_vdsp = 1;
            }

            test_spv_vfo_med = vfo;
            test_spv_vfo_ref = 5.0;
            test_spv_vfo_tol = double.Parse(textBoxToleranciaVfo.Text, System.Globalization.CultureInfo.InvariantCulture);

            if ((Math.Abs(test_spv_vfo_med - 5.0) / 5.0) > (test_spv_vfo_tol / 100.0))
            {
                pictureBoxVfibopt.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                test_spv_ok_fallo_vfo = 0;
            }
            else
            {
                pictureBoxVfibopt.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                test_spv_ok_fallo_vfo = 1;
            }

            test_spv_vreles_med = vreles;
            test_spv_vreles_ref = 5.0;
            test_spv_vreles_tol = double.Parse(textBoxToleranciaVreles.Text, System.Globalization.CultureInfo.InvariantCulture);

            if ((Math.Abs(test_spv_vreles_med - 5.0) / 5.0) > (test_spv_vreles_tol / 100.0))
            {
                pictureBoxVreles.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                test_spv_ok_fallo_vreles = 0;
            }
            else
            {
                pictureBoxVreles.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                test_spv_ok_fallo_vreles = 1;
            }

            test_spv_v3_3v_med = v3_3v;
            test_spv_v3_3v_ref = 3.3;
            test_spv_v3_3v_tol = double.Parse(textBoxToleranciaV3_3V.Text, System.Globalization.CultureInfo.InvariantCulture);

            if ((Math.Abs(test_spv_v3_3v_med - 3.3) / 3.3) > (test_spv_v3_3v_tol / 100.0))
            {
                pictureBoxV3_3V.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                test_spv_ok_fallo_v3_3v = 0;
            }
            else
            {
                pictureBoxV3_3V.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                test_spv_ok_fallo_v3_3v = 1;
            }

            test_spv_vbat_med = vbat;
            test_spv_vbat_ref = ref_vbat;
            test_spv_vbat_tol = double.Parse(textBoxToleranciaVbat.Text, System.Globalization.CultureInfo.InvariantCulture);

            if ((Math.Abs(test_spv_vbat_med - test_spv_vbat_ref) / test_spv_vbat_ref) > (test_spv_vbat_tol / 100.0))
            {
                pictureBoxVbat.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                test_spv_ok_fallo_vbat = 0;
            }
            else
            {
                pictureBoxVbat.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                test_spv_ok_fallo_vbat = 1;
            }

            foreach (PictureBox pb in marcasSupervisores)
            {
                pb.Visible = true;
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            // test de las medidas de las medidas de tension, corriente y temperatura de supervisores
            // comparando las medidas de la placa con los valores configurados como referencia

            // Comprobar que todos los valores de referencia se han definido

            if (!serialPort1.IsOpen) return;

            if ((ref_temp1 == -1.0) || (ref_vin == -1.0) || (ref_iin == -1.0)
                || (ref_vbat == -1.0)
                || (textBoxToleranciaVin.Text.Length == 0) || (textBoxToleranciaIin.Text.Length == 0)
                || (textBoxToleranciaVreles.Text.Length == 0) || (textBoxToleranciaVdsp.Text.Length == 0)
                || (textBoxToleranciaV3_3V.Text.Length == 0) || (textBoxToleranciaVfo.Text.Length == 0)
                || (textBoxToleranciaVbat.Text.Length == 0))
            {
                MessageBox.Show("Este test compara las medidas de temperaturas/tensiones/corrientes" +
                    " de los supervisores de la placa con valores nominales de los diferentes" +
                    " reguladores (+5V, +3.3V), o con valores configurados como" +
                    " referencia tal como las medidas de la tensión de la bateria, la de entrada de alimentación" + 
                    " (+10V a +24V), o la de la corriente de alimentación." +
                    "\nEn el caso de los sensores de temperatura se usara como referencia la" + 
                    " temperatura ambiente medida en la proximidad de la placa." +
                    " NOTA: El chequeo será en este caso un chequeo de lectura 'absurda'," +
                    " es decir se comprueba que la lectura de ambos se encuentra en el" +
                    " rango (Tamb - 1ºC) a (Tamb + 15ºC)." +
                    "\n\nAntes de ejecutar el test deben definirse los valores de referencia.",
                    "Test de supervisores V/I/TEMP", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            test_spv_ok_fallo_temp1 = -1;
            test_spv_ok_fallo_temp2 = -1;
            test_spv_ok_fallo_vin = -1;
            test_spv_ok_fallo_iin = -1;
            test_spv_ok_fallo_vdsp = -1;
            test_spv_ok_fallo_vfo = -1;
            test_spv_ok_fallo_vreles = -1;
            test_spv_ok_fallo_v3_3v = -1;
            test_spv_ok_fallo_vbat = -1;

            leyenda_resultados_tests[(int)TipoTest.TEST_SPVS] = -1;
            actualiza_marcas_leyenda_resultados_tests();

            testSPV();

            if ((test_spv_ok_fallo_temp1 == 1) && (test_spv_ok_fallo_temp2 == 1) && (test_spv_ok_fallo_vin == 1)
                && (test_spv_ok_fallo_iin == 1) && (test_spv_ok_fallo_vdsp == 1) && (test_spv_ok_fallo_vreles == 1)
                && (test_spv_ok_fallo_vfo == 1) && (test_spv_ok_fallo_v3_3v == 1) && (test_spv_ok_fallo_vbat == 1))
                leyenda_resultados_tests[(int)TipoTest.TEST_SPVS] = 1;
            else leyenda_resultados_tests[(int)TipoTest.TEST_SPVS] = 0;

            actualiza_marcas_leyenda_resultados_tests();
        }

        private void checkBoxOut_CheckedChanged(object sender, EventArgs e)
        {
            if (test != TipoTest.TEST_IO)
            {
                CheckBox cb = (CheckBox)sender;

                string tag = cb.Tag.ToString();
                int nOutput = Int32.Parse(tag);

                if (cb.Checked)
                {
                    output |= 1 << nOutput;
                }
                else
                {
                    output &= ~(1 << nOutput);
                }

                //serialPort1.WriteLine("output " + output + "\r");
            }


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Prueba de entradas salidas
            CheckBox[] checkBoxesReles = { checkBoxOut0, checkBoxOut1, checkBoxOut2, checkBoxOut3,
                                      checkBoxOut4, checkBoxOut5, checkBoxOut6, checkBoxOut7,
                                      checkBoxOut8, checkBoxOut9, checkBoxOut10, checkBoxOut11,
                                      checkBoxOut12, checkBoxOut13, checkBoxOut14, checkBoxOut15};
            CheckBox[] checkBoxesLEDs = { checkBoxLedOut0, checkBoxLedOut1, checkBoxLedOut2, checkBoxLedOut3, checkBoxLedOut4 };

            int progreso = 0;

            switch (test)
            {
                case TipoTest.TEST_IO:
                    if (contador_test == 0)
                    {
                        foreach (PictureBox pb in marcasReles)
                        {
                            pb.Visible = false;
                        }
                        foreach (PictureBox pb in marcasInputs)
                        {
                            pb.Visible = false;
                        }
                        foreach (PictureBox pb in marcasLEDs)
                        {
                            pb.Visible = false;
                        }

                        progressBarTestActual.Value = 0;
                        for (int i = 0; i < 16; i++)
                        {
                            test_io_reles_entradas_ok_fallo[i] = -1;
                        }
                        for (int i = 0; i < 5; i++)
                        {
                            test_io_leds_entradas_ok_fallo[i] = -1;
                        }


                        // Aqui mensaje al usuario para que conecte cables entre reles y entradas
                        timer1.Enabled = false;
                        MessageBox.Show("Conectar cables entre conectores:\n* CON22<->CON4\n* CON32<->CON45\n* CON33<->CON54\n* CON34<->CON55.\n" +
                            "A continuación pulsar Aceptar.", "Prueba de reles y entradas.");
                        timer1.Enabled = true;

                        output = 0;
                        outputleds = 0;
                        respuesta_output_ok = 0;
                        respuesta_outputleds_ok = 0;
                        contador_test = 1;

                        timeout_secuencia_test = 0;
                    }
                    if (contador_test == 1)
                    {
                        if (timeout_secuencia_test > 10 * 5) // 5 segundos
                        {
                            timer1.Enabled = false;
                            test = TipoTest.NO_TEST;
                            MessageBox.Show("Test abortado: Duracion excesiva", "Prueba de reles, salidas optoacopladas y entradas", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        timeout_secuencia_test++;

                        for (int i = 0; i < 16; i++)
                        {
                            if ((output & (1 << i)) == 0)
                            {
                                checkBoxesReles[i].Checked = false;
                            }
                            else
                            {
                                checkBoxesReles[i].Checked = true;
                            }
                        }
                        for (int i = 0; i < 5; i++)
                        {
                            if ((outputleds & (1 << i)) == 0)
                            {
                                checkBoxesLEDs[i].Checked = false;
                            }
                            else
                            {
                                checkBoxesLEDs[i].Checked = true;
                            }
                        }
                        if ((respuesta_output_ok == 1) && (respuesta_outputleds_ok == 1)) contador_test = 2;
                    }
                    else if (contador_test <= 6)
                    {
                        contador_test++;
                    }
                    else if (contador_test == 7)
                    {
                        int num_entradas;
                        int fallos;
                        int input_probada;
                        if ((output & ~0xffff) == 0)
                        {
                            input_probada = output;
                            num_entradas = 16;
                        }
                        else
                        {
                            input_probada = outputleds;
                            num_entradas = 5;
                        }

                        fallos = input ^ input_probada;

                        for (int i = 0; i < num_entradas; i++)
                        {
                            if ((fallos & (1 << i)) != 0)
                            {
                                marcasInputs[i].Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                                marcasInputs[i].Visible = true;
                                if (num_entradas == 16)
                                {
                                    marcasReles[i].Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                                    marcasReles[i].Visible = true;
                                    test_io_reles_entradas_ok_fallo[i] = 0;
                                }
                                else
                                {
                                    marcasLEDs[i].Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                                    marcasLEDs[i].Visible = true;
                                    test_io_leds_entradas_ok_fallo[i] = 0;
                                }
                                leyenda_resultados_tests[(int)TipoTest.TEST_IO] = 0;
                                actualiza_marcas_leyenda_resultados_tests();
                            }
                            else if (marcasInputs[i].Visible != true)
                            {
                                marcasInputs[i].Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                                marcasInputs[i].Visible = true;
                                if (num_entradas == 16)
                                {
                                    marcasReles[i].Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                                    marcasReles[i].Visible = true;
                                }
                                else
                                {
                                    marcasLEDs[i].Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                                    marcasLEDs[i].Visible = true;
                                }
                            }
                        }

                        if (output == 0)
                        {
                            output = 1;
                            respuesta_output_ok = 0;
                        }
                        else if (output < 0x8000)
                        {
                            output <<= 1;
                            respuesta_output_ok = 0;
                        }
                        else if (output == 0x8000)
                        {
                            for (int i = 0; i < 16; i++)
                            {
                                if (test_io_reles_entradas_ok_fallo[i] == -1) test_io_reles_entradas_ok_fallo[i] = 1;
                            }

                            output <<= 1;
                            checkBoxesReles[15].Checked = false;
                            // Aqui mensaje al usuario para que conecte cables entre leds y entradas
                            timer1.Enabled = false;
                            MessageBox.Show("Conectar cable entre conector CON35 y conectores CON4 y CON45.\n" +
                                "A continuación pulsar Aceptar.", "Prueba de salidas optoacopladas (usando entradas).");
                            timer1.Enabled = true;

                            for (int i = 0; i < 5; i++)
                            {
                                marcasInputs[i].Visible = false;
                            }

                            respuesta_outputleds_ok = 0;

                        }
                        else
                        {
                            if (outputleds == 0)
                            {
                                outputleds = 1;
                            }
                            else outputleds <<= 1;
                            respuesta_outputleds_ok = 0;
                        }

                        if (outputleds == 0x20)
                        {
                            checkBoxesLEDs[4].Checked = false;
                            timer1.Enabled = false;
                            test = TipoTest.NO_TEST;
                            if (leyenda_resultados_tests[(int)TipoTest.TEST_IO] == -1) leyenda_resultados_tests[(int)TipoTest.TEST_IO] = 1;
                            actualiza_marcas_leyenda_resultados_tests();

                            for (int i = 0; i < 5; i++)
                            {
                                if (test_io_leds_entradas_ok_fallo[i] == -1) test_io_leds_entradas_ok_fallo[i] = 1;
                            }
                        }

                        contador_test = 1;
                        timeout_secuencia_test = 0;
                    }

                    if (test == TipoTest.NO_TEST)
                        progressBarTestActual.Value = 100;
                    else
                    {
                        if (output == 0)
                        {
                            progreso = 0;
                        }
                        else if (output <= 0x8000)
                        {
                            progreso = ((int)Math.Log(output, 2)) + 1;
                        }
                        else if (outputleds == 0)
                        {
                            progreso = 17;
                        }
                        else
                        {
                            progreso = ((int)Math.Log(outputleds, 2)) + 18;
                        }

                        progreso = (progreso * 100 + contador_test * 100 / 7) / 22;
                        if (progreso > 100) progreso = 100;
                        progressBarTestActual.Value = progreso;
                    }

                    break;
                case TipoTest.TEST_TXRXOPTICOS:
                    if (contador_test == 0)
                    {
                        foreach (PictureBox pb in marcasDisparos)
                        {
                            pb.Visible = false;
                        }
                        foreach (PictureBox pb in marcasErroresOpticos)
                        {
                            pb.Visible = false;
                        }
                        pictureBoxSynchSD.Visible = false;

                        progressBarTestActual.Value = 0;

                        for (int i = 0; i < 6; i++)
                        {
                            test_fo_disp_error_ok_fallo[i] = -1;
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            test_fo_disp_ovt_ok_fallo[i] = -1;
                        }
                        test_fo_bot_s_rxsynch_ok_fallo = -1;

                        // Aqui mensaje al usuario para que conecte elaces opticos de disparos a errores de driver
                        timer1.Enabled = false;
                        MessageBox.Show("Conectar enlaces ópticos de disparos a errores de driver:\n* XT1 <-> XR1\n* XT3 <-> XR3\n* XT5 <-> XR6\n* XT7 <-> XR8\n* XT9 <-> XR11\n XT11 <-> XR13\n" +
                            "A continuación pulsar Aceptar.", "Prueba de transmisores y receptores ópticos.");
                        timer1.Enabled = true;

                        disparos = 0;
                        respuesta_disparos_ok = 0;
                        contador_test = 1;

                        timeout_secuencia_test = 0;
                    }
                    if (contador_test == 1)
                    {
                        if (timeout_secuencia_test > 10 * 5) // 5 segundos
                        {
                            timer1.Enabled = false;
                            test = TipoTest.NO_TEST;
                            MessageBox.Show("Test abortado: Duracion excesiva", "Prueba de transmisores y receptores ópticos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        timeout_secuencia_test++;

                        radioButtonDispTopR.Checked = ((disparos & 1) != 0);
                        radioButtonDispBotR.Checked = !((disparos & 1) != 0);
                        radioButtonDispTopS.Checked = ((disparos & 2) != 0);
                        radioButtonDispBotS.Checked = !((disparos & 2) != 0);
                        radioButtonDispTopT.Checked = ((disparos & 4) != 0);
                        radioButtonDispBotT.Checked = !((disparos & 4) != 0);
                        if (respuesta_disparos_ok == 1) contador_test = 2;
                    }
                    else if (contador_test < 7)
                    {
                        contador_test++;
                    }
                    else if (contador_test == 7)
                    {
                        int fallos;
                        int errores_esperados;
                        int num_errores_a_comprobar;

                        if (disparos < 8)
                        {
                            int masc_top = 8;
                            int masc_bot = 0x0040;

                            num_errores_a_comprobar = 6;
                            errores_esperados = 0x8007;
                            for (int i = 0; i < 3; i++)
                            {
                                if ((disparos & (1 << i)) != 0)
                                {
                                    errores_esperados &= ~masc_top;
                                    errores_esperados |= masc_bot;
                                }
                                else
                                {
                                    errores_esperados |= masc_top;
                                    errores_esperados &= ~masc_bot;
                                }
                                masc_top <<= 1;
                                masc_bot <<= 1;
                            }
                        }
                        else
                        {
                            num_errores_a_comprobar = 3;
                            errores_esperados = 0;
                            if ((disparos & 1) == 0) errores_esperados |= 1;
                            else errores_esperados |= 2;
                            if ((disparos & 2) == 0) errores_esperados |= 4;
                            if ((disparos & 2) != 0) errores_esperados |= 0x8000;
                        }

                        fallos = errores ^ errores_esperados;

                        for (int i = 0; i < num_errores_a_comprobar; i++)
                        {
                            int errorinicial;
                            if (num_errores_a_comprobar == 6)
                                errorinicial = 3;
                            else
                                errorinicial = 0;
                            if ((fallos & (1 << (i + errorinicial))) != 0)
                            {
                                marcasErroresOpticos[i + errorinicial].Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                                marcasErroresOpticos[i + errorinicial].Visible = true;
                                leyenda_resultados_tests[(int)TipoTest.TEST_TXRXOPTICOS] = 0;
                                actualiza_marcas_leyenda_resultados_tests();

                                if (num_errores_a_comprobar == 6)
                                {
                                    test_fo_disp_error_ok_fallo[i] = 0;
                                }
                                else
                                {
                                    test_fo_disp_ovt_ok_fallo[i] = 0;
                                }
                            }
                            else if (marcasErroresOpticos[i + errorinicial].Visible != true)
                            {
                                marcasErroresOpticos[i + errorinicial].Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                                marcasErroresOpticos[i + errorinicial].Visible = true;
                            }
                        }
                        if (disparos >= 8)
                        {
                            if ((fallos & 0x8000) != 0)
                            {
                                pictureBoxSynchSD.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                                pictureBoxSynchSD.Visible = true;
                                leyenda_resultados_tests[(int)TipoTest.TEST_TXRXOPTICOS] = 0;
                                actualiza_marcas_leyenda_resultados_tests();

                                test_fo_bot_s_rxsynch_ok_fallo = 0;
                            }
                            else if (pictureBoxSynchSD.Visible != true)
                            {
                                pictureBoxSynchSD.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                                pictureBoxSynchSD.Visible = true;
                            }
                        }

                        if (disparos == 7)
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                if (test_fo_disp_error_ok_fallo[i] == -1) test_fo_disp_error_ok_fallo[i] = 1;
                            }

                            // Aqui mensaje al usuario para que conecte enlaces opticos de disparos top a errores temp y de botR
                            // a receptor de sincronismo
                            timer1.Enabled = false;
                            MessageBox.Show("Conectar enlaces ópticos entre:\n* TOP R (XT1) <-> ERROR_OVTEMP_R (XR5)\n* BOT R (XT3) <-> ERROR_OVTEMP_S (XR10)\n* TOP S (XT5) <-> ERROR_OVTEMP_T (XR15)\n* BOT S (XT7) <-> RX_SYNCH (XR16)\n" +
                                "A continuación pulsar Aceptar.", "Prueba de transmisores y receptores ópticos.");
                            timer1.Enabled = true;
                        }

                        if (disparos == 16)
                        {
                            timer1.Enabled = false;
                            test = TipoTest.NO_TEST;

                            for (int i = 0; i < 3; i++)
                            {
                                if (test_fo_disp_ovt_ok_fallo[i] == -1) test_fo_disp_ovt_ok_fallo[i] = 1;
                            }
                            if (test_fo_bot_s_rxsynch_ok_fallo == -1) test_fo_bot_s_rxsynch_ok_fallo = 1;

                            if (leyenda_resultados_tests[(int)TipoTest.TEST_TXRXOPTICOS] == -1) leyenda_resultados_tests[(int)TipoTest.TEST_TXRXOPTICOS] = 1;
                            actualiza_marcas_leyenda_resultados_tests();
                        }
                        else
                        {
                            disparos++;
                            respuesta_disparos_ok = 0;
                        }

                        contador_test = 1;
                        timeout_secuencia_test = 0;
                    }

                    if (test == TipoTest.NO_TEST)
                        progressBarTestActual.Value = 100;
                    else
                    {
                        progreso = disparos * 100 / 16;
                        progreso = progreso + contador_test * 100 / (16 * 7);
                        if (progreso > 100) progreso = 100;

                        progressBarTestActual.Value = progreso;
                    }

                    break;
                case TipoTest.TEST_TURBINAS:
                    if (contador_test == 0)
                    {
                        foreach (PictureBox pb in marcasTurbinas)
                        {
                            pb.Visible = false;
                        }

                        progressBarTestActual.Value = 0;

                        test_ctrlturbina1_ok_fallo = -1;
                        test_ctrlturbina1_pulsos_sec_0 = -1;
                        test_ctrlturbina1_pulsos_sec_50 = -1;
                        test_ctrlturbina2_ok_fallo = -1;
                        test_ctrlturbina2_pulsos_sec_0 = -1;
                        test_ctrlturbina2_pulsos_sec_50 = -1;
                        test_ctrlturbina3_ok_fallo = -1;
                        test_ctrlturbina3_pulsos_sec_0 = -1;
                        test_ctrlturbina3_pulsos_sec_50 = -1;

                        // Aqui mensaje al usuario para que conecte conectores puente en los conectores de turbinas
                        timer1.Enabled = false;
                        MessageBox.Show("Conectar conectores puente en conectores CON46, CON48 y CON49.\n" +
                            "A continuación pulsar Aceptar.", "Prueba de interfaces de control de velocidad de turbinas.");
                        timer1.Enabled = true;

                        dutyturbina1 = 0;
                        dutyturbina2 = 0;
                        dutyturbina3 = 0;
                        respuesta_dutyturbina_ok = 0;

                        contador_test = 1;

                        timeout_secuencia_test = 0;
                    }
                    if (contador_test == 1)
                    {
                        if (timeout_secuencia_test > 10 * 5) // 5 segundos
                        {
                            timer1.Enabled = false;
                            test = TipoTest.NO_TEST;
                            MessageBox.Show("Test abortado: Duracion excesiva", "Prueba de interfaces de control de velocidad de turbinas", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        timeout_secuencia_test++;

                        numericUpDownTurbina1.Value = dutyturbina1;
                        numericUpDownTurbina2.Value = dutyturbina2;
                        numericUpDownTurbina3.Value = dutyturbina3;
                        if (respuesta_dutyturbina_ok == 1) contador_test = 2;
                    }
                    else if (contador_test < 30)
                    {
                        contador_test++;
                    }
                    else if (contador_test == 30)
                    {
                        if ((dutyturbina1 == 0) && (dutyturbina2 == 0) && (dutyturbina3 == 0))
                        {
                            test_ctrlturbina1_pulsos_sec_0 = rpm1;
                            test_ctrlturbina2_pulsos_sec_0 = rpm2;
                            test_ctrlturbina3_pulsos_sec_0 = rpm3;
                        }

                        if (dutyturbina1 == 50) test_ctrlturbina1_pulsos_sec_50 = rpm1;
                        if (dutyturbina2 == 50) test_ctrlturbina2_pulsos_sec_50 = rpm2;
                        if (dutyturbina3 == 50) test_ctrlturbina3_pulsos_sec_50 = rpm3;

                        if (((dutyturbina1 == 0) && (rpm1 == 0)) || ((dutyturbina1 == 50) && (rpm1 > 4950) && (rpm1 < 5050)))
                        {
                            if (pictureBoxTurbina1.Visible == false)
                            {
                                pictureBoxTurbina1.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                                pictureBoxTurbina1.Visible = true;
                            }
                        }
                        else
                        {
                            if (dutyturbina1 == 0)
                            {
                                test_ctrlturbina1_pulsos_sec_0 = rpm1;
                            }

                            pictureBoxTurbina1.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                            pictureBoxTurbina1.Visible = true;
                            leyenda_resultados_tests[(int)TipoTest.TEST_TURBINAS] = 0;
                            test_ctrlturbina1_ok_fallo = 0;
                            actualiza_marcas_leyenda_resultados_tests();
                        }
                        if (((dutyturbina2 == 0) && (rpm2 == 0)) || ((dutyturbina2 == 50) && (rpm2 > 4950) && (rpm2 < 5050)))
                        {
                            if (pictureBoxTurbina2.Visible == false)
                            {
                                pictureBoxTurbina2.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                                pictureBoxTurbina2.Visible = true;
                            }
                        }
                        else
                        {
                            if (dutyturbina2 == 0)
                            {
                                test_ctrlturbina2_pulsos_sec_0 = rpm1;
                            }

                            pictureBoxTurbina2.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                            pictureBoxTurbina2.Visible = true;
                            leyenda_resultados_tests[(int)TipoTest.TEST_TURBINAS] = 0;
                            test_ctrlturbina2_ok_fallo = 0;
                            actualiza_marcas_leyenda_resultados_tests();
                        }
                        if (((dutyturbina3 == 0) && (rpm3 == 0)) || ((dutyturbina3 == 50) && (rpm3 > 4950) && (rpm3 < 5050)))
                        {
                            if (pictureBoxTurbina3.Visible == false)
                            {
                                pictureBoxTurbina3.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                                pictureBoxTurbina3.Visible = true;
                            }
                        }
                        else
                        {
                            if (dutyturbina3 == 0)
                            {
                                test_ctrlturbina3_pulsos_sec_0 = rpm1;
                            }

                            pictureBoxTurbina3.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                            pictureBoxTurbina3.Visible = true;
                            leyenda_resultados_tests[(int)TipoTest.TEST_TURBINAS] = 0;
                            test_ctrlturbina3_ok_fallo = 0;
                            actualiza_marcas_leyenda_resultados_tests();
                        }

                        if ((dutyturbina1 == 0) && (dutyturbina2 == 0) && (dutyturbina3 == 0))
                            dutyturbina1 = 50;
                        else if (dutyturbina1 == 50)
                        {
                            dutyturbina1 = 0;
                            dutyturbina2 = 50;
                        }
                        else if (dutyturbina2 == 50)
                        {
                            dutyturbina2 = 0;
                            dutyturbina3 = 50;
                        }
                        else if (dutyturbina3 == 50)
                        {
                            timer1.Enabled = false;
                            test = TipoTest.NO_TEST;

                            if (test_ctrlturbina1_ok_fallo == -1) test_ctrlturbina1_ok_fallo = 1;
                            if (test_ctrlturbina2_ok_fallo == -1) test_ctrlturbina2_ok_fallo = 1;
                            if (test_ctrlturbina3_ok_fallo == -1) test_ctrlturbina3_ok_fallo = 1;

                            if (leyenda_resultados_tests[(int)TipoTest.TEST_TURBINAS] == -1) leyenda_resultados_tests[(int)TipoTest.TEST_TURBINAS] = 1;
                            actualiza_marcas_leyenda_resultados_tests();
                        }

                        respuesta_dutyturbina_ok = 0;
                        contador_test = 1;
                        timeout_secuencia_test = 0;
                    }

                    if (test == TipoTest.NO_TEST)
                        progressBarTestActual.Value = 100;
                    else
                    {
                        if ((dutyturbina1 == 0) && (dutyturbina2 == 0) && (dutyturbina3 == 0))
                            progreso = 0;
                        else if (dutyturbina1 == 50)
                        {
                            progreso = 25;
                        }
                        else if (dutyturbina2 == 50)
                        {
                            progreso = 50;
                        }
                        else if (dutyturbina3 == 50)
                        {
                            progreso = 75;
                        }

                        progreso = progreso + contador_test * 25 / 30;
                        if (progreso > 100) progreso = 100;
                        progressBarTestActual.Value = progreso;
                    }

                    break;
                case TipoTest.TEST_RS422:
                    if (contador_test == 0)
                    {
                        pictureBoxRS422.Visible = false;

                        progressBarTestActual.Value = 0;
                        test_rs422_loop_rxdis_txdis_ok_fallo = -1;
                        test_rs422_loop_rxdis_txena_ok_fallo = -1;
                        test_rs422_loop_rxena_txdis_ok_fallo = -1;
                        test_rs422_loop_rxena_txena_ok_fallo = -1;

                        // Aqui mensaje al usuario 
                        timer1.Enabled = false;
                        MessageBox.Show("Conectar conector puente en conector de puerto RS422 (CON12).\n" + 
                            "A continuación pulsar Aceptar.", "Prueba de puerto RS422.");
                        timer1.Enabled = true;

                        enabletx_rs422 = 0;
                        enablerx_rs422 = 0;
                        respuesta_enabletx_rs422_ok = 0;
                        respuesta_enablerx_rs422_ok = 0;
                        buffer_tx_rs422_test = "Probando RS422: 0123456789ABCDEF";

                        contador_test = 1;

                        timeout_secuencia_test = 0;
                    }
                    if (contador_test == 1)
                    {
                        if (timeout_secuencia_test > 10 * 5) // 5 segundos
                        {
                            timer1.Enabled = false;
                            test = TipoTest.NO_TEST;
                            MessageBox.Show("Test abortado: Duracion excesiva", "Prueba de puerto RS422", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        timeout_secuencia_test++;

                        checkBoxEnableTX_RS422.Checked = (enabletx_rs422 == 1);
                        checkBoxEnableRX_RS422.Checked = (enablerx_rs422 == 1);

                        if ((respuesta_enabletx_rs422_ok == 1) && (respuesta_enablerx_rs422_ok == 1))
                            contador_test = 2;
                    }
                    else if (contador_test < 10)
                    {
                        contador_test++;
                    }
                    else if (contador_test == 10)
                    {
                        buffer_rx_rs422 = "";
                        textBoxRS422.Text = "";
                        buffer_tx_rs422 = buffer_tx_rs422_test;
                        contador_test++;
                    }
                    else if (contador_test < 20)
                    {
                        contador_test++;
                    }
                    else if (contador_test == 20)
                    {
                        bool error;

                        if ((enabletx_rs422 == 0) || (enablerx_rs422 == 0))
                        {
                            if (textBoxRS422.Text == "") error = false;
                            else error = true;
                        }
                        else
                        {
                            if (textBoxRS422.Text == "Probando RS422: 0123456789ABCDEF") error = false;
                            else error = true;
                        }

                        if (error)
                        {
                            if ((enabletx_rs422 == 0) && (enablerx_rs422 == 0)) test_rs422_loop_rxdis_txdis_ok_fallo = 0;
                            if ((enabletx_rs422 == 1) && (enablerx_rs422 == 0)) test_rs422_loop_rxdis_txena_ok_fallo = 0;
                            if ((enabletx_rs422 == 0) && (enablerx_rs422 == 1)) test_rs422_loop_rxena_txdis_ok_fallo = 0;
                            if ((enabletx_rs422 == 1) && (enablerx_rs422 == 1)) test_rs422_loop_rxena_txena_ok_fallo = 0;

                            pictureBoxRS422.Visible = true;
                            pictureBoxRS422.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                            //timer1.Enabled = false;
                            //test = TipoTest.NO_TEST;
                            leyenda_resultados_tests[(int)TipoTest.TEST_RS422] = 0;
                            actualiza_marcas_leyenda_resultados_tests();
                        }
                        else
                        {
                            if ((enabletx_rs422 == 0) && (enablerx_rs422 == 0)) test_rs422_loop_rxdis_txdis_ok_fallo = 1;
                            if ((enabletx_rs422 == 1) && (enablerx_rs422 == 0)) test_rs422_loop_rxdis_txena_ok_fallo = 1;
                            if ((enabletx_rs422 == 0) && (enablerx_rs422 == 1)) test_rs422_loop_rxena_txdis_ok_fallo = 1;
                            if ((enabletx_rs422 == 1) && (enablerx_rs422 == 1)) test_rs422_loop_rxena_txena_ok_fallo = 1;
                        }

                        if ((enabletx_rs422 == 0) && (enablerx_rs422 == 0))
                        {
                            enabletx_rs422 = 1;
                        }
                        else if ((enabletx_rs422 == 1) && (enablerx_rs422 == 0))
                        {
                            enabletx_rs422 = 0;
                            enablerx_rs422 = 1;
                        }
                        else if ((enabletx_rs422 == 0) && (enablerx_rs422 == 1))
                        {
                            enabletx_rs422 = 1;
                            enablerx_rs422 = 1;
                        }
                        else if ((enabletx_rs422 == 1) && (enablerx_rs422 == 1))
                        {
                            timer1.Enabled = false;
                            test = TipoTest.NO_TEST;

                            if (leyenda_resultados_tests[(int)TipoTest.TEST_RS422] == -1)
                            {
                                pictureBoxRS422.Visible = true;
                                pictureBoxRS422.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                                leyenda_resultados_tests[(int)TipoTest.TEST_RS422] = 1;
                                actualiza_marcas_leyenda_resultados_tests();
                            }
                        }

                        contador_test = 1;
                        timeout_secuencia_test = 0;
                    }

                    if (test == TipoTest.NO_TEST)
                        progressBarTestActual.Value = 100;
                    else
                    {
                        if ((enabletx_rs422 == 0) && (enablerx_rs422 == 0))
                        {
                            progreso = 0;
                        }
                        else if ((enabletx_rs422 == 1) && (enablerx_rs422 == 0))
                        {
                            progreso = 25;
                        }
                        else if ((enabletx_rs422 == 0) && (enablerx_rs422 == 1))
                        {
                            progreso = 50;
                        }
                        else if ((enabletx_rs422 == 1) && (enablerx_rs422 == 1))
                        {
                            progreso = 75;
                        }

                        progreso = progreso + contador_test * 25 / 20;
                        if (progreso > 100) progreso = 100;
                        progressBarTestActual.Value = progreso;
                    }

                    break;
                case TipoTest.TEST_CAN:
                    if (contador_test == 0)
                    {
                        pictureBoxCAN.Visible = false;

                        progressBarTestActual.Value = 0;
                        test_cana_loop_ok_fallo = -1;
                        test_canb_loop_ok_fallo = -1;
                        test_cana_canb_ok_fallo = -1;

                        // Aqui mensaje al usuario para que el cable CAN este desconectado
                        timer1.Enabled = false;
                        MessageBox.Show("Desconecte (si lo estaba) el cable entre ambos puertos CAN.\n" +
                            "A continuación pulsar Aceptar.", "Prueba de puertos CAN A y CAN B.");
                        timer1.Enabled = true;
                        con_cable_can = false;

                        canatx = 0;
                        canbtx = 0;
                        respuesta_canatx_ok = 0;
                        respuesta_canbtx_ok = 0;
                        contador_test = 1;

                        timeout_secuencia_test = 0;
                    }
                    if (contador_test == 1)
                    {
                        if (timeout_secuencia_test > 10 * 5) // 5 segundos
                        {
                            timer1.Enabled = false;
                            test = TipoTest.NO_TEST;
                            MessageBox.Show("Test abortado: Duracion excesiva", "Prueba de puertos CAN A y CAN B", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        timeout_secuencia_test++;

                        checkBoxCANATX.Checked = ((canatx & 1) != 0);
                        checkBoxCANBTX.Checked = ((canbtx & 1) != 0);

                        if ((respuesta_canatx_ok == 1) && (respuesta_canbtx_ok == 1)) contador_test = 2;
                    }
                    else if (contador_test < 7)
                    {
                        contador_test++;
                    }
                    else if (contador_test == 7)
                    {
                        bool error;

                        if (con_cable_can == false)
                        {
                            if ((canatx == canarx) && (canbtx == canbrx))
                            {
                                error = false;
                                if ((canatx == 1) && (canbtx == 1))
                                {
                                    if (test_cana_loop_ok_fallo == -1) test_cana_loop_ok_fallo = 1;
                                    if (test_canb_loop_ok_fallo == -1) test_canb_loop_ok_fallo = 1;
                                }
                            }
                            else
                            {
                                error = true;
                                if (canatx != canarx) test_cana_loop_ok_fallo = 0;
                                if (canbtx != canbrx) test_canb_loop_ok_fallo = 0;
                            }
                        }
                        else
                        {
                            if ((((canatx == 0) || (canbtx == 0)) && (canarx == 0) && (canbrx == 0)) || (((canatx == 1) && (canbtx == 1)) && (canarx == 1) && (canbrx == 1)))
                            {
                                error = false;
                                if ((canatx == 1) && (canbtx == 1))
                                    if(test_cana_canb_ok_fallo == -1)
                                        test_cana_canb_ok_fallo = 1;
                            }
                            else
                            {
                                error = true;
                                test_cana_canb_ok_fallo = 0;
                            }
                        }

                        if (error)
                        {
                            pictureBoxCAN.Visible = true;
                            pictureBoxCAN.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                            //timer1.Enabled = false;
                            //test = TipoTest.NO_TEST;
                            leyenda_resultados_tests[(int)TipoTest.TEST_CAN] = 0;
                            actualiza_marcas_leyenda_resultados_tests();
                        }

                        if (!con_cable_can)
                        {
                            if (canatx == 0)
                                canatx = 1;
                            else
                            {
                                if (canbtx == 0)
                                {
                                    canatx = 0;
                                    canbtx = 1;
                                }
                                else
                                {
                                    // Aqui mensaje al usuario para que el cable CAN se conecte
                                    timer1.Enabled = false;
                                    MessageBox.Show("Conectar el cable entre ambos puertos CAN.\n" +
                                        "A continuación pulsar Aceptar.", "Prueba de puertos CAN A y CAN B.");
                                    timer1.Enabled = true;
                                    con_cable_can = true;
                                    canatx = 0;
                                    canbtx = 0;
                                }
                            }
                        }
                        else
                        {
                            if (canatx == 0)
                                canatx = 1;
                            else
                            {
                                if (canbtx == 0)
                                {
                                    canatx = 0;
                                    canbtx = 1;
                                }
                                else
                                {
                                    timer1.Enabled = false;
                                    test = TipoTest.NO_TEST;
                                    if (leyenda_resultados_tests[(int)TipoTest.TEST_CAN] == -1)
                                    {
                                        pictureBoxCAN.Visible = true;
                                        pictureBoxCAN.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                                        leyenda_resultados_tests[(int)TipoTest.TEST_CAN] = 1;
                                        actualiza_marcas_leyenda_resultados_tests();
                                    }
                                }
                            }
                        }

                        contador_test = 1;
                        timeout_secuencia_test = 0;
                        respuesta_canatx_ok = 0;
                        respuesta_canbtx_ok = 0;

                    }

                    if (test == TipoTest.NO_TEST)
                        progressBarTestActual.Value = 100;
                    else
                    {
                        progreso = 0;
                        if (con_cable_can) progreso += 50;
                        if (canatx == 1) progreso += 12;
                        if (canbtx == 1) progreso += 25;
                        if (progreso > 100) progreso = 100;

                        progressBarTestActual.Value = progreso;
                    }
                    break;
                case TipoTest.TEST_SRAM:
                    if (contador_test == 0)
                    {
                        pictureBoxSRAM.Visible = false;
                        test_sram_tiempo_test = -1;
                        progressBarTestActual.Value = 0;

                        respuesta_test_sram = -1;
                        buffer_tx += "testsram\r";
                        contador_comandos++;

                        contador_test = 1;

                        timeout_secuencia_test = 0;
                    }
                    if (contador_test == 1)
                    {
                        if (timeout_secuencia_test > 5 * 10) // 5 segundos
                        {
                            timer1.Enabled = false;
                            test = TipoTest.NO_TEST;
                            MessageBox.Show("Test abortado: Duracion excesiva", "Prueba de memoria SRAM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        timeout_secuencia_test++;

                        if (respuesta_test_sram != -1)
                        {
                            timer1.Enabled = false;
                            test = TipoTest.NO_TEST;

                            progressBarTestActual.Value = 100;

                            if (respuesta_test_sram == 0) pictureBoxSRAM.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                            else pictureBoxSRAM.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                            pictureBoxSRAM.Visible = true;

                            test_sram_tiempo_test = timeout_secuencia_test;
                            leyenda_resultados_tests[(int)TipoTest.TEST_SRAM] = respuesta_test_sram;
                            actualiza_marcas_leyenda_resultados_tests();
                        }
                    }
                    break;
                case TipoTest.TEST_FRAM:
                    if (contador_test == 0)
                    {
                        pictureBoxFRAMEscritura1.Visible = false;
                        pictureBoxFRAMEscritura2.Visible = false;
                        pictureBoxFRAMBloqueo.Visible = false;
                        pictureBoxFRAMDesbloqueo.Visible = false;
                        progressBarTestActual.Value = 0;

                        test_fram_escritura1_ok_fallo = -1;
                        test_fram_escritura2_ok_fallo = -1;
                        test_fram_bloqueo_ok_fallo = -1;
                        test_fram_desbloqueo_ok_fallo = -1;
                        test_fram_tiempo_test_escritura1 = -1;
                        test_fram_tiempo_test_escritura2 = -1;
                        test_fram_tiempo_test_bloqueo = -1;
                        test_fram_tiempo_test_desbloqueo = -1;

                        respuesta_test_fram = -1;
                        buffer_tx += "testfram\r";
                        contador_comandos++;

                        contador_test = 1;

                        timeout_secuencia_test = 0;
                    }
                    if (contador_test == 1)
                    {
                        if (respuesta_test_fram != -1)
                        {
                            test_fram_tiempo_test_escritura1 = timeout_secuencia_test;

                            if (respuesta_test_fram != 1)
                            {
                                timer1.Enabled = false;
                                test = TipoTest.NO_TEST;
                                pictureBoxFRAMEscritura1.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                                leyenda_resultados_tests[(int)TipoTest.TEST_FRAM] = 0;
                                actualiza_marcas_leyenda_resultados_tests();
                                test_fram_escritura1_ok_fallo = 0;
                            }
                            else
                            {
                                pictureBoxFRAMEscritura1.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                                contador_test = 2;
                                progressBarTestActual.Value = 25;
                                timeout_secuencia_test = 0;
                                test_fram_escritura1_ok_fallo = 1;
                            }
                            pictureBoxFRAMEscritura1.Visible = true;
                        }
                        else timeout_secuencia_test++;
                    }
                    if (contador_test == 2)
                    {
                        if (respuesta_test_fram != 1)
                        {
                            test_fram_tiempo_test_escritura2 = timeout_secuencia_test;

                            if (respuesta_test_fram != 2)
                            {
                                timer1.Enabled = false;
                                test = TipoTest.NO_TEST;
                                pictureBoxFRAMEscritura2.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                                leyenda_resultados_tests[(int)TipoTest.TEST_FRAM] = 0;
                                actualiza_marcas_leyenda_resultados_tests();
                                test_fram_escritura2_ok_fallo = 0;
                            }
                            else
                            {
                                pictureBoxFRAMEscritura2.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                                contador_test = 3;
                                progressBarTestActual.Value = 50;
                                timeout_secuencia_test = 0;
                                test_fram_escritura2_ok_fallo = 1;
                            }
                            pictureBoxFRAMEscritura2.Visible = true;
                        }
                        else timeout_secuencia_test++;
                    }
                    if (contador_test == 3)
                    {
                        if (respuesta_test_fram != 2)
                        {
                            test_fram_tiempo_test_bloqueo = timeout_secuencia_test;

                            if (respuesta_test_fram != 3)
                            {
                                timer1.Enabled = false;
                                test = TipoTest.NO_TEST;
                                pictureBoxFRAMBloqueo.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                                leyenda_resultados_tests[(int)TipoTest.TEST_FRAM] = 0;
                                actualiza_marcas_leyenda_resultados_tests();
                                test_fram_bloqueo_ok_fallo = 0;
                            }
                            else
                            {
                                pictureBoxFRAMBloqueo.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                                contador_test = 4;
                                progressBarTestActual.Value = 75;
                                timeout_secuencia_test = 0;
                                test_fram_bloqueo_ok_fallo = 1;
                            }
                            pictureBoxFRAMBloqueo.Visible = true;
                        }
                        else timeout_secuencia_test++;
                    }
                    if (contador_test == 4)
                    {
                        if (respuesta_test_fram != 3)
                        {
                            test_fram_tiempo_test_desbloqueo = timeout_secuencia_test;

                            timer1.Enabled = false;
                            test = TipoTest.NO_TEST;

                            if (respuesta_test_fram != 4)
                            {
                                pictureBoxFRAMDesbloqueo.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                                leyenda_resultados_tests[(int)TipoTest.TEST_FRAM] = 0;
                                test_fram_desbloqueo_ok_fallo = 0;
                            }
                            else
                            {
                                pictureBoxFRAMDesbloqueo.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                                leyenda_resultados_tests[(int)TipoTest.TEST_FRAM] = 1;
                                progressBarTestActual.Value = 100;
                                test_fram_desbloqueo_ok_fallo = 1;
                            }

                            pictureBoxFRAMDesbloqueo.Visible = true;
                            actualiza_marcas_leyenda_resultados_tests();
                        }
                        else timeout_secuencia_test++;
                    }
                    if (timeout_secuencia_test > 10 * 10) //10 segundos 
                    {
                        timer1.Enabled = false;
                        test = TipoTest.NO_TEST;
                        MessageBox.Show("Test abortado: Duracion excesiva", "Prueba de memoria FRAM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case TipoTest.TEST_RTC:
                    if (contador_test == 0)
                    {
                        progressBarTestActual.Value = 0;

                        test_rtc_puesta_en_hora_ok_fallo = -1;
                        pictureBoxPuestaEnHoraRTC.Visible = false;
                        leyenda_resultados_tests[(int)TipoTest.TEST_RTC] = -1;
                        actualiza_marcas_leyenda_resultados_tests();

                        DateTime dateTime;
                        dateTime = DateTime.Now;
                        respuesta_escrtc = 0;
                        buffer_tx += "escrtc " + dateTime.ToString("HH:mm:ss-dd/MM/yy") + "\r";
                        contador_comandos++;

                        contador_test = 1;
                        timeout_secuencia_test = 0;
                    }
                    if (contador_test == 1)
                    {
                        if (respuesta_escrtc == 1)
                        {
                            test_rtc_puesta_en_hora_ok_fallo = 1;
                            pictureBoxPuestaEnHoraRTC.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                            pictureBoxPuestaEnHoraRTC.Visible = true;

                            estado_en_hora = false;
                            progressBarTestActual.Value = 100;

                            timer1.Enabled = false;
                            test = TipoTest.NO_TEST;
                        }
                        else
                        {
                            timeout_secuencia_test++;

                            if (timeout_secuencia_test > 5 * 10) //5 segundos 
                            {
                                timer1.Enabled = false;
                                test = TipoTest.NO_TEST;

                                test_rtc_puesta_en_hora_ok_fallo = 0;
                                pictureBoxPuestaEnHoraRTC.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                                pictureBoxPuestaEnHoraRTC.Visible = true;

                                MessageBox.Show("Test abortado: Duracion excesiva", "Prueba del Real Time Clock (RTC)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    break;
                case TipoTest.NO_TEST:

                    break;
            }


        }

        void actualiza_medias_ADCs()
        {
            double medida;

            for (int i = 0; i < 16; i++)
            {
                medida = medias_adcs[i];
                switch (i)
                {
                    case 0: // CON23: Temperatura ambiente (placa 09015_2020_02_03, - 55ºC a 100ºC) -- CANAL A0
                        medida = medida * (20.0 / 1.8) * (1.5 / 2047.0);
                        medida = Math.Round(medida * 10000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCA0.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V";
                        break;
                    case 1: // CON18: Humedad relativa ambiente (placa 09015_2023_02_01) -- CANAL B0
                        medida = medida * (20.0 / 2.49) * (1.5 / 2047.0);
                        medida = Math.Round(medida * 10000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCB0.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V";
                        break;
                    case 2: // CON11: Tension AC compuesta Vrs -- CANAL A1
                        medida = medida * (20.0 / (5.36 * 147.0)) * (1.5 / 2047.0);
                        medida = Math.Round(medida * 10000000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCA1.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "mA";
                        break;
                    case 3: // CON5: Corriente AC Ir (HAT-1500S, 1350Arms x 1.3) -- CANAL B1
                        medida = medida * (20.0 / 4.42) * (1.5 / 2047.0);
                        medida = Math.Round(medida * 10000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCB1.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V";
                        break;
                    case 4: // CON24: Temperatura ambiente (placa 09015_2020_02_03, - 55ºC a 100ºC) -- CANAL A2
                        medida = medida * (20.0 / 1.8) * (1.5 / 2047.0);
                        medida = Math.Round(medida * 10000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCA2.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V";
                        break;
                    case 5: // CON19: Humedad relativa ambiente (placa 09015_2023_02_01) -- CANAL B2
                        medida = medida * (20.0 / 2.49) * (1.5 / 2047.0);
                        medida = Math.Round(medida * 10000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCB2.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V";
                        break;
                    case 6: // CON10: Tension AC compuesta Vst ---------------- CANAL A3
                        medida = medida * (20.0 / (5.36 * 147.0)) * (1.5 / 2047.0);
                        medida = Math.Round(medida * 10000000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCA3.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "mA";
                        break;
                    case 7: // CON3: Corriente AC Is (HAT-1500S, 1350Arms x 1.3) ---------------- CANAL B3
                        medida = medida * (20.0 / 4.42) * (1.5 / 2047.0);
                        medida = Math.Round(medida * 10000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCB3.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V";
                        break;
                    case 8: // CON25: Vdc2 (sensor LV-25) ---------------- CANAL A4
                        medida = medida * (20.0 / (5.36 * 147.0)) * (1.5 / 2047.0);
                        medida = Math.Round(medida * 10000000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCA4.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "mA";
                        break;
                    case 9: // CON20: Idc2 (HAX-2000). Imax=2080A (1600A x 1.3) ---------------- CANAL B4
                        medida = medida * (20.0 / 6.81) * (1.5 / 2047.0);
                        medida = Math.Round(medida * 10000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCB4.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V";
                        break;
                    case 10: // CON9: Presión ambiente (4-20mA) ---------------- CANAL A5
                        medida = (medida + 2047.0) * (1.0 / 120.0) * (3.0 / 4095.0);
                        medida = Math.Round(medida * 10000000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCA5.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "mA";
                        break;
                    case 11: // CON2: Corriente AC It (HAT-1500S, 1350Arms x 1.3) ---------------- CANAL B5
                        medida = medida * (20.0 / 4.42) * (1.5 / 2047.0);
                        medida = Math.Round(medida * 10000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCB5.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V";
                        break;
                    case 12: // CON26: Vdc1 (sensor DVL-1500) ---------------- CANAL A6
                        medida = medida * (20.0 / (3.74 * 100.0)) * (1.5 / 2047.0);
                        medida = Math.Round(medida * 10000000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCA6.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "mA";
                        break;
                    case 13: // CON21: Idc1 (HAX-2000). Imax=2080A (1600A x 1.3) ---------------- CANAL B6
                        medida = medida * (20.0 / 6.81) * (1.5 / 2047.0);
                        medida = Math.Round(medida * 10000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCB6.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V";
                        break;
                    case 14: // CON6: Presión ambiente (4-20mA) ---------------- CANAL A7
                        medida = (medida + 2047.0) * (1.0 / 120.0) * (3.0 / 4095.0);
                        medida = Math.Round(medida * 10000000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCA7.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "mA";
                        break;
                    case 15: // CON1: Libre ---------------- CANAL B7
                        medida = medida * (20.0 / 2.49) * (1.5 / 2047.0);
                        medida = Math.Round(medida * 10000.0, MidpointRounding.AwayFromZero);
                        medida /= 10000.0;
                        labelMediaADCB7.Text = medida.ToString(System.Globalization.CultureInfo.InvariantCulture) + "V";
                        break;
                    default:
                        break;
                }

                medias_adcs_vi[i] = medida;
            }
        }

        private DateTime hora_fecha_dsp_a_datetime(string hora_fecha)
        {
            string s_hora = hora_fecha.Substring(0, hora_fecha.IndexOf(":"));
            hora_fecha = hora_fecha.Substring(hora_fecha.IndexOf(":") + 1);
            string s_minuto = hora_fecha.Substring(0, hora_fecha.IndexOf(":"));
            hora_fecha = hora_fecha.Substring(hora_fecha.IndexOf(":") + 1);
            string s_segundo = hora_fecha.Substring(0, hora_fecha.IndexOf("-"));
            hora_fecha = hora_fecha.Substring(hora_fecha.IndexOf("-") + 1);
            string s_dia = hora_fecha.Substring(0, hora_fecha.IndexOf("/"));
            hora_fecha = hora_fecha.Substring(hora_fecha.IndexOf("/") + 1);
            string s_mes = hora_fecha.Substring(0, hora_fecha.IndexOf("/"));
            hora_fecha = hora_fecha.Substring(hora_fecha.IndexOf("/") + 1);
            string s_anual = hora_fecha;

            int segundo = int.Parse(s_segundo);
            int minuto = int.Parse(s_minuto);
            int hora = int.Parse(s_hora);
            int dia = int.Parse(s_dia);
            int mes = int.Parse(s_mes);
            int anual = int.Parse(s_anual);

            DateTime dt = new DateTime(anual, mes, dia, hora, minuto, segundo);
            return dt;
        }

        private bool en_hora(DateTime hora_pc, string s_hora_dsp)
        {
            DateTime hora_pc_century0 = new DateTime(hora_pc.Year - ((hora_pc.Year / 100) * 100), hora_pc.Month,
                hora_pc.Day, hora_pc.Hour, hora_pc.Minute, hora_pc.Second);
            DateTime hora_dsp = hora_fecha_dsp_a_datetime(s_hora_dsp);

            TimeSpan diferencia = new TimeSpan(hora_pc_century0.Ticks - hora_dsp.Ticks);
            if (Math.Abs(diferencia.TotalSeconds) > 2) return false;
            else return true;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            // Indicacion de entradas digitales
            CheckBox[] checkBoxesInputs = { checkBoxIn0, checkBoxIn1, checkBoxIn2, checkBoxIn3,
                                      checkBoxIn4, checkBoxIn5, checkBoxIn6, checkBoxIn7,
                                      checkBoxIn8, checkBoxIn9, checkBoxIn10, checkBoxIn11,
                                      checkBoxIn12, checkBoxIn13, checkBoxIn14, checkBoxIn15};

            CheckBox[] checkBoxesErrores = { checkBoxErrorOVTempR, checkBoxErrorOVTempS, checkBoxErrorOVTempT,
                checkBoxErrorTopR, checkBoxErrorTopS, checkBoxErrorTopT,
                checkBoxErrorBotR, checkBoxErrorBotS, checkBoxErrorBotT };

            TextBox[] textNTCs = { textNTC0, textNTC1, textNTC2, textNTC3, textNTC4 };

            labelContadorComandos.Text = "Com. pend.: " + contador_comandos;
            labelTimeout.Text = "Time out: " + timeout_esperarespuesta;
            labelComandoActual.Text = "Comando actual: " + comando_actual;

            if (contador_comandos > 0)
            {
                timeout_esperarespuesta++;
                if (timeout_esperarespuesta > 25)
                {

                    timeout_esperarespuesta = 0;
                    buffer_tx += "ping\r";
                    contador_comandos++;
                    try
                    {
                        if(serialPort1.IsOpen) serialPort1.DiscardInBuffer();
                    }
                    catch(Exception)
                    { }
                }
            }
            else timeout_esperarespuesta = 0;

            if ((contador_comandos == 0) && serialPort1.IsOpen)
            {
                if (buffer_tx.Length < 100)
                {
                    switch (comando_actual)
                    {
                        case 0:
                            buffer_tx += "input\r";
                            break;
                        case 1:
                            buffer_tx += "outputreles " + (output & 0xffff) + " " + ((output ^ (output>>8)) & 0xff) + "\r";
                            break;
                        case 2:
                            buffer_tx += "leeNTC " + leeNTC + "\r";
                            leeNTC++;
                            if (leeNTC > 4) leeNTC = 0;
                            break;
                        case 3:
                            buffer_tx += "supervisores\r";
                            break;
                        case 4:
                            buffer_tx += "rpmturbinas\r";
                            break;
                        case 5:
                            buffer_tx += "dutyturbina 1 " + dutyturbina1 + "\r";
                            break;
                        case 6:
                            buffer_tx += "dutyturbina 2 " + dutyturbina2 + "\r";
                            break;
                        case 7:
                            buffer_tx += "dutyturbina 3 " + dutyturbina3 + "\r";
                            break;
                        case 8:
                            buffer_tx += "errores\r";
                            break;
                        case 9:
                            if (buffer_tx_rs422.Length > 0)
                            {
                                if (buffer_tx_rs422.Length < 16)
                                {
                                    buffer_tx += "rs422" + buffer_tx_rs422 + "\r";
                                    buffer_tx_rs422 = "";
                                }
                                else
                                {
                                    buffer_tx += "rs422" + buffer_tx_rs422.Substring(0, 16) + "\r";
                                    buffer_tx_rs422 = buffer_tx_rs422.Substring(16);
                                }

                            }
                            else contador_comandos--;
                            break;
                        case 10:
                            buffer_tx += "leeadc " + canaladc + "\r";
                            canaladc++;
                            if (canaladc >= 16) canaladc = 0;
                            panelCanal0.Refresh();
                            panelCanal1.Refresh();
                            panelCanal2.Refresh();
                            panelCanal3.Refresh();
                            panelCanal4.Refresh();
                            panelCanal5.Refresh();
                            panelCanal6.Refresh();
                            panelCanal7.Refresh();
                            panelCanal8.Refresh();
                            panelCanal9.Refresh();
                            panelCanal10.Refresh();
                            panelCanal11.Refresh();
                            panelCanal12.Refresh();
                            panelCanal13.Refresh();
                            panelCanal14.Refresh();
                            panelCanal15.Refresh();
                            break;
                        case 11:
                            buffer_tx += "disparos " + (disparos & 7) + "\r";
                            break;
                        case 12:
                            buffer_tx += "outputleds " + (outputleds & 0x1f) + "\r";
                            break;
                        case 13:
                            buffer_tx += "enabletx_rs422 " + enabletx_rs422 + "\r";
                            break;
                        case 14:
                            buffer_tx += "enablerx_rs422 " + enablerx_rs422 + "\r";
                            break;
                        case 15:
                            buffer_tx += "canatx " + (canatx & 1) + "\r";
                            break;
                        case 16:
                            buffer_tx += "canarx\r";
                            break;
                        case 17:
                            buffer_tx += "canbtx " + (canbtx & 1) + "\r";
                            break;
                        case 18:
                            buffer_tx += "canbrx\r";
                            break;
                        case 19:
                            buffer_tx += "leertc\r";
                            break;
                    }
                    comando_actual++;
                    if ((comando_actual == 10) && (tabControl1.SelectedIndex != 2)) comando_actual++;
                    if (comando_actual > 19) comando_actual = 0;
                    if ((tabControl1.SelectedIndex == 2) && (test == TipoTest.NO_TEST)) comando_actual = 10;
                    contador_comandos++;

                    if(leyenda_resultados_tests[(int)TipoTest.TEST_LEER_VERSIONES] != 1)
                    {
                        buffer_tx += "ping\r";
                        contador_comandos++;
                    }
                }
            }

            periodo_peticion_ping++;
            if (periodo_peticion_ping >= 1000) // perido de peticion de ping 1 segundo
            {
                periodo_peticion_ping = 0;
                buffer_tx += "ping\r";
                contador_comandos++;
            }

            try
            {
                if (serialPort1.IsOpen)
                {
                    if (serialPort1.BytesToWrite == 0)
                    {
                        if (buffer_tx.Length >= 16)
                        {
                            serialPort1.Write(buffer_tx.Substring(0, 16));
                            buffer_tx = buffer_tx.Substring(16);
                        }
                        else
                        {
                            serialPort1.Write(buffer_tx.Substring(0, buffer_tx.Length));
                            buffer_tx = "";
                        }

                        //serialPort1.WriteLine("input\r");
                        //serialPort1.WriteLine("leeNTC " + leeNTC + "\r");
                        //serialPort1.WriteLine("supervisores\r");
                        //serialPort1.WriteLine("rpmturbinas\r");
                        //serialPort1.WriteLine("dutyturbina 1 " + numericUpDownTurbina1.Value + "\r");
                        //serialPort1.WriteLine("dutyturbina 2 " + numericUpDownTurbina2.Value + "\r");
                        //serialPort1.WriteLine("dutyturbina 3 " + numericUpDownTurbina3.Value + "\r");
                    }
                }
                else if(!serialPort2.IsOpen)
                    botonConectarDesconectar.Text = "Conectar";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error USB-Serie");
            }

            for (int i = 0; i < 5; i++)
            {
                textNTCs[i].Text = resistenciasNTC[i];
            }

            for (int i = 0; i < 16; i++)
            {
                if ((input & (1 << i)) != 0)
                {
                    checkBoxesInputs[i].Checked = true;
                }
                else
                {
                    checkBoxesInputs[i].Checked = false;
                }
            }

            for (int i = 0; i < 9; i++)
            {
                checkBoxesErrores[i].Checked = ((errores & (1 << i)) != 0);
            }

            checkBoxRXSigmaDelta.Checked = ((errores & 0x8000) != 0);

            if (flag_cambia_versiones) reset_informes_tests(false);

            string cadena_USB_UART = "";
            if ((cadena_USB_UART_VID != "") || (cadena_USB_UART_PID != "") || (cadena_USB_UART_SN != ""))
                cadena_USB_UART = "USB-UART-VID:" + cadena_USB_UART_VID + "\r\nUSB-UART-PID:" + cadena_USB_UART_PID +
                    "\r\nUSB-UART-SN:" + cadena_USB_UART_SN + "\r\n";

            textBoxPing.Text = cadena_USB_UART + cadena_versiones;

            // Supervisores de temperaturas, tensiones y corriente
            textBoxTemp1.Text = temperatura1.ToString();
            textBoxTemp2.Text = temperatura2.ToString();
            textBoxVin.Text = vin.ToString();
            textBoxVdsp.Text = vdsp.ToString();
            textBoxVfo.Text = vfo.ToString();
            textBoxVreles.Text = vreles.ToString();
            textBoxVbat.Text = vbat.ToString();
            textBoxV3_3V.Text = v3_3v.ToString();
            textBoxIin.Text = iin.ToString();

            // Medidas de velocidad de turbina, en RPMs
            textBoxTurbina1.Text = rpm1.ToString();
            textBoxTurbina2.Text = rpm2.ToString();
            textBoxTurbina3.Text = rpm3.ToString();

            // Recepcion del puerto RS422
            if (buffer_rx_rs422.Length > 0)
            {
                textBoxRS422.Text += buffer_rx_rs422;
                textBoxRS422.SelectionStart = textBoxRS422.TextLength;
                textBoxRS422.ScrollToCaret();
                buffer_rx_rs422 = "";
            }

            // Recepciones de los 2 puertos CAN
            if (canarx == 0) checkBoxCANARX.Checked = false;
            else checkBoxCANARX.Checked = true;

            if (canbrx == 0) checkBoxCANBRX.Checked = false;
            else checkBoxCANBRX.Checked = true;

            if (test == TipoTest.NO_TEST)
                pictureBoxWorking.Visible = false;
            else
                pictureBoxWorking.Visible = true;

            // valores medios medidos por los canales adc del dsp
            actualiza_medias_ADCs();

            // Actualiza hora actual del PC y RTC 
            DateTime dateTime;
            dateTime = DateTime.Now;
            textBoxHoraPC.Text = dateTime.ToString("HH:mm:ss-dd/MM/yy");
            textBoxHoraDSP.Text = cadena_hora_dsp;

            if(respuesta_leertc == 1)
            {
                respuesta_leertc = 0;

                bool estado_en_hora_actual = en_hora(dateTime, cadena_hora_dsp);

                if(!estado_en_hora)
                {
                    test_rtc_duracion_en_hora = 0;

                    if (estado_en_hora_actual)
                    {
                        estado_en_hora = true;
                        hora_inicio_rtc_enhora = dateTime;
                        test_rtc_en_hora_ok_fallo = -1;
                        pictureBoxEnHoraRTC.Visible = false;
                    }
                    else
                    {
                        test_rtc_en_hora_ok_fallo = 0;
                        pictureBoxEnHoraRTC.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                        pictureBoxEnHoraRTC.Visible = true;
                    }
                }
                else
                {
                    if(!estado_en_hora_actual)
                    {
                        estado_en_hora = false;
                        test_rtc_duracion_en_hora = 0;
                        test_rtc_en_hora_ok_fallo = 0;
                        pictureBoxEnHoraRTC.Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                        pictureBoxEnHoraRTC.Visible = true;
                    }
                    else
                    {
                        long ticks = dateTime.Ticks - hora_inicio_rtc_enhora.Ticks;
                        TimeSpan ts = new TimeSpan(ticks);
                        test_rtc_duracion_en_hora = (int)ts.TotalSeconds;
                        if(test_rtc_duracion_en_hora > 5)
                        {
                            test_rtc_en_hora_ok_fallo = 1;
                            pictureBoxEnHoraRTC.Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                            pictureBoxEnHoraRTC.Visible = true;
                        }
                    }
                }

                int nueva_leyenda;
                if ((test_rtc_puesta_en_hora_ok_fallo == -1) || (test_rtc_en_hora_ok_fallo == -1))
                {
                    nueva_leyenda = -1;
                }
                else if ((test_rtc_puesta_en_hora_ok_fallo == 0) || (test_rtc_en_hora_ok_fallo == 0))
                {
                    nueva_leyenda = 0;
                }
                else
                {
                    nueva_leyenda = 1;
                }

                if(nueva_leyenda != leyenda_resultados_tests[(int)TipoTest.TEST_RTC])
                {
                    leyenda_resultados_tests[(int)TipoTest.TEST_RTC] = nueva_leyenda;
                    actualiza_marcas_leyenda_resultados_tests();
                }

                if (test_rtc_duracion_en_hora > 5) labelEnHoraRTC.Text = "> 5 seg";
                else labelEnHoraRTC.Text = test_rtc_duracion_en_hora + " seg";
            }

        }

        private void textBoxRefSupv_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (tb.Tag.Equals("RefTemp1"))
            {
                if (tb.Text.Length == 0)
                {
                    ref_temp1 = -1.0;
                }
                else
                {
                    ref_temp1 = double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture);
                }

                textBoxRefTemp2.Text = tb.Text;
            }
            else if (tb.Tag.Equals("RefTemp2"))
            {
                if (tb.Text.Length == 0)
                {
                    ref_temp2 = -1.0;
                }
                else
                {
                    ref_temp2 = double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            else if (tb.Tag.Equals("RefVin"))
            {
                if (tb.Text.Length == 0)
                {
                    ref_vin = -1.0;
                }
                else
                {
                    ref_vin = double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            else if (tb.Tag.Equals("RefIin"))
            {
                if (tb.Text.Length == 0)
                {
                    ref_iin = -1.0;
                }
                else
                {
                    ref_iin = double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            else if (tb.Tag.Equals("RefVbat"))
            {
                if (tb.Text.Length == 0)
                {
                    ref_vbat = -1.0;
                }
                else
                {
                    ref_vbat = double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture);
                }
            }

        }

        private void lee_versiones()
        {
            if ((test == TipoTest.NO_TEST) && serialPort1.IsOpen)
            {
                buffer_tx += "ping\r";
                contador_comandos++;
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string[] cad_parser = { "R_NTC", "input=", "FPGA_VER:", "temperaturas=", "tensiones=",
                "corriente_in=", "rpmturbinas=", "recepcion=", "errores=", "canal0=",
                "canal1=", "canal2=", "canal3=", "canal4=", "canal5=", "canal6=", "canal7=",
                "canal8=", "canal9=", "canal10=", "canal11=", "canal12=", "canal13=", "canal14=",
                "canal15=", "outputreles:ok", "dutyturbina:ok", "disparos:ok", "outputleds:ok",
                "enabletx_rs422:ok", "enablerx_rs422:ok", "canatx:ok", "canarx=", "canbtx:ok", "canbrx=",
                "testsram:ok", "testsram:fallo", "testfram:borrado", "testfram:escritura", "testfram:bloqueo",
                "testfram:ok", "testfram:fallo", "leertc=", "escrtc:ok", "ERROR DE INTERPRETE:"};

            buffer_rx += serialPort1.ReadExisting();

            for (int i = 0; i < buffer_rx.Length; i++)
            {
                for (int j = 0; j < cad_parser.Length; j++)
                {
                    if (buffer_rx.Length - i >= cad_parser[j].Length)
                    {
                        if (buffer_rx.Substring(i, cad_parser[j].Length).Equals(cad_parser[j]))
                        {
                            buffer_rx = buffer_rx.Substring(i);
                            i = 0;
                            switch (j)
                            {
                                case 0:
                                    if (buffer_rx.Contains("\r\n"))
                                    {
                                        int ntc = buffer_rx[5] - '0';
                                        if ((ntc >= 0) && (ntc <= 5))
                                        {
                                            resistenciasNTC[ntc] = buffer_rx.Substring(7, buffer_rx.IndexOf("\r\n") - 7);
                                            buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf("\r\n") + 2);
                                            contador_comandos--;
                                        }
                                        else
                                        {
                                            buffer_rx = buffer_rx.Substring(1);
                                        }

                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                                case 1:
                                    if (buffer_rx.Length >= 14)
                                    {
                                        input = Convert.ToInt32(buffer_rx.Substring(8, 4), 16);
                                        buffer_rx = buffer_rx.Substring(14);
                                        contador_comandos--;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                                case 2:
                                    if (buffer_rx.Contains("\r\n"))
                                    {
                                        string anterior_cadena_versiones = cadena_versiones;

                                        bool respuesta_ok = true;

                                        if (buffer_rx.Contains("FPGA_VER:0x") && buffer_rx.Contains("-"))
                                        {
                                            cadena_ver_fpga = buffer_rx.Substring(buffer_rx.IndexOf("FPGA_VER:0x"));
                                            cadena_ver_fpga = cadena_ver_fpga.Substring(cadena_ver_fpga.IndexOf("0x") + 2);
                                            cadena_ver_fpga = cadena_ver_fpga.Substring(0, cadena_ver_fpga.IndexOf("-"));
                                        }
                                        else respuesta_ok = false;

                                        if (buffer_rx.Contains("FIRMWARE_VER:") && buffer_rx.Contains("-"))
                                        {
                                            cadena_ver_firmware = buffer_rx.Substring(buffer_rx.IndexOf("FIRMWARE_VER:"));
                                            cadena_ver_firmware = cadena_ver_firmware.Substring(cadena_ver_firmware.IndexOf(":") + 1);
                                            cadena_ver_firmware = cadena_ver_firmware.Substring(0, cadena_ver_firmware.IndexOf("-"));
                                        }
                                        else respuesta_ok = false;

                                        if (buffer_rx.Contains("FPGA_ID_DNA:0x") && buffer_rx.Contains("-"))
                                        {
                                            cadena_id_dna = buffer_rx.Substring(buffer_rx.IndexOf("FPGA_ID_DNA:0x"));
                                            cadena_id_dna = cadena_id_dna.Substring(cadena_id_dna.IndexOf("0x") + 2);
                                            cadena_id_dna = cadena_id_dna.Substring(0, cadena_id_dna.IndexOf("-"));
                                        }
                                        else respuesta_ok = false;

                                        if (buffer_rx.Contains("DSP_PARTID:0x") && buffer_rx.Contains("-"))
                                        {
                                            cadena_dsp_partid = buffer_rx.Substring(buffer_rx.IndexOf("DSP_PARTID:0x"));
                                            cadena_dsp_partid = cadena_dsp_partid.Substring(cadena_dsp_partid.IndexOf("0x") + 2);
                                            cadena_dsp_partid = cadena_dsp_partid.Substring(0, cadena_dsp_partid.IndexOf("-"));
                                        }
                                        else respuesta_ok = false;

                                        if (buffer_rx.Contains("DSP_CLASSID:0x") && buffer_rx.Contains("-"))
                                        {
                                            cadena_dsp_classid = buffer_rx.Substring(buffer_rx.IndexOf("DSP_CLASSID:0x"));
                                            cadena_dsp_classid = cadena_dsp_classid.Substring(cadena_dsp_classid.IndexOf("0x") + 2);
                                            cadena_dsp_classid = cadena_dsp_classid.Substring(0, cadena_dsp_classid.IndexOf("-"));
                                        }
                                        else respuesta_ok = false;

                                        if (buffer_rx.Contains("DSP_REVID:0x") && buffer_rx.Contains("-"))
                                        {
                                            cadena_dsp_revid = buffer_rx.Substring(buffer_rx.IndexOf("DSP_REVID:0x"));
                                            cadena_dsp_revid = cadena_dsp_revid.Substring(cadena_dsp_revid.IndexOf("0x") + 2);
                                            cadena_dsp_revid = cadena_dsp_revid.Substring(0, cadena_dsp_revid.IndexOf("-"));
                                        }
                                        else respuesta_ok = false;

                                        buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf("\r\n") + 2);
                                        contador_comandos--;

                                        if (respuesta_ok == true)
                                        {
                                            cadena_versiones = "FPGA_VER:0x" + cadena_ver_fpga + "\r\n" +
                                                               "FIRMWARE_VER:" + cadena_ver_firmware + "\r\n" +
                                                               "FPGA_ID_DNA:0x" + cadena_id_dna + "\r\n" +
                                                               "DSP_PARTID:0x" + cadena_dsp_partid + "\r\n" +
                                                               "DSP_CLASSID:0x" + cadena_dsp_classid + "\r\n" +
                                                               "DSP_REVID:0x" + cadena_dsp_revid;
                                            if (anterior_cadena_versiones != cadena_versiones) flag_cambia_versiones = true;
                                            leyenda_resultados_tests[(int)TipoTest.TEST_LEER_VERSIONES] = 1;
                                        }
                                        else
                                        {
                                            //cadena_versiones = "";
                                            leyenda_resultados_tests[(int)TipoTest.TEST_LEER_VERSIONES] = 0;
                                        }

                                        actualiza_marcas_leyenda_resultados_tests();
                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                                case 3:
                                    if (buffer_rx.Contains("\r\n"))
                                    {
                                        string temp = buffer_rx.Substring(cad_parser[3].Length, buffer_rx.IndexOf(",") - cad_parser[3].Length);
                                        temperatura1 = double.Parse(temp, System.Globalization.CultureInfo.InvariantCulture);
                                        temp = buffer_rx.Substring(buffer_rx.IndexOf(",") + 1, buffer_rx.IndexOf("\r\n") - buffer_rx.IndexOf(",") - 1);
                                        temperatura2 = double.Parse(temp, System.Globalization.CultureInfo.InvariantCulture);
                                        buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf("\r\n") + 2);
                                        contador_comandos--;
                                    }
                                    break;
                                case 4:
                                    if (buffer_rx.Contains("\r\n"))
                                    {
                                        string v = buffer_rx.Substring(cad_parser[4].Length, buffer_rx.IndexOf(",") - cad_parser[4].Length);
                                        vin = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture);
                                        buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf(",") + 1);
                                        v = buffer_rx.Substring(0, buffer_rx.IndexOf(","));
                                        vreles = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture);
                                        buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf(",") + 1);
                                        v = buffer_rx.Substring(0, buffer_rx.IndexOf(","));
                                        vdsp = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture);
                                        buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf(",") + 1);
                                        v = buffer_rx.Substring(0, buffer_rx.IndexOf(","));
                                        v3_3v = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture);
                                        buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf(",") + 1);
                                        v = buffer_rx.Substring(0, buffer_rx.IndexOf(","));
                                        vfo = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture);
                                        buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf(",") + 1);
                                        v = buffer_rx.Substring(0, buffer_rx.IndexOf("\r\n"));
                                        vbat = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture);
                                        buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf("\r\n") + 2);
                                        contador_comandos--;
                                    }
                                    break;
                                case 5:
                                    if (buffer_rx.Contains("\r\n"))
                                    {
                                        string corriente = buffer_rx.Substring(cad_parser[5].Length, buffer_rx.IndexOf("\r\n") - cad_parser[5].Length);
                                        iin = Math.Round(double.Parse(corriente, System.Globalization.CultureInfo.InvariantCulture)) / 1000.0;
                                        buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf("\r\n") + 2);
                                        contador_comandos--;
                                    }
                                    break;
                                case 6:
                                    if (buffer_rx.Contains("\r\n"))
                                    {
                                        string rps = buffer_rx.Substring(cad_parser[6].Length, buffer_rx.IndexOf(",") - cad_parser[6].Length);
                                        rpm1 = double.Parse(rps, System.Globalization.CultureInfo.InvariantCulture);
                                        buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf(",") + 1);
                                        rps = buffer_rx.Substring(0, buffer_rx.IndexOf(","));
                                        rpm2 = double.Parse(rps, System.Globalization.CultureInfo.InvariantCulture);
                                        buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf(",") + 1);
                                        rps = buffer_rx.Substring(0, buffer_rx.IndexOf("\r\n"));
                                        rpm3 = double.Parse(rps, System.Globalization.CultureInfo.InvariantCulture);
                                        buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf("\r\n") + 2);
                                        contador_comandos--;
                                    }
                                    break;
                                case 7:
                                    if (buffer_rx.Contains("\r\n"))
                                    {
                                        string recepcion = buffer_rx.Substring(cad_parser[7].Length, buffer_rx.IndexOf("\r\n") - cad_parser[7].Length);
                                        buffer_rx_rs422 += recepcion;
                                        buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf("\r\n") + 2);
                                        contador_comandos--;
                                    }
                                    break;
                                case 8:
                                    if (buffer_rx.Length >= 16)
                                    {
                                        errores = Convert.ToInt32(buffer_rx.Substring(10, 4), 16);
                                        buffer_rx = buffer_rx.Substring(16);
                                        contador_comandos--;
                                    }
                                    break;
                                case 9:
                                case 10:
                                case 11:
                                case 12:
                                case 13:
                                case 14:
                                case 15:
                                case 16:
                                case 17:
                                case 18:
                                case 19:
                                case 20:
                                case 21:
                                case 22:
                                case 23:
                                case 24:
                                    if (buffer_rx.Length >= cad_parser[j].Length + 20 * 4 + 2)
                                    {
                                        double media = 0.0;
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length);
                                        for (int k = 0; k < num_samples; k++)
                                        {
                                            voltaje[j - 9, k] = Convert.ToInt32(buffer_rx.Substring(k * 4, 4), 16);
                                            media += (voltaje[j - 9, k] - 2047);
                                        }
                                        media = media / num_samples;
                                        medias_adcs[j - 9] = media;
                                        buffer_rx = buffer_rx.Substring(20 * 2 + 2);
                                        contador_comandos--;
                                    }
                                    break;
                                case 25:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                        respuesta_output_ok = 1;
                                    }
                                    break;
                                case 26:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                        respuesta_dutyturbina_ok = 1;
                                    }
                                    break;
                                case 27:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                        respuesta_disparos_ok = 1;
                                    }
                                    break;
                                case 28:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                        respuesta_outputleds_ok = 1;
                                    }
                                    break;
                                case 29:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                        respuesta_enabletx_rs422_ok = 1;
                                    }
                                    break;
                                case 30:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                        respuesta_enablerx_rs422_ok = 1;
                                    }
                                    break;
                                case 31:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                        respuesta_canatx_ok = 1;
                                    }
                                    break;
                                case 32:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 3))
                                    {
                                        canarx = Convert.ToInt32(buffer_rx.Substring(cad_parser[j].Length, 1), 10);
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 3);
                                        contador_comandos--;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                                case 33:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                        respuesta_canbtx_ok = 1;
                                    }
                                    break;
                                case 34:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 3))
                                    {
                                        canbrx = Convert.ToInt32(buffer_rx.Substring(cad_parser[j].Length, 1), 10);
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 3);
                                        contador_comandos--;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                                case 35:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        respuesta_test_sram = 1;
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                                case 36:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        respuesta_test_sram = 0;
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                                case 37:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        respuesta_test_fram = 1;
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                                case 38:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        respuesta_test_fram = 2;
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                                case 39:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        respuesta_test_fram = 3;
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                                case 40:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        respuesta_test_fram = 4;
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                                case 41:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        respuesta_test_fram = 0;
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                                case 42:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + "00:00:00-01/01/01".Length + 2))
                                    {
                                        respuesta_leertc = 1;
                                        cadena_hora_dsp = buffer_rx.Substring(cad_parser[j].Length, + "00:00:00-01/01/01".Length);
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + "00:00:00-01/01/01".Length + 2);
                                        contador_comandos--;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                                case 43:
                                    if (buffer_rx.Length >= (cad_parser[j].Length + 2))
                                    {
                                        respuesta_escrtc = 1;
                                        buffer_rx = buffer_rx.Substring(cad_parser[j].Length + 2);
                                        contador_comandos--;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                                case 44: // Error de interprete
                                    if ((buffer_rx.Length > cad_parser[j].Length) && (buffer_rx.Contains("\r\n")))
                                    {
                                        buffer_rx = buffer_rx.Substring(buffer_rx.IndexOf("\r\n") + 2);
                                        contador_comandos--;
                                        buffer_tx += "ping\r";
                                        contador_comandos++;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            if (contador_comandos < 0) contador_comandos = 0;


        }

        private void textNumerico_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void textNTCref_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;


            int ntc = Int32.Parse((string)tb.Tag);
            if (tb.Text.Length > 0)
            {
                refNTCs[ntc] = double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                refNTCs[ntc] = -1.0;
            }
        }

        private void testNTC()
        {
            for (int i = 0; i < 5; i++)
            {
                double error = Math.Abs(test_ntcs_medidas[i] - test_ntcs_referencias[i]) / test_ntcs_referencias[i];
                if (error > (test_ntcs_tolerancias[i] / 100.0))
                {
                    marcasNTCs[i].Image = TestFuncionalBRD15001.Properties.Resources.Red_Cross_300px;
                    test_ntcs_ok_fallo[i] = 0;
                }
                else
                {
                    marcasNTCs[i].Image = TestFuncionalBRD15001.Properties.Resources.Green_Tick_300px;
                    test_ntcs_ok_fallo[i] = 1;
                }
            }

            foreach (PictureBox pb in marcasNTCs)
            {
                pb.Visible = true;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            TextBox[] textBox_ntcs_tolerancias = { textBoxToleranciaNTC0, textBoxToleranciaNTC1, textBoxToleranciaNTC2,
                                                   textBoxToleranciaNTC3, textBoxToleranciaNTC4};

            // test de las medidas de NTC comparando las medidas de la placa con los valores
            // configurados como referencia

            if (!serialPort1.IsOpen) return;

            // Comprobar que todos los valores de referencia y tolerancias se han definido
            for (int i = 0; i < 5; i++)
            {
                if ((refNTCs[i] == -1.0) || (textBox_ntcs_tolerancias[i].Text.Length == 0))
                {
                    MessageBox.Show("Este test compara las medidas de resistencia de la placa con los valores conectados a los conectores CON27 a CON31.\n" + 
                        "Antes de ejecutar el test deben definirse los 5 valores de referencia.\n" + 
                        "Los valores ohmicos de estos deben estar comprendidos en el rango 500 Ohmios a 100 KOhmios.",
                        "Test de medidas de resistencias NTCs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            // Comprobar que los valores de referencia se encuentran en rangos admitidos
            for (int i = 0; i < 5; i++)
            {
                if ((refNTCs[i] < 500.0) || (refNTCs[i] > 100000.0))
                {
                    MessageBox.Show("Los valores ohmicos de las referecias usadas deben estar comprendidos en el rango 500 Ohmios a 100 KOhmios.", "Test de medidas de resistencias NTCs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                test_ntcs_tolerancias[i] = double.Parse(textBox_ntcs_tolerancias[i].Text, System.Globalization.CultureInfo.InvariantCulture);
                test_ntcs_referencias[i] = refNTCs[i];
                try
                {
                    test_ntcs_medidas[i] = double.Parse(resistenciasNTC[i], System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Test de medidas de resistencias NTCs");
                    return;
                }
            }

            leyenda_resultados_tests[(int)TipoTest.TEST_NTCS] = -1;
            actualiza_marcas_leyenda_resultados_tests();

            testNTC();

            for (int i = 0, contador_oks = 0; i < 5; i++)
            {
                if (test_ntcs_ok_fallo[i] == 1) contador_oks++;
                if ((i == 4) && (contador_oks == 5)) leyenda_resultados_tests[(int)TipoTest.TEST_NTCS] = 1;
                else leyenda_resultados_tests[(int)TipoTest.TEST_NTCS] = 0;
            }

            actualiza_marcas_leyenda_resultados_tests();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (test != TipoTest.NO_TEST)
            {
                timer1.Enabled = false;
                DialogResult res = MessageBox.Show("¿Desea cancelar el test actual?", "Carga de firmware de test", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.No)
                {
                    timer1.Enabled = true;
                    return;
                }
                else test = TipoTest.NO_TEST;
            }

            string nombreCOM = comboBoxCOMs.GetItemText(comboBoxCOMs.SelectedItem);
            try
            {
                serialPort1.Close();
                serialPort2.Close();

                serialPort2.PortName = nombreCOM;
                serialPort2.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Carga de firmware de test");
                return;
            }
            botonConectarDesconectar.Text = "Desconectar";


            MessageBox.Show("Pulsar el pulsador S1 durante 3 segundos para activar modo mantenimiento\r\n(Secuencia S.O.S en LED3)", "Carga de firmware de test");

            // autobaud
            byte[] byte_buffer = new byte[1024];
            byte b;
            int timeout1 = 100;
            int timeout2 = 0;
            byte_buffer[0] = (byte)'a';
            while (timeout1 > 0)
            {
                try
                {

                    serialPort2.Write(byte_buffer, 0, 1);
                    timeout2 = 1000000;
                    while ((serialPort2.BytesToRead == 0) && (timeout2 > 0)) timeout2--;
                    if (serialPort2.BytesToRead == 0) timeout1 = 0;
                    else
                    {
                        b = (byte)serialPort2.ReadByte();
                        if (b == (byte)'a') break;
                        timeout1--;
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Carga de firmware de test");
                    return;
                }
            }
            if(timeout1 == 0)
            {
                try
                {
                    serialPort2.Close();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Carga de firmware de test");
                }
                MessageBox.Show("Error de ajuste automatico de baudios", "Carga de firmware de test");
                return;
            }

            StreamReader reader;
            try
            {
                reader = new StreamReader(@"Pruebas_funcionales_brd15001.a00");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Carga de firmware de test");
                return;
            }

            Form2 form_progreso = new Form2();
            form_progreso.Show();
            long tam = reader.BaseStream.Length;  

            while (!reader.EndOfStream)
            {
                string linea = reader.ReadLine();

                long pos = (100 * reader.BaseStream.Position) / tam;
                form_progreso.progressBarCargaFirmware.Value = (int)pos;

                string[] hexSeparados = linea.Split(' ');
                int cuentaBytes = 0;
                byte valor = 0;

                foreach(string hex in hexSeparados)
                {
                    if (hex.Length == 0) continue;
                    try
                    {
                        valor = Convert.ToByte(hex, 16);
                    }
                    catch
                    {
                        //MessageBox.Show(ex.Message, "Bootloading Firmware de test");
                        continue;
                    }
                    byte_buffer[cuentaBytes] = valor;
                    cuentaBytes++;
                    if (cuentaBytes == 1024)
                    {
                        try
                        {
                            serialPort2.Write(byte_buffer, 0, 1024);
                            while (serialPort2.BytesToWrite > 0) ;
                        }
                        catch(Exception ex)
                        {
                            form_progreso.Close();
                            form_progreso.Dispose();

                            MessageBox.Show(ex.Message, "Carga de firmware de test");
                            return;
                        }
                    }
                }

                if (cuentaBytes > 0)
                {
                    try
                    {
                        serialPort2.Write(byte_buffer, 0, cuentaBytes);
                        while (serialPort2.BytesToWrite > 0) ;
                    }
                    catch (Exception ex)
                    {
                        form_progreso.Close();
                        form_progreso.Dispose();

                        MessageBox.Show(ex.Message, "Carga de firmware de test");
                        return;
                    }
                }

            }

            try
            {
                serialPort2.Close();
                lee_pid_vid_serial_usb_uart(nombreCOM);

                serialPort1.PortName = nombreCOM;
                serialPort1.Open();

                buffer_tx = "ping\r";
                contador_comandos = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Carga de firmware de test");
            }

            form_progreso.Close();
            form_progreso.Dispose();

        }




    }
}