using ControladorEstacion.Messages;
using FacturadorEstacionesPOSWinForm;
using FacturadorEstacionesPOSWinForm.Repo;
using FacturadorEstacionesRepositorio;
using Microsoft.Extensions.Options;

namespace ControladorEstacion
{
    public partial class Form1 : Form
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly InfoEstacion _infoEstacion;
        public Form1(IEstacionesRepositorio estacionesRepositorio, IOptions<InfoEstacion> infoEstacion, IConexionEstacionRemota conexionEstacionRemota)
        {
            _estacionesRepositorio = estacionesRepositorio;
            InitializeComponent();
            var messageReceiver = new RabbitMQMessagesReceiver();
            messageReceiver.ReceiveMessages();
            var surtidores = _estacionesRepositorio.GetSurtidoresSiges();
            var surtidoresComponets = new List<Surtidor>();
            var posActual = 0;
            foreach (var surtidor in surtidores)
            {
                var newSurtidor = new Surtidor(surtidor);
                surtidoresComponets.Add(newSurtidor);
                newSurtidor.Location = new System.Drawing.Point(12 + (165 * posActual), 80 );
                newSurtidor.Name = surtidor.Descripcion;
                this.Controls.Add(newSurtidor);
                posActual++;
            }
        }
    }
}