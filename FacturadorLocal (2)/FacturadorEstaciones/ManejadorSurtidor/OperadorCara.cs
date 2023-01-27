using System.IO.Ports;
using System.Text;
using FacturadorEstacionesRepositorio;
using FactoradorEstacionesModelo.Siges;
using ManejadorSurtidor.SICOM;
using Microsoft.Extensions.Options;
using ManejadorSurtidor.Messages;
using FactoradorEstacionesModelo;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using NLog;
using Newtonsoft.Json;

namespace ManejadorSurtidor
{
    public class OperadorCara
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;

        SerialPort serialPort1;
        IEnumerable<SurtidorSiges> Surtidores;
        private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private int count = 0;
        private LectorIButton lectorIButton;
        private string iButtom;
        private bool leyendoButton = false;
        private IMessageProducer _messageProducer;
        private bool respondio;
        private bool finalizo;
        private readonly Sicom _sicom;
        private readonly ISicomConection _sicomConection;
        public OperadorCara(Logger logger, IEnumerable<SurtidorSiges> surtidores, IEstacionesRepositorio estacionesRepositorio, IOptions<Sicom> options, ISicomConection sicomConection, IMessageProducer messageProducer)
        {
            _sicomConection = sicomConection;
            _logger = logger;
            _estacionesRepositorio = estacionesRepositorio;
            _sicom = options.Value;
            _messageProducer = messageProducer;
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
                if (!serialPort1.IsOpen) { serialPort1.Open(); }
                foreach (var surtidor in Surtidores)
                {
                    foreach (var manguera in surtidor.mangueras)
                    {
                        await desautorizarManguera(surtidor, manguera, stoppingToken);
                        manguera.Estado = "BuscandoUltimaVenta";
                        await venta(surtidor, manguera, stoppingToken);
                        manguera.Estado = "Totalizadores";
                        await totalizadorManguera(surtidor, manguera, stoppingToken);
                        manguera.NuevoTotalizador = manguera.totalizador;
                        manguera.NuevaVenta = manguera.ultimaVenta;
                        manguera.Estado = "Lista";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Info, $"Error {ex.Message}");
                _logger.Log(NLog.LogLevel.Info, $"Error {ex.StackTrace}");

            }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var surtidor in Surtidores)
                    {
                        surtidor.turno = _estacionesRepositorio.ObtenerTurnoSurtidor(surtidor.Id);
                        _logger.Log(NLog.LogLevel.Info, $"Turno {JsonConvert.SerializeObject(surtidor.turno)} ");
                        try
                        {

                            if (surtidor.turno != null)
                            {
                                surtidor.idTurno = surtidor.turno.Id;
                                if (surtidor.turno.IdEstado == 1)
                                {
                                    _logger.Log(NLog.LogLevel.Info, $"Abriendo turno {surtidor.Descripcion}");
                                    foreach (var manguera in surtidor.mangueras)
                                    {
                                        manguera.Estado = "Totalizadores";
                                        _logger.Log(NLog.LogLevel.Info, $"Buscando totalizador Manguera {manguera.Descripcion}");
                                        await totalizadorManguera(surtidor, manguera, stoppingToken);

                                        _logger.Log(NLog.LogLevel.Info, $"Abriendo {manguera.Descripcion} {surtidor.turno.Id} {manguera.totalizador}");
                                        _estacionesRepositorio.EnviarTotalizadorApertura(surtidor.Id, surtidor.turno.Id, manguera.Id, manguera.totalizador.ToString());
                                    }
                                }
                                else if (surtidor.turno.IdEstado == 3)
                                {
                                    _logger.Log(NLog.LogLevel.Info, $"Cerrando turno {surtidor.Descripcion}");
                                    foreach (var manguera in surtidor.mangueras)
                                    {
                                        manguera.Estado = "Totalizadores";
                                        _logger.Log(NLog.LogLevel.Info, $"Buscando totalizador Manguera {manguera.Descripcion}");
                                        await totalizadorManguera(surtidor, manguera, stoppingToken);
                                        _logger.Log(NLog.LogLevel.Info, $"Cerrando {manguera.Descripcion} {surtidor.turno.Id} {manguera.totalizador}");
                                        _estacionesRepositorio.EnviarTotalizadorCierre(surtidor.Id, surtidor.turno.Id, manguera.Id, manguera.totalizador.ToString());

                                    }
                                }
                                else if (surtidor.turno.IdEstado == 2)
                                {
                                    _logger.Log(NLog.LogLevel.Info, $"Surtidor {JsonConvert.SerializeObject(surtidor)} ");

                                    if (!serialPort1.IsOpen) { serialPort1.Open(); }
                                    if (serialPort1.IsOpen)
                                    {
                                        if (surtidor.mangueras.Any(x => x.Estado == "Desautorizar" || x.Estado == "BuscarBoton" || (x.Estado == "Colgada" && x.Vendiendo)))
                                        {
                                            foreach (var manguera in surtidor.mangueras)
                                            {
                                                switch (manguera.Estado)
                                                {
                                                    case "Desautorizar":
                                                        manguera.Estado = "Desautorizando";
                                                        manguera.Vendiendo = false;
                                                        await desautorizarManguera(surtidor, manguera, stoppingToken);
                                                        break;
                                                    case "BuscarBoton":
                                                        manguera.Vehiculo = await ValidarBoton(surtidor, manguera);
                                                        if (manguera.Vehiculo != null && manguera.Vehiculo.estado == 0)
                                                        {
                                                            manguera.Estado = "Autorizando";
                                                            await autorizarManguera(surtidor, manguera, stoppingToken);
                                                            manguera.Estado = "Vendiendo";
                                                            manguera.Vendiendo = true;
                                                            manguera.Date = DateTime.Now;
                                                        }
                                                        else
                                                        {

                                                            manguera.Estado = "Desautorizando";
                                                            await desautorizarManguera(surtidor, manguera, stoppingToken);
                                                        }
                                                        break;
                                                    case "Colgada":
                                                        //Protocolo fin venta
                                                        if (manguera.Vendiendo)
                                                        {

                                                            manguera.Estado = "Desautorizando";
                                                            await desautorizarManguera(surtidor, manguera, stoppingToken);

                                                            manguera.Estado = "BuscandoUltimaVenta";
                                                            manguera.CambioVenta = false;
                                                            await venta(surtidor, manguera, stoppingToken);
                                                            while (!manguera.CambioVenta)
                                                            {
                                                                Thread.Sleep(100);
                                                            }
                                                            _logger.Log(NLog.LogLevel.Info, $"Manguera {JsonConvert.SerializeObject(manguera)} ");

                                                            if (manguera.Vehiculo != null && manguera.ultimaVenta > 0)
                                                            {
                                                                bool vender = true;
                                                                if (manguera.ultimaVenta == manguera.NuevaVenta)
                                                                {

                                                                    _logger.Log(NLog.LogLevel.Info, $"Ver totalizador");
                                                                    manguera.Estado = "Totalizadores";
                                                                    manguera.CambioVenta = false;
                                                                    await totalizadorManguera(surtidor, manguera, stoppingToken);

                                                                    _logger.Log(NLog.LogLevel.Info, $"Manguera {JsonConvert.SerializeObject(manguera)} ");
                                                                    while (!manguera.CambioVenta)
                                                                    {
                                                                        Thread.Sleep(100);
                                                                    }
                                                                    if (manguera.NuevoTotalizador == manguera.totalizador)
                                                                    {
                                                                        vender = false;
                                                                    }
                                                                    manguera.NuevoTotalizador = manguera.totalizador;
                                                                }
                                                                if (vender)
                                                                {

                                                                    _logger.Log(NLog.LogLevel.Info, $"Guardando venta");
                                                                    _estacionesRepositorio.AgregarVenta(manguera.Id, manguera.ultimaVenta, manguera.Vehiculo.idrom);

                                                                }
                                                                manguera.NuevaVenta = manguera.ultimaVenta;
                                                            }
                                                            manguera.Estado = "FinVenta";
                                                            manguera.Vendiendo = false;
                                                            manguera.Vehiculo = null;
                                                        }
                                                        break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            await estado(surtidor, null, stoppingToken);
                                        }
                                    }



                                }

                                Thread.Sleep(400);
                            }
                            else
                            {
                                foreach (var manguera in surtidor.mangueras)
                                {

                                    await desautorizarManguera(surtidor, manguera, stoppingToken);
                                }

                                Thread.Sleep(5000);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Log(NLog.LogLevel.Info, $"Error {ex.Message}");
                            _logger.Log(NLog.LogLevel.Info, $"Error {ex.StackTrace}");
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
            }
        }

        private async Task<VehiculoSuic> ValidarBoton(SurtidorSiges surtidor, MangueraSiges manguera)
        {
            var intentos = 0;
            while (++intentos < 3)
            {
                try
                {


                    _logger.Log(NLog.LogLevel.Info, "Validando boton ");
                    var boton = await lectorIButton.leerBoton(surtidor.PuertoIButton, manguera.Ubicacion == "Par", _logger);
                    var vehiculo = await _sicomConection.validateIButton(boton);
                    if (vehiculo == null)
                    {
                        vehiculo = _estacionesRepositorio.GetVehiculoSuic(boton);
                    }
                    return vehiculo;

                }
                catch (Exception ex)
                {
                    _logger.Log(NLog.LogLevel.Info, $"Error {ex.Message}");
                    _logger.Log(NLog.LogLevel.Info, $"Error {ex.StackTrace}");
                }
            }
            return null;
        }

        private async Task estado(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            serialPort1.ReceivedBytesThreshold = 10;
            _logger.Log(NLog.LogLevel.Info, "PreguntandoEstados ");
            string trama = $"0230303{surtidor.Numero}{(146 + surtidor.Numero).ToString("X")}";
            await EnviarTramaAsync(surtidor, manguera, trama, stoppingToken);
        }

        private async Task venta(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {

            serialPort1.ReceivedBytesThreshold = 54;
            string trama = $"0130303{surtidor.Numero}{(145 + surtidor.Numero).ToString("X")}";

            await EnviarTramaAsync(surtidor, manguera, trama, stoppingToken);
        }

        private async Task autorizarManguera(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            await sendEstado(surtidor.Id, manguera.Ubicacion, "Autorizando");
            serialPort1.ReceivedBytesThreshold = 10;
            var ubicacion = manguera.Ubicacion == "Par" ? 1 : 0;
            _logger.Log(NLog.LogLevel.Info, $"Preguntando autorización {manguera.Ubicacion}");
            string trama = $"1830303{surtidor.Numero}0{ubicacion}00{(168 + surtidor.Numero + ubicacion).ToString("X")}";//Desautorizar

            await EnviarTramaAsync(surtidor, manguera, trama, stoppingToken, false);


            await sendEstado(surtidor.Id, manguera.Ubicacion, "Autorizada");
        }

        private async Task desautorizarManguera(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            serialPort1.ReceivedBytesThreshold = 10;
            _logger.Log(NLog.LogLevel.Info, $"Preguntando Desautorizando {manguera.Ubicacion}");
            var ubicacion = manguera.Ubicacion == "Par" ? 1 : 0;
            string trama = $"1830303{surtidor.Numero}0{ubicacion}FF{(167 + surtidor.Numero + ubicacion).ToString("X")}";//Desautorizar

            await EnviarTramaAsync(surtidor, manguera, trama, stoppingToken);

            await sendEstado(surtidor.Id, manguera.Ubicacion, "Desautorizada");
        }


        private async Task totalizadorManguera(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            serialPort1.ReceivedBytesThreshold = 10;
            var ubicacion = manguera.Ubicacion == "Par" ? 1 : 0;
            string trama = $"1630303{surtidor.Numero}0{ubicacion}{(166 + surtidor.Numero + ubicacion).ToString("X")}";//totalizador

            _logger.Log(NLog.LogLevel.Info, $"Preguntando totalizador {manguera.Ubicacion} {trama}");
            await EnviarTramaAsync(surtidor, manguera, trama, stoppingToken);
        }
        private async Task EnviarTramaAsync(SurtidorSiges s, MangueraSiges manguera, string trama, CancellationToken stoppingToken, bool debeEsperar = true)
        {
            var surtidor = Surtidores.First(x => x.Numero == s.Numero);
            surtidor.esperando = true;
            respondio = false;
            finalizo = false;
            if (manguera != null)
            {
                var mang = surtidor.mangueras.First(x => x.Id == manguera.Id);
                mang.esperando = true;
            }
            byte[] tramaByte = FromHex(trama);
            _logger.Log(NLog.LogLevel.Info, $"Enviando trama {trama}");
            count = 0;
            while (!respondio && (debeEsperar || ++count < 6))
            {
                serialPort1.Write(tramaByte, 0, tramaByte.Length); //ENVIO DE LA TRAMA
                Thread.Sleep(400);
            }
            while (!finalizo)
            {
                Thread.Sleep(400);
            }
        }

        public void DataReceiverHandler(object sender,
            SerialDataReceivedEventArgs e)
        {
            try
            {

                respondio = true;
                var surtidor = Surtidores.Any(x => x.esperando) ? Surtidores.First(x => x.esperando) : null;

                surtidor.esperando = false;
                var manguera = surtidor.mangueras.Any(x => x.esperando) ? surtidor.mangueras.First(x => x.esperando) : null;

                if (manguera != null)
                    manguera.esperando = false;
                if (surtidor == null)
                {
                    _logger.Log(NLog.LogLevel.Info, $"No conectado");
                    return;
                }
                if (++count == 500)
                {
                    count = 0;
                    respondio = true;
                    surtidor.esperando = false;
                }

                SerialPort sp = (SerialPort)sender;
                if (manguera != null)
                {
                    _logger.Log(NLog.LogLevel.Info, $"{manguera.Estado}");
                    switch (manguera.Estado)
                    {
                        case "Desautorizando":
                            GetTRamaDesautorizar(sp, surtidor, manguera);
                            break;
                        case "Autorizando":
                            GetTRamaDesautorizar(sp, surtidor, manguera);
                            break;
                        case "BuscandoUltimaVenta":
                            manguera.ultimaVenta = GetTRamaVenta(sp, surtidor, manguera);

                            manguera.CambioVenta = true;
                            break;
                        case "Totalizadores":
                            manguera.totalizador = GetTRamaTotalizador(sp, surtidor, manguera);
                            manguera.CambioVenta = true;
                            break;
                    }
                }
                else
                {
                    VerificarEstado(sp, surtidor, manguera);
                }

            }
            catch (Exception ex)
            {

            }
            finalizo = true;



        }

        private double GetTRamaTotalizador(SerialPort sp, SurtidorSiges surtidor, MangueraSiges manguera)
        {

            surtidor = Surtidores.First(x => x.Id == surtidor.Id);
            manguera = surtidor.mangueras.First(x => x.Id == manguera.Id);
            var hexString = "";
            while (!hexString.Contains("163030") || hexString.Length < 37)
            {
                if (sp.BytesToRead > 0)
                {
                    string intdata = sp.ReadExisting();
                    byte[] response = Encoding.GetEncoding(28591).GetBytes(intdata);
                    hexString += BitConverter.ToString(response);
                    if (hexString.Contains("-"))
                        hexString = hexString.Replace("-", "");
                    if (hexString.Contains("3F"))
                        hexString = hexString.Replace("3F", "");

                    if (hexString.Contains("163030"))
                        hexString = hexString.Substring(hexString.IndexOf("163030"));
                }
                Thread.Sleep(250);
            }

            _logger.Log(NLog.LogLevel.Info, $"estado bruto {hexString}");
            respondio = true;
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



            return double.Parse(totalventa);



        }

        private void VerificarEstado(SerialPort sp, SurtidorSiges surtidor, MangueraSiges manguera)
        {
            surtidor = Surtidores.First(x => x.Id == surtidor.Id);
            var hexString = "";
            while (!hexString.Contains("023030") || hexString.Length < 15)
            {
                if (sp.BytesToRead > 0)
                {
                    string intdata = sp.ReadExisting();
                    byte[] response = Encoding.GetEncoding(28591).GetBytes(intdata);
                    hexString += BitConverter.ToString(response);
                    if (hexString.Contains("-"))
                        hexString = hexString.Replace("-", "");
                    if (hexString.Contains("3F"))
                        hexString = hexString.Replace("3F", "");

                    if (hexString.Contains("023030"))
                        hexString = hexString.Substring(hexString.IndexOf("023030"));
                }
                Thread.Sleep(250);
            }
            _logger.Log(NLog.LogLevel.Info, $"estado bruto {hexString}");
            count = 0;
            var mangueraPar = surtidor.mangueras.FirstOrDefault(x => x.Ubicacion == "Par");
            var mangueraImpar = surtidor.mangueras.FirstOrDefault(x => x.Ubicacion == "Impar");
            string estadoImPar = hexString.Substring(12, 2);
            string estadoPar = hexString.Substring(14, 2);
            _logger.Log(NLog.LogLevel.Info, $"estado impar {estadoImPar}.");
            _logger.Log(NLog.LogLevel.Info, $"estado par {estadoPar}.");

            if ((estadoPar.Contains("00") || estadoPar.Contains("20")) && !mangueraPar.Vendiendo)
            {
                mangueraPar.Estado = "Desautorizar";
            }
            if (estadoPar.Contains("B2"))
            {
                mangueraPar.Estado = "BuscarBoton";
            }
            if (estadoPar.Contains("80"))
            {
                mangueraPar.Estado = "Colgada";
            }

            if ((estadoImPar.Contains("00") || estadoImPar.Contains("20")) && mangueraImpar.Estado != "Vendiendo")
            {
                mangueraImpar.Estado = "Desautorizar";
            }
            if (estadoImPar.Contains("B2"))
            {
                mangueraImpar.Estado = "BuscarBoton";
            }
            if (estadoImPar.Contains("80"))
            {
                mangueraImpar.Estado = "Colgada";
            }

        }

        private double GetTRamaVenta(SerialPort sp, SurtidorSiges surtidor, MangueraSiges manguera)
        {
            surtidor = Surtidores.First(x => x.Id == surtidor.Id);
            manguera = surtidor.mangueras.First(x => x.Id == manguera.Id);
            var hexString = "";
            while (!hexString.Contains("0130303") || hexString.Length < 54)
            {
                if (sp.BytesToRead > 0)
                {
                    string intdata = sp.ReadExisting();
                    byte[] response = Encoding.GetEncoding(28591).GetBytes(intdata);
                    hexString += BitConverter.ToString(response);
                    if (hexString.Contains("-"))
                        hexString = hexString.Replace("-", "");

                    if (hexString.Contains("0130303"))
                        hexString = hexString.Substring(hexString.IndexOf("0130303"));
                }
                Thread.Sleep(250);
            }
            _logger.Log(NLog.LogLevel.Info, $"venta bruto {hexString}");
            count = 0;
            string totalventa = "";
            //0130303130303030303030303030303F31303639323F3F34383603013030313030303030303030303030 
            var iniciolector = manguera.Ubicacion == "Par" ? 43 : 21;

            totalventa += hexString.Substring(iniciolector, 1) == "F" ? "0" : hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1) == "F" ? "0" : hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1) == "F" ? "0" : hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1) == "F" ? "0" : hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1) == "F" ? "0" : hexString.Substring(iniciolector, 1);
            iniciolector += 2;

            _logger.Log(NLog.LogLevel.Info, $"venta bruto {totalventa}");
            return double.Parse(totalventa) / 100;

        }

        private void GetTRamaDesautorizar(SerialPort sp, SurtidorSiges surtidor, MangueraSiges manguera)
        {
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


        private async Task sendEstado(int id, string ubicacion, string estado)
        {
            //await _messageProducer.SendMessage(new Mensaje()
            //{
            //    SurtidorId = id,
            //    Estado = estado,
            //    Ubicacion = ubicacion
            //});
        }
    }
}
