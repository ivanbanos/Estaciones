using FactoradorEstacionesModelo.Objetos;
using FacturadorEstacionesRepositorio;
using Microsoft.Extensions.Options;
using Modelo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FacturadorEstacionesPOSWinForm
{
    public partial class Form3 : Form
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly InfoEstacion _infoEstacion;
        private Isla[] islas;
        public Form3(IEstacionesRepositorio estacionesRepositorio, InfoEstacion infoEstacion)
        {
            _estacionesRepositorio = estacionesRepositorio;
            _infoEstacion = infoEstacion;
            InitializeComponent();
            islas = _estacionesRepositorio.getIslas().ToArray();
            comboBox3.Items.Clear();
            comboBox3.Items.AddRange(islas);
            textBox1.PlaceholderText = "Código";
        }

        private void abrir_Click(object sender, EventArgs e)
        {
            try
            {

                var isla = (Isla)comboBox3.SelectedItem;
                var codigo = textBox1.Text;
                _estacionesRepositorio.CerrarTurno(isla, codigo, 0);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception)
            {
                this.DialogResult = DialogResult.Abort;
                MessageBox.Show("Código de empleado no existe o turno no abierto");
            }
            //var respuesta = send_cmd($"000000CER0{isla.idIsla}{codigo}*");
            //if (respuesta.Contains("000000CERX"))
            //{
            //    var resultado = MessageBox.Show("¡Turno cerrado!", respuesta, MessageBoxButtons.OK);

            //    this.Close();
            //}
            //else
            //{
            //    var resultado = MessageBox.Show("¡Error cerrando turno!", respuesta, MessageBoxButtons.OK);

            //    this.Close();
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


        public string send_cmd(string szData)
        {
            Socket m_socClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {

                Console.WriteLine("Enviando por socket");


                int alPort = System.Convert.ToInt16(_infoEstacion.puerto, 10);
                System.Net.IPAddress remoteIPAddress = System.Net.IPAddress.Parse(_infoEstacion.ip);
                System.Net.IPEndPoint remoteEndPoint = new System.Net.IPEndPoint(remoteIPAddress, alPort);
                m_socClient.Connect(remoteEndPoint);

                byte[] byData = System.Text.Encoding.ASCII.GetBytes(szData);
                m_socClient.Send(byData);
                byte[] b = new byte[100];
                m_socClient.Receive(b);
                string szReceived = Encoding.ASCII.GetString(b);
                m_socClient.Close();
                m_socClient.Dispose();
                return szReceived;
            }
            catch (Exception)
            {
                m_socClient.Close();
                m_socClient.Dispose();
                return "Error";
            }
            //Dispose();
        }
    }
}
