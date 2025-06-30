using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using NLog;
using QRCoder;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;

namespace Tramas
{
    public class Program
    {
        static bool _continue;
        static SerialPort _serialPort;

        //public static void Main()
        //{
        //    Console.WriteLine("Colocar puerto\n");
        //    var puerto = Console.ReadLine();
        //    Console.WriteLine("Colocar ip\n");
        //    var ip = Console.ReadLine();
        //    _continue = true;
        //    while (_continue)
        //    {
        //        Console.WriteLine("Colocar trama\n");
        //        var trama = Console.ReadLine();
        //        var respuesta = send_cmd(trama, puerto, ip);
        //        Console.WriteLine(respuesta);
        //    }

        //}


        //private static void pd_PrintPageOnly(object sender, PrintPageEventArgs ev)
        //{
        //    try
        //    {
        //        float yPos = 0;
        //        int count = 0;
        //        float leftMargin = 5;
        //        float topMargin = 10;
        //        String line = null;
        //        int sizePaper = ev.PageSettings.PaperSize.Width;
        //        int fonSizeInches = 72 / 9;
        //        var _charactersPerPage = 40;
        //        if (_charactersPerPage == 0)
        //        {
        //            _charactersPerPage = fonSizeInches * sizePaper / 100;
        //        }

        //        count = printLine("www.sigessoluciones.com", ev, count, leftMargin, topMargin, false, isQr: true);
        //        if (line != null)
        //            ev.HasMorePages = true;
        //        else
        //            ev.HasMorePages = false;


        //        if (line != null)
        //            ev.HasMorePages = true;
        //        else
        //            ev.HasMorePages = false;
        //    }
        //    catch (Exception ex)
        //    {
        //    }

        //}

        //private static int printLine(string text, PrintPageEventArgs ev, int count, float leftMargin, float topMargin, bool v2, bool isQr)
        //{


        //    float yPos = topMargin + (count * 10);

        //    GenerateQRCode(text, 120);
        //    Image newImage = Image.FromFile("C:\\Users\\ivana\\Documents\\file.bmp");
        //    RectangleF srcRect = new RectangleF(0, 0, 120F, 120F);
        //    GraphicsUnit units = GraphicsUnit.Pixel;
        //    ev.Graphics.DrawImage(newImage, leftMargin, yPos, srcRect, units);
        //    count++;
        //    return count;
        //}



        //private static Image GenerateQRCode(string content, int size)
        //{
        //    QrEncoder encoder = new QrEncoder(ErrorCorrectionLevel.H);
        //    QrCode qrCode;
        //    encoder.TryEncode(content, out qrCode);

        //    GraphicsRenderer gRenderer = new GraphicsRenderer(new FixedModuleSize(4, QuietZoneModules.Two), System.Drawing.Brushes.Black, System.Drawing.Brushes.White);
        //    Graphics g = gRenderer.Draw(qrCode.Matrix);

        //    MemoryStream ms = new MemoryStream();
        //    gRenderer.WriteToStream(qrCode.Matrix, ImageFormat.Bmp, ms);

        //    var imageTemp = new Bitmap(ms);

        //    var image = new Bitmap(imageTemp, new System.Drawing.Size(new System.Drawing.Point(size, size)));

        //    image.Save("C:\\Users\\ivana\\Documents\\file.bmp", ImageFormat.Bmp);

        //    return image;
        //}
        //public static string send_cmd(string szData, string puerto, string ip)
        //{
        //    Socket m_socClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //    try
        //    {

        //        Console.WriteLine("Enviando por socket");


        //        int alPort = System.Convert.ToInt16(puerto, 10);
        //        System.Net.IPAddress remoteIPAddress = System.Net.IPAddress.Parse(ip);
        //        System.Net.IPEndPoint remoteEndPoint = new System.Net.IPEndPoint(remoteIPAddress, alPort);
        //        m_socClient.Connect(remoteEndPoint);

        //        byte[] byData = System.Text.Encoding.ASCII.GetBytes(szData);
        //        m_socClient.Send(byData);
        //        byte[] b = new byte[100];
        //        m_socClient.Receive(b);
        //        string szReceived = Encoding.ASCII.GetString(b);
        //        m_socClient.Close();
        //        m_socClient.Dispose();
        //        return szReceived;
        //    }
        //    catch (Exception)
        //    {
        //        m_socClient.Close();
        //        m_socClient.Dispose();
        //        return "Error";
        //    }
        //    Dispose();
        //}

        public static void Main()
        {

            string name;
            string message;
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            _serialPort = new SerialPort();
            System.ComponentModel.IContainer components = null;
            components = new System.ComponentModel.Container();
            _serialPort = new SerialPort(components);
            _serialPort.PortName = SetPortName("COM8");
            _serialPort.BaudRate = SetPortBaudRate(9600);
            _serialPort.Encoding = Encoding.GetEncoding(28591);
            _serialPort.Parity = SetPortParity(Parity.None);
            _serialPort.DataBits = SetPortDataBits(8);
            _serialPort.StopBits = SetPortStopBits(StopBits.One);
            _serialPort.ReceivedBytesThreshold = 10;
            _serialPort.RtsEnable = true;
            _serialPort.Handshake = SetPortHandshake(Handshake.XOnXOff);

            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceiverHandler);
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _serialPort.Open();
            _continue = true;

            Console.Write("Name: ");
            name = Console.ReadLine();


            while (_continue)
            {
                Console.WriteLine("Colocar trama\n");
                var trama = Console.ReadLine();
                message = trama;
                if (stringComparer.Equals("quit", message))
                {
                    _continue = false;
                }
                else
                {

                    byte[] tramaByte = Encoding.GetEncoding(28591).GetBytes(message);
                    Console.WriteLine("Enviando trama");
                    _serialPort.Write(tramaByte, 0, tramaByte.Length);
                }
            }

            _serialPort.Close();
        }
        public static void DataReceiverHandler(object sender,
           SerialDataReceivedEventArgs e)
        {
            
                SerialPort sp = (SerialPort)sender;
                string intdata = sp.ReadExisting();

            byte[] response = Encoding.GetEncoding(28591).GetBytes(intdata);
                string hexString = BitConverter.ToString(response);
                hexString = hexString.Replace("-", "");

            Console.WriteLine($"Recibiendo {hexString}");
               

        }
        public static byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }

        public static void Read()
        {
            while (_continue)
            {
                try
                {
                    string message = _serialPort.ReadLine();
                    byte[] response = Encoding.GetEncoding(28591).GetBytes(message);
                    string hexString = BitConverter.ToString(response);
                    hexString = hexString.Replace("-", "");

                    Console.WriteLine($"Recibiendo {hexString}");
                }
                catch (TimeoutException) { }
            }
        }

        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("COM port({0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "")
            {
                portName = defaultPortName;
            }
            return portName;
        }

        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate({0}): ", defaultPortBaudRate);
            baudRate = Console.ReadLine();

            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }

        public static Parity SetPortParity(Parity defaultPortParity)
        {
            string parity;

            Console.WriteLine("Available Parity options:");
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Parity({0}):", defaultPortParity.ToString());
            parity = Console.ReadLine();

            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity);
        }

        public static int SetPortDataBits(int defaultPortDataBits)
        {
            string dataBits;

            Console.Write("Data Bits({0}): ", defaultPortDataBits);
            dataBits = Console.ReadLine();

            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits);
        }

        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("Available Stop Bits options:");
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Stop Bits({0}):", defaultPortStopBits.ToString());
            stopBits = Console.ReadLine();

            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits);
        }

        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Available Handshake options:");
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Handshake({0}):", defaultPortHandshake.ToString());
            handshake = Console.ReadLine();

            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake);
        }
    }
}