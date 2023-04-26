using FactoradorEstacionesModelo.Objetos;
using FactoradorEstacionesModelo.Siges;
using FacturadorEstacionesRepositorio;
using Microsoft.Extensions.Options;
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
    public partial class Sicom : Form
    {

        private System.Timers.Timer timer1;
        public Sicom(VehiculoSuic value)
        {
            InitializeComponent();
            Vehiculo.Text += " "+value.placa;
            Fecha.Text = value.fechaFin.ToString();
            Idrom.Text += " " + value.idrom;
            if(value.fechaFin < DateTime.Now)
            {

                Autorizado.Text = $"No autorizado, motivo vencido";
            }
            else if (value.estado == 0)
            {
                Autorizado.Text = "Autorizado";
            }
            else 
            {
                Autorizado.Text =  $"No autorizado, motivo {value.motivoTexto}";
            }
            var _timer = new Timer();
            _timer.Interval = 15000; // interval in milliseconds here.
            _timer.Tick += (s, e) => this.Close();
            _timer.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


    }
}
