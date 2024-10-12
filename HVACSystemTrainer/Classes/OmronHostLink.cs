using Microsoft.Win32;
using System.IO.Ports;
using System.Text;

namespace HVACSystemTrainer.Classes
{
    public class OmronHostLink
    {
        public SerialPort _serialPort;
        RegistryManager _registryManager;
        public bool isOpen = false;
        public OmronHostLink()
        {
            _registryManager = new RegistryManager();
            var port = _registryManager.ReadStringValue("PLCSerialPort");
            var baudRate = _registryManager.ReadDWORDValue("PLCBaudRate");
            _serialPort = new SerialPort(port, baudRate)
            {
                Parity = (Parity)Enum.Parse(typeof(Parity), _registryManager.ReadStringValue("PLCParity"), true),
                DataBits =_registryManager.ReadDWORDValue("PLCDataBits"),
                StopBits = (StopBits)_registryManager.ReadDWORDValue("PLCStopBits"),
                Handshake = Handshake.None,
                Encoding = Encoding.ASCII
            };
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = _serialPort.ReadExisting();
                if (IsValidFCS(data))
                {
                    OnDataReceived(data);
                }
                else
                {
                    Console.WriteLine("Invalid FCS received: " + data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in DataReceived: " + ex.Message);
            }
        }

        public void Open()
        {
            Close();
                _serialPort.Open();
            isOpen = true;
        }

        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.DataReceived -= SerialPort_DataReceived;
                _serialPort.Close();
                isOpen = false;
            }
        }

        private string CalculateFCS(string message)
        {
            int fcs = 0;
            foreach (char c in message)
            {
                fcs ^= Convert.ToByte(c);
            }
            return fcs.ToString("X2");
        }

        public void ReadMemory(string address, int length)
        {
            string command = $"@00RD{address.PadLeft(4, '0')}{length.ToString("X2")}";
            string fcs = CalculateFCS(command);
            string message = $"{command}{fcs}*";
            SendMessage(message);
        }

        public void WriteMemory(string address, string data)
        {
            string command = $"@00WD{address.PadLeft(4, '0')}{data}";
            string fcs = CalculateFCS(command);
            string message = $"{command}{fcs}*";
            SendMessage(message);
        }

        private void SendMessage(string message)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Write(message);
            }
            else
            {
                throw new InvalidOperationException("Serial port is not open.");
            }
        }

        public event EventHandler<string>? DataReceived;

        private void OnDataReceived(string data)
        {
            DataReceived?.Invoke(this, data);
        }
        private bool IsValidFCS(string message)
        {
            if (message.Length < 4) // Minimum length for valid data with FCS
            {
                return false;
            }
            string data = message.Substring(0, message.Length - 3);
            string receivedFCS = message.Substring(message.Length - 3, 2);
            string calculatedFCS = CalculateFCS(data);
            return calculatedFCS.Equals(receivedFCS, StringComparison.OrdinalIgnoreCase);
        }
    }
}
