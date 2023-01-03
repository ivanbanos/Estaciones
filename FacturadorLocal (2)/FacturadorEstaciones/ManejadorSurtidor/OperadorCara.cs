using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Text;
using FacturadorEstacionesRepositorio;
using FactoradorEstacionesModelo.Siges;
using System.Collections.Generic;
using System.Linq;
using NLog;
using ManejadorSurtidor.SICOM;
using Microsoft.Extensions.Options;

namespace ManejadorSurtidor
{
    public class OperadorCara
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;

        SerialPort serialPort1;
        IEnumerable<SurtidorSiges> Surtidores;
        private readonly Logger _logger;
        private int count = 0;
        private LectorIButton lectorIButton;
        private string iButtom;
        private bool leyendoButton = false;
        private readonly Sicom _sicom;
        private readonly ISicomConection _sicomConection;
        public OperadorCara(IEnumerable<SurtidorSiges> surtidores, IEstacionesRepositorio estacionesRepositorio, IOptions<Sicom> options, ISicomConection sicomConection)
        {
            _sicomConection = sicomConection;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _estacionesRepositorio = estacionesRepositorio;
            _sicom = options.Value;
            lectorIButton = new LectorIButton();
            Surtidores = surtidores;

            System.ComponentModel.IContainer components = null;
            components = new System.ComponentModel.Container();
            serialPort1 = new SerialPort(components);
            serialPort1.PortName = surtidores.First().Puerto;
            serialPort1.BaudRate = 4800;
            serialPort1.Encoding = Encoding.GetEncoding(28591);
            //serialPort1.Encoding = Encoding.Default;
            serialPort1.Parity = Parity.Even;
            serialPort1.DataBits = 8;
            serialPort1.StopBits = StopBits.One;
            serialPort1.ReceivedBytesThreshold = 10;
            serialPort1.RtsEnable = true;
            serialPort1.Handshake = Handshake.None;
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceiverHandler);
            //serialPort1.WriteTimeout = 500;
            //serialPort1.ReadTimeout = 500;
            lectorIButton = new LectorIButton();
        }

        public async Task OperarCara(CancellationToken stoppingToken)
        {
            try
            {
                foreach (var surtidor in Surtidores)
                {
                    surtidor.mangueras = new List<MangueraSiges>();
                    surtidor.mangueras.AddRange(surtidor.caras.Select(x => _estacionesRepositorio.GetMangueras(x.Id).First()));

                    _logger.Log(NLog.LogLevel.Info, $"Inicicando Surtidor {surtidor.Descripcion}");

                }
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Info, $"Error {ex.Message}");
                _logger.Log(NLog.LogLevel.Info, $"Error {ex.StackTrace}");
                
            }
            if (!serialPort1.IsOpen) { serialPort1.Open(); }
            foreach (var surtidor in Surtidores)
            {
                foreach (var manguera in surtidor.mangueras)
                {
                    await desautorizarManguera(surtidor, manguera, stoppingToken);
                }
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var surtidor in Surtidores)
                    {

                        var turno = _estacionesRepositorio.ObtenerTurnoSurtidor(surtidor.Id);
                        if (surtidor.idTurno==null && turno != null)
                        {
                            _logger.Log(NLog.LogLevel.Info, $"Abriendo turno {surtidor.Descripcion}");

                            surtidor.Cerrando = false;
                            surtidor.idTurno = turno.Id;
                            surtidor.action = 7;
                            foreach (var manguera in surtidor.mangueras)
                            {
                                manguera.Totalizador = true;
                                await totalizadorManguera(surtidor, manguera, stoppingToken);
                            }
                            surtidor.action = 6;
                        }
                        if (turno == null)
                        {
                            surtidor.idTurno = null;
                        }
                        else
                        {
                            if (turno.IdEstado == 3)
                            {
                                if (!surtidor.Cerrando)
                                {
                                    _logger.Log(NLog.LogLevel.Info, $"Cerrando turno {surtidor.Descripcion}");

                                    surtidor.Cerrando = true;
                                    surtidor.action = 7;
                                    foreach (var manguera in surtidor.mangueras)
                                    {
                                        manguera.Totalizador = true;
                                        await totalizadorManguera(surtidor, manguera, stoppingToken);
                                    }
                                    surtidor.action = 6;
                                }
                            }
                            else {
                                 _logger.Log(NLog.LogLevel.Info, $"Action {surtidor.action} esperando {surtidor.esperando} respondio {surtidor.respondio} ");

                                if (!serialPort1.IsOpen) { serialPort1.Open(); }
                                if (serialPort1.IsOpen)
                                {
                                    await executeAction(surtidor, stoppingToken);
                                }
                            }
                            
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(NLog.LogLevel.Info, $"Error {ex.Message}");
                    _logger.Log(NLog.LogLevel.Info, $"Error {ex.StackTrace}");
                    foreach (var surtidor in Surtidores)
                    {
                        foreach (var manguera in surtidor.mangueras)
                        {

                            await desautorizarManguera(surtidor, manguera, stoppingToken);
                        }
                    }
                    serialPort1.Close();
                }
                await Task.Delay(3000, stoppingToken);
            }
        }

        public async Task executeAction(SurtidorSiges surtidor, CancellationToken stoppingToken)
        {
            switch (surtidor.action)
            {
                case 0:
                    await desautorizarManguera(surtidor, surtidor.mangueras.First(x => x.Ubicacion == "Impar"), stoppingToken);
                    break;
                case 1:
                    await desautorizarManguera(surtidor, surtidor.mangueras.First(x => x.Ubicacion == "Par"), stoppingToken);
                    break;
                case 2:
                    await autorizarManguera(surtidor, surtidor.mangueras.First(x => x.Ubicacion == "Impar"), stoppingToken);
                    break;
                case 3:
                    await autorizarManguera(surtidor, surtidor.mangueras.First(x => x.Ubicacion == "Par"), stoppingToken);
                    break;
                case 4:
                    await venta(surtidor, stoppingToken);
                    break;
                case 6:
                    await estado(surtidor, stoppingToken);
                    break;

            }

        }

        private async Task estado(SurtidorSiges surtidor, CancellationToken stoppingToken)
        {
            serialPort1.ReceivedBytesThreshold = 10;
            //_logger.Log(NLog.LogLevel.Info, "PreguntandoEstados ");
            string trama = $"0230303{surtidor.Numero}{(146 + surtidor.Numero).ToString("X")}";
            await EnviarTramaAsync(surtidor, trama, stoppingToken);
        }

        private async Task venta(SurtidorSiges surtidor, CancellationToken stoppingToken)
        {

            serialPort1.ReceivedBytesThreshold = 54;
            string trama = $"0130303{surtidor.Numero}{(145 + surtidor.Numero).ToString("X")}";
            await EnviarTramaAsync(surtidor, trama, stoppingToken);
        }

        private async Task autorizarManguera(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            serialPort1.ReceivedBytesThreshold = 10;
            var ubicacion = manguera.Ubicacion == "Par" ? 1 : 0;
            _logger.Log(NLog.LogLevel.Info, $"Preguntando autorización {manguera.Ubicacion}");
            string trama = $"1830303{surtidor.Numero}0{ubicacion}00{(168 + surtidor.Numero + ubicacion).ToString("X")}";//Desautorizar
            await EnviarTramaAsync(surtidor, trama, stoppingToken);
        }

        private async Task desautorizarManguera(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            serialPort1.ReceivedBytesThreshold = 10;
            _logger.Log(NLog.LogLevel.Info, $"Preguntando Desautorizando {manguera.Ubicacion}");
            var ubicacion = manguera.Ubicacion == "Par" ? 1 : 0;
            string trama = $"1830303{surtidor.Numero}0{ubicacion}FF{(167 + surtidor.Numero + ubicacion).ToString("X")}";//Desautorizar
            await EnviarTramaAsync(surtidor, trama, stoppingToken);
            

        }


        private async Task totalizadorManguera(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            serialPort1.ReceivedBytesThreshold = 10;
            _logger.Log(NLog.LogLevel.Info, $"Preguntando totalizador {manguera.Ubicacion}");
            var ubicacion = manguera.Ubicacion == "Par" ? 1 : 0;
            string trama = $"1630303{surtidor.Numero}0{ubicacion}{(166 + surtidor.Numero + ubicacion).ToString("X")}";//totalizador
            await EnviarTramaAsync(surtidor, trama, stoppingToken);
        }
        private async Task EnviarTramaAsync(SurtidorSiges s, string trama, CancellationToken stoppingToken)
        {
            var surtidor = Surtidores.First(x => x.Numero == s.Numero);
            surtidor.esperando = true;
            surtidor.respondio = false;
            while (!surtidor.respondio)
            {

                byte[] tramaByte = FromHex(trama);
                //_logger.Log(NLog.LogLevel.Info, $"Enviando trama {trama}");
                serialPort1.Write(tramaByte, 0, tramaByte.Length); //ENVIO DE LA TRAMA
                await Task.Delay(1000, stoppingToken);
            }
        }

        public void DataReceiverHandler(object sender,
            SerialDataReceivedEventArgs e)
        {
            var surtidor = Surtidores.Any(x => x.esperando) ? Surtidores.First(x => x.esperando) : null;
            if (surtidor == null)
            {
                _logger.Log(NLog.LogLevel.Info, $"No conectado");
                return;
            }
            if(++count == 50)
            {
                count = 0;
                surtidor.respondio = true;
                surtidor.esperando = false;
            }
            else if(!leyendoButton)
            {
                try
                {
                    //_logger.Log(NLog.LogLevel.Info, $"Recibiendo");


                    SerialPort sp = (SerialPort)sender;
                    string intdata = sp.ReadExisting();
                    // _logger.Log(NLog.LogLevel.Info, $"Buffer {sp.ReadBufferSize}");

                    byte[] response = Encoding.GetEncoding(28591).GetBytes(intdata);
                    string hexString = BitConverter.ToString(response);
                    hexString = hexString.Replace("-", "");
                    hexString = hexString.Replace("3F", "");
                    //_logger.Log(NLog.LogLevel.Info, $"{hexString}");
                    if (surtidor.action == 4)
                    {
                        if (hexString.StartsWith("0130303") && hexString.Length > 41)
                        {
                            surtidor.respondio = true;
                            surtidor.esperando = false;
                            count = 0;
                            string totalventa = "";
                            var iniciolector = surtidor.ventaPar == 2 ? 35 : 17;
                            hexString = hexString.Substring(0, hexString.Length / 2);
                            totalventa += hexString.Substring(iniciolector, 1) + ".";
                            iniciolector += 2;
                            totalventa += hexString.Substring(iniciolector, 1);
                            iniciolector += 2;
                            totalventa += hexString.Substring(iniciolector, 1);
                            iniciolector += 2;

                            _logger.Log(NLog.LogLevel.Info, $"total {totalventa}");
                            if (surtidor.ventaPar == 2)
                            {
                                AgregarVenta(totalventa, surtidor.mangueras.First(x => x.Ubicacion == "Par"), surtidor.IButtonPar);
                                surtidor.IButtonPar = "";
                                surtidor.action = 1;
                            }
                            else if (surtidor.ventaImpar == 2)
                            {
                                AgregarVenta(totalventa, surtidor.mangueras.First(x => x.Ubicacion == "Impar"), surtidor.IButtonImpar);
                                surtidor.IButtonImpar = "";
                                surtidor.action = 0;
                            }
                        }
                    }
                    else if (surtidor.action == 6)
                    {
                        surtidor.respondio = true;
                        surtidor.esperando = false;
                        count = 0;
                        hexString += "                                        ";

                        string estadoImPar = hexString.Substring(12, 2);
                        string estadoPar = hexString.Substring(14, 2);
                        if (estadoPar == "B2")
                        {

                            _logger.Log(NLog.LogLevel.Info, "Iniciando venta manguera par");
                            System.ComponentModel.IContainer components = null;
                            components = new System.ComponentModel.Container();
                            leyendoButton = true;
                            var iButton = lectorIButton.leerBoton(_sicom.PuertoBoton, true, _logger).Result;
                            _logger.Log(NLog.LogLevel.Info, $"IButton {iButton}");
                            try
                            {
                                var autorizar = _sicomConection.validateIButton(iButton).Result;
                                if (!autorizar)
                                {
                                    //buscarlocal
                                    surtidor.action = 6;
                                    _logger.Log(NLog.LogLevel.Info, $"IButton {iButton} No autorizado");
                                }
                                else {
                                    surtidor.IButtonPar = iButton;
                                    surtidor.action = 3;
                                    surtidor.ventaPar = 1;
                                }
                            }
                            catch (Exception ex)
                            {
                                surtidor.action = 6;
                                _logger.Log(NLog.LogLevel.Info, $"IButton {iButton} No autorizado");
                            }
                            leyendoButton = false;
                            
                        } else
                        if (estadoPar == "80" && surtidor.ventaPar == 1)
                        {

                            surtidor.action = 4;
                            surtidor.ventaPar = 2;

                        }
                        else
                        if (estadoImPar == "B2")
                        {

                            _logger.Log(NLog.LogLevel.Info, "Iniciando venta manguera impar");
                            System.ComponentModel.IContainer components = null;
                            components = new System.ComponentModel.Container();
                            leyendoButton = true;
                            var iButton = lectorIButton.leerBoton(_sicom.PuertoBoton, false, _logger).Result;
                            _logger.Log(NLog.LogLevel.Info, $"IButton {iButton}");
                            try
                            {
                                var autorizar = _sicomConection.validateIButton(iButton).Result;
                                if (!autorizar)
                                {
                                    //buscar local
                                    surtidor.action = 6;
                                    _logger.Log(NLog.LogLevel.Info, $"IButton {iButton} No autorizado");
                                }
                                else
                                {
                                    surtidor.IButtonImpar = iButton;
                                    surtidor.action = 2;
                                    surtidor.ventaImpar = 1;
                                }
                            }
                            catch(Exception ex)
                            {
                                surtidor.action = 6;
                                _logger.Log(NLog.LogLevel.Info, $"IButton {iButton} No autorizado. {ex.Message}");
                            }
                            leyendoButton = false;
                        }else
                        if (estadoImPar == "80" && surtidor.ventaImpar == 1)
                        {
                            surtidor.action = 4;
                            surtidor.ventaImpar = 2;

                        }
                        else
                        {
                            surtidor.action = 6;
                        }
                    }
                    else if (surtidor.action == 7)
                    {
                        if (hexString.StartsWith("163030") && hexString.Length > 37)
                        {
                            surtidor.respondio = true;
                            surtidor.esperando = false;
                            count = 0;
                            string totalventa = "";
                            var iniciolector = 27;
                            totalventa += hexString.Substring(iniciolector, 1);
                            iniciolector += 2;
                            totalventa += hexString.Substring(iniciolector, 1);
                            iniciolector += 2;
                            totalventa += hexString.Substring(iniciolector, 1);
                            iniciolector += 2;
                            totalventa += hexString.Substring(iniciolector, 1);
                            iniciolector += 2;
                            totalventa += hexString.Substring(iniciolector, 1);
                            iniciolector += 2;
                            totalventa += hexString.Substring(iniciolector, 1);
                            var manguera = surtidor.mangueras.First(x => x.Totalizador);
                            if (surtidor.Cerrando)
                            {

                                _estacionesRepositorio.EnviarTotalizadorCierre(surtidor.Id, surtidor.idTurno, manguera.Id, totalventa);
                            }
                            else
                            {
                                _estacionesRepositorio.EnviarTotalizadorApertura(surtidor.Id, surtidor.idTurno, manguera.Id, totalventa);
                            }

                            surtidor.mangueras.First(x => x.Totalizador).Totalizador = false;


                        }

                    }
                    else
                    {
                        surtidor.respondio = true;
                        surtidor.esperando = false;
                        count = 0;
                        surtidor.action = 6;

                    }

                }
                catch (Exception ex)
                {
                    _logger.Log(NLog.LogLevel.Info, $"Error {ex.Message}");
                    _logger.Log(NLog.LogLevel.Info, $"Error {ex.StackTrace}");
                }
            }
            

        }

        private void AgregarVenta(string cantidadventa, MangueraSiges manguera, string iButton)
        {
            _logger.Log(NLog.LogLevel.Info, $"Finalizando venta manguera {manguera.Descripcion} id {manguera.Id}  total {cantidadventa}");
            _estacionesRepositorio.AgregarVenta(manguera.Id, cantidadventa, iButton);
        }

        public byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }
    }
}
