﻿using FactoradorEstacionesModelo.Objetos;
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
    public partial class Fidelizacion : Form
    {
        private readonly IFidelizacion _fidelizacion;
        private readonly FacturaSiges _factura;
        public Fidelizacion(IFidelizacion fidelizacion, FacturaSiges factura)
        {
            InitializeComponent();
            textBox1.PlaceholderText = "Documento";
            _fidelizacion = fidelizacion;
        }

        private void abrir_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    MessageBox.Show("Debe diligenciar documento");
                }
                _fidelizacion.SubirPuntops((float)_factura.Total, textBox1.Text, _factura.DescripcionResolucion + "-" + _factura.Consecutivo).Wait();
                this.DialogResult = DialogResult.OK;
                this.Close();
            } catch(Exception)
            {

                this.DialogResult = DialogResult.Abort;
                MessageBox.Show("Código de empleado no existe o turno abierto");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


    }
}
