using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManejadorSurtidor.SICOM
{
    public class LectorIButton
    {
        private SerialPort serialPort1;
        private string resultado;
        private bool leido;
        private int cant;
        private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public async Task<string> leerBoton(string puerto, bool caraPAr, Logger _logger)
        {
            cant = 0;
            System.ComponentModel.IContainer components = null;
            components = new System.ComponentModel.Container();
            serialPort1 = new SerialPort(components);
            serialPort1.PortName = puerto;
            serialPort1.BaudRate = 9600;
            serialPort1.Encoding = Encoding.GetEncoding(28591);
            //serialPort1.Encoding = Encoding.Default;
            serialPort1.Parity = Parity.None;
            serialPort1.DataBits = 8;
            serialPort1.StopBits = StopBits.One;
            serialPort1.ReceivedBytesThreshold = 10;
            serialPort1.RtsEnable = true;
            serialPort1.Handshake = Handshake.None;
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceiverHandler);
            //serialPort1.WriteTimeout = 500;
            //serialPort1.ReadTimeout = 500;
            while (true)
            {

                if (!serialPort1.IsOpen) { serialPort1.Open(); break; }
            }
            serialPort1.ReceivedBytesThreshold = 10;
            _logger.Log(NLog.LogLevel.Info, $"Leyendo boton ");
            string trama = caraPAr?$"3b31324971" : $"3b31314972";
            await EnviarTramaAsync(trama);
            serialPort1.Close();
            try
            {
                var codigos = resultado.Split("-");
                var iButton = "";
                for(int i=0; i < 8; i++)
                {
                    iButton = codigos[i]+iButton;
                }
                _logger.Log(NLog.LogLevel.Info, $"Leyendo boton {resultado}");
                return iButton;
            }
            catch (Exception) {
                return "fail";
            }
        }
        private async Task EnviarTramaAsync(string trama)
        {
            leido = false;
            while (!leido)
            {

                byte[] tramaByte = FromHex(trama);
                //_logger.LogInformation( $"Enviando trama {trama}");
                serialPort1.Write(tramaByte, 0, tramaByte.Length); //ENVIO DE LA TRAMA
                Thread.Sleep(1000);
            }
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
        public void DataReceiverHandler(object sender,
            SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string intdata = sp.ReadExisting();
            // _logger.LogInformation( $"Buffer {sp.ReadBufferSize}");
            
            byte[] response = Encoding.GetEncoding(28591).GetBytes(intdata);
            string hexString = BitConverter.ToString(response);

            resultado = hexString;
            cant++;
            _logger.Log(NLog.LogLevel.Info, $"Leyendo boton {resultado}");
            if ((!resultado.ToLower().Contains("4e-42") && resultado.Length>8) || cant>10)
            {
                leido = true;
            }
        }
    }
}
