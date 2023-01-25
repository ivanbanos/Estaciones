using FactoradorEstacionesModelo;
using FactoradorEstacionesModelo.Siges;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControladorEstacion
{
    public partial class Surtidor : UserControl, IObserver<Mensaje>
    {
        public Surtidor(FactoradorEstacionesModelo.Siges.SurtidorSiges surtidor)
        {
            InitializeComponent();
            this.Nombre.Text = surtidor.Descripcion;
            SurtidorControl = surtidor;
            //Buscar Turno
            //Buscar Mangueras
        }

        public SurtidorSiges SurtidorControl { get; }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(Mensaje value)
        {
            if (SurtidorControl.Id == value.SurtidorId)
            {
                Turno.Text = value.Turno;
                Islero.Text = value.Empleado;
                if (value.Ubicacion == "Par")
                {
                    this.EstadoPar.Text = value.Estado;
                }
                else
                {
                    this.EstadoImpar.Text = value.Estado;
                }
            }
        }
    }
}
