using ControladorEstacion.Messages;
using FacturadorEstacionesPOSWinForm.Repo;
using FacturadorEstacionesRepositorio;
using Microsoft.Extensions.Options;
using Modelo;

namespace ControladorEstacion
{
    public partial class Form1 : Form
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;

        public Form1(IEstacionesRepositorio estacionesRepositorio, IOptions<InfoEstacion> infoEstacion)
        {
            _estacionesRepositorio = estacionesRepositorio;
            InitializeComponent();
            var messageReceiver = new RabbitMQMessagesReceiver(infoEstacion);
            var surtidores = _estacionesRepositorio.GetSurtidoresSiges();
            var surtidoresComponets = new List<Surtidor>();
            var posActual = 0;
            foreach (var surtidor in surtidores)
            {
                var newSurtidor = new Surtidor(surtidor);
                surtidoresComponets.Add(newSurtidor);
                newSurtidor.Location = new System.Drawing.Point(12 , 80 + (161 * posActual));
                newSurtidor.Name = surtidor.Descripcion;
                this.Controls.Add(newSurtidor);
                messageReceiver.Subscribe(newSurtidor);
                posActual++;
            }
        }
    }
}