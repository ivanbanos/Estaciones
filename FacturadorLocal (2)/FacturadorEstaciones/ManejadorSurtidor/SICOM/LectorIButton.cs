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

        public async Task<string> leerBoton(string puerto, int surtidorNumero, bool caraPAr, Logger _logger)
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
           while (!serialPort1.IsOpen)
            {
                serialPort1.Open();
            }
            
            serialPort1.ReceivedBytesThreshold = 10;
            _logger.Log(NLog.LogLevel.Info, $"Leyendo boton ");
            string trama = GetTrama(surtidorNumero, caraPAr);
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
                _logger.Log(NLog.LogLevel.Info, $"Leyendo boton {iButton}");
                return iButton;
            }
            catch (Exception) {
                return "fail";
            }
        }

        private string GetTrama(int surtidorNumero, bool caraPAr)
        {
            if (caraPAr)
            {
                switch (surtidorNumero)
                {
                    case 1:
                        return "3B31324971";
                    case 2:
                        return "3B32324972";
                    case 3:
                        return "3B33324973";
                    case 4:
                        return "3B34324974";
                    case 5:
                        return "3B35324975";
                    case 6:
                        return "3B36324976";
                    case 7:
                        return "3B37324977";
                    case 8:
                        return "3B38324972";
                }
            }
            else
            {
                switch (surtidorNumero)
                {
                    case 1:
                        return "3B31314972";
                    case 2:
                        return "3B32314971";
                    case 3:
                        return "3B33314970";
                    case 4:
                        return "3B34314977";
                    case 5:
                        return "3B35314976";
                    case 6:
                        return "3B36314975";
                    case 7:
                        return "3B37314974";
                    case 8:
                        return "3B38314972";
                }
            }
            return "";
        }

        private async Task EnviarTramaAsync(string trama)
        {
            leido = false;
            var count = 0;
            while (!leido && count++ < 7)
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
            try
            {
                SerialPort sp = (SerialPort)sender;

                cant = 0;
                resultado = "";
                while ((resultado.ToLower().Contains("4e-42") || resultado.Length < 8) && cant++ < 5)
                {
                    if (sp.BytesToRead > 0)
                    {
                        string intdata = sp.ReadExisting();
                        // _logger.LogInformation( $"Buffer {sp.ReadBufferSize}");

                        byte[] response = Encoding.GetEncoding(28591).GetBytes(intdata);
                        string hexString = BitConverter.ToString(response);

                        resultado += hexString;

                        resultado = resultado.Replace("4e-42-", "");
                        _logger.Log(NLog.LogLevel.Info, $"Leyendo boton {resultado}");
                        if (resultado.Length >= 8)
                        {
                            leido = true;
                        }
                    }
                    Thread.Sleep(250);
                }
            }catch(Exception ex)
            {

            }
           
            
        }
    }
}
