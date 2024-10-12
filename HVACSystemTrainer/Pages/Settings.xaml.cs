using HVACSystemTrainer.Classes;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HVACSystemTrainer.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        RegistryManager registryManager;
        public Settings()
        {
            InitializeComponent();
            registryManager = new RegistryManager();
            arduinoSerialPort.Text = registryManager.ReadStringValue("ArduinoSerialPort");
            arduinoBaudRate.Text = registryManager.ReadDWORDValue("ArduinoBaudRate").ToString();
            arduinoParity.Text = registryManager.ReadStringValue("ArduinoParity");
            arduinoDataBit.Text = registryManager.ReadDWORDValue("ArduinoDataBits").ToString();
            arduinoStopBit.Text = registryManager.ReadDWORDValue("ArduinoStopBits").ToString();

            PLCserialPort.Text = registryManager.ReadStringValue("PLCSerialPort");
            PLCbaudRate.Text = registryManager.ReadDWORDValue("PLCBaudRate").ToString();
            PLCparity.Text = registryManager.ReadStringValue("PLCParity");
            PLCdataBit.Text = registryManager.ReadDWORDValue("PLCDataBits").ToString();
            PLCstopBit.Text = registryManager.ReadDWORDValue("PLCStopBits").ToString();

            compressorMotorDM.Text = registryManager.ReadDWORDValue("CompressorMotorDM").ToString();
            condenserMotorDM.Text = registryManager.ReadDWORDValue("CondenserMotorDM").ToString();
            inductionMotorDM.Text = registryManager.ReadDWORDValue("InductionMotorDM").ToString();
            damperControlMotorDM.Text = registryManager.ReadDWORDValue("DamperControlDM").ToString();
            machineStatusDM.Text = registryManager.ReadDWORDValue("MachineStatusDM").ToString();
            machineModeDM.Text = registryManager.ReadDWORDValue("MachineModeDM").ToString();
            temperatureDM.Text = registryManager.ReadDWORDValue("TemperatureDM").ToString();
        }

        private void saveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                registryManager.WriteStringValue("ArduinoSerialPort", arduinoSerialPort.Text);
                registryManager.WriteDWORDValue("ArduinoBaudRate", Convert.ToInt32(arduinoBaudRate.Text));
                registryManager.WriteStringValue("ArduinoParity",  arduinoParity.Text);
                registryManager.WriteDWORDValue("ArduinoDataBits", Convert.ToInt32(arduinoDataBit.Text));
                registryManager.WriteDWORDValue("ArduinoStopBits", Convert.ToInt32(arduinoStopBit.Text));
                registryManager.WriteStringValue("PLCSerialPort", PLCserialPort.Text);
                registryManager.WriteDWORDValue("PLCBaudRate", Convert.ToInt32(PLCbaudRate.Text));
                registryManager.WriteStringValue("PLCParity", PLCparity.Text);
                registryManager.WriteDWORDValue("PLCDataBits", Convert.ToInt32(PLCdataBit.Text));
                registryManager.WriteDWORDValue("PLCStopBits", Convert.ToInt32(PLCstopBit.Text));
                registryManager.WriteDWORDValue("CompressorMotorDM", Convert.ToInt32(compressorMotorDM.Text));
                registryManager.WriteDWORDValue("CondenserMotorDM", Convert.ToInt32(condenserMotorDM.Text));
                registryManager.WriteDWORDValue("InductionMotorDM", Convert.ToInt32(inductionMotorDM.Text));
                registryManager.WriteDWORDValue("DamperControlDM", Convert.ToInt32(damperControlMotorDM.Text));
                registryManager.WriteDWORDValue("MachineStatusDM", Convert.ToInt32(machineStatusDM.Text));
                registryManager.WriteDWORDValue("MachineModeDM", Convert.ToInt32(machineModeDM.Text));
                registryManager.WriteDWORDValue("TemperatureDM", Convert.ToInt32(temperatureDM.Text));
                MessageBox.Show("Settings Successfully Saved.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
