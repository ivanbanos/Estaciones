using FactoradorEstacionesModelo.Objetos;
using FacturadorEstacionesRepositorio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FacturadorEstacionesPOSWinForm
{
    public partial class AgregarTercero : Form
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private List<TipoIdentificacion> _tiposIdentificacion;

        public AgregarTercero(IEstacionesRepositorio estacionesRepositorio, string identificacion)
        {
            InitializeComponent();
            _estacionesRepositorio = estacionesRepositorio;
            this.comboBox1.Text = "Selec. Tipo Identificacion";

            _tiposIdentificacion = _estacionesRepositorio.getTiposIdentifiaciones();

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(_tiposIdentificacion.ToArray());
            this.Identificacion.Text = identificacion;
            this.Identificacion.Enabled = false;
            
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if ((TipoIdentificacion)comboBox1.SelectedItem == null)
            {

                MessageBox.Show("Debe seleccionar tipo de identificación");
                return;
            }
            _estacionesRepositorio.crearTercero(0, (TipoIdentificacion)comboBox1.SelectedItem,Identificacion.Text, Nombre.Text, Telefono.Text,Correo.Text,Direccion.Text,"");
            this.Close();
        }

    }
}
