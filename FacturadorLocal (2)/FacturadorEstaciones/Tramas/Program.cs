using NLog;
using System;
using System.Configuration;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Tramas
{
    public class Program
    {
        static bool _continue;
        static SerialPort _serialPort;

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

            Console.WriteLine("Type QUIT to exit");

            while (_continue)
            {
                message = Console.ReadLine();

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