using System;
using System.IO.Ports;
using System.Windows;
using System.Windows.Threading;

namespace HVACSystemTrainer.Classes
{
    public class ArduinoLink
    {
        public string[]? SensorsData { get; private set; }
        private RegistryManager registry;
        public SerialPort serialPort;

        public ArduinoLink()
        {
            registry = new RegistryManager();
            serialPort = new SerialPort(registry.ReadStringValue("ArduinoSerialPort"))
            {
                BaudRate = registry.ReadDWORDValue("ArduinoBaudRate"),
                Parity = (Parity)Enum.Parse(typeof(Parity), registry.ReadStringValue("ArduinoParity"), true),
                DataBits = registry.ReadDWORDValue("ArduinoDataBits"),
                StopBits = (StopBits)registry.ReadDWORDValue("ArduinoStopBits"),
                Handshake = Handshake.None,
                DtrEnable = true
            };

            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open port: {ex.Message}");
            }
        }

        public void ClosePort()
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    //MessageBox.Show("Serial port closed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
