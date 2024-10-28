using HVACSystemTrainer.Classes;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PLC_Communication.NET;
using System.Text;

namespace HVACSystemTrainer.Pages
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        ArduinoLink arduinoLink;
        BackgroundWorker backgroundWorker;
        ReportManager reportManager;
        RegistryManager registryManager;
        CancellationTokenSource cancellationTokenSource;
        MainWindow mainWindow;
        OmronSerialHostLink omronSerialHostLink;
        SerialPort _serialPort;
        public Main()
        {
            Application.Current.MainWindow = mainWindow;
            InitializeComponent();
            reportManager = new ReportManager();
            registryManager = new RegistryManager();
            arduinoLink = new ArduinoLink();
            arduinoLink.serialPort.DataReceived += SerialPort_DataReceived;

            this.Unloaded += Main_Unloaded;
            mainWindow = (MainWindow)Application.Current.MainWindow;
            _serialPort = new SerialPort()
            {
                BaudRate = registryManager.ReadDWORDValue("PLCBaudRate"),
                PortName = registryManager.ReadStringValue("PLCSerialPort"),
                Parity = (Parity)Enum.Parse(typeof(Parity), registryManager.ReadStringValue("PLCParity"), true),
                DataBits = registryManager.ReadDWORDValue("PLCDataBits"),
                StopBits = (StopBits)registryManager.ReadDWORDValue("PLCStopBits"),
                Handshake = Handshake.None,
                Encoding = Encoding.ASCII
            };
            _serialPort.DataReceived += PLC_DataReceived;
       
            if (!_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Open();
                    omronSerialHostLink = new OmronSerialHostLink(_serialPort);
                    backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += BackgroundWorker_DoWork;
                    backgroundWorker.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    systemReportList.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm: ") + "PLC serial port is closed. Check the cable connection and configuration then restart the system.");
                }
            }
            
            
            compressorMotorImage.Source = (registryManager.ReadDWORDValue("CompressorMotor") == 1) ? new BitmapImage(new Uri("/Images/icons8-toggle-on-96.png", UriKind.Relative)) : compressorMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-off-96.png", UriKind.Relative));
            condenserMotorImage.Source = (registryManager.ReadDWORDValue("CondenserMotor") == 1) ? new BitmapImage(new Uri("/Images/icons8-toggle-on-96.png", UriKind.Relative)) : compressorMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-off-96.png", UriKind.Relative));
            inductionMotorImage.Source = (registryManager.ReadDWORDValue("InductionMotor") == 1) ? new BitmapImage(new Uri("/Images/icons8-toggle-on-96.png", UriKind.Relative)) : compressorMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-off-96.png", UriKind.Relative));
            damper.Text = registryManager.ReadDWORDValue("DamperControl").ToString() + "%";
        }

        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (_serialPort.IsOpen)
                {
                    omronSerialHostLink.Reads(0, HeaderCode_Read.RD, (ushort)registryManager.ReadDWORDValue("MachineStatusDM"), 2);
                    Thread.Sleep(3000);
                }
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (arduinoLink.serialPort.IsOpen)
                {
                    string message = arduinoLink.serialPort.ReadExisting();
                    string[] SensorsData = message.Split(',');
                    arduinoLink.serialPort.DiscardInBuffer();
                    arduinoLink.serialPort.DiscardOutBuffer();
                    if (SensorsData.Length>0)
                    {
                        Dispatcher.BeginInvoke(new Action(() => {
                            temperature1.Text = SensorsData[0] + " °C";
                            temperature2.Text = SensorsData[1] + " °C";
                            temperature3.Text = SensorsData[2] + " °C";
                            temperature4.Text = SensorsData[3] + " °C";
                            airVelocity.Text = SensorsData[5] + " m/s";
                        }));
                        if (SensorsData[0] == "20")
                        {
                            omronSerialHostLink.Writes(0, HeaderCode_Write.WD, (ushort)registryManager.ReadDWORDValue("TemperatureDM"), new short[] { (short)1 });
                            systemReportList.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm: ") + "Temperature reached the 20°C set point");
                        }
                        if (SensorsData[3] == "28")
                        {
                            omronSerialHostLink.Writes(0, HeaderCode_Write.WD, (ushort)registryManager.ReadDWORDValue("TemperatureDM"), new short[] { (short)2 });
                            systemReportList.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm: ") + "Temperature reached the 28°C set point");
                        }
                    }
                }
                else
                {
                    systemReportList.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm: ") + "Arduino serial port is closed. Check the cable connection and configuration then restart the system.");
                }
            }
            catch (Exception ex)
            {
                systemReportList.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm: ") + ex.Message);
            }
        }
        private void PLC_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string processedData = _serialPort.ReadExisting();

            if (processedData.Length == 19)
            {
                string d000ValueHex = processedData.Substring(7, 4);
                string d001ValueHex = processedData.Substring(11, 4);

                int d000Value = Convert.ToInt32(d000ValueHex, 16);
                int d001Value = Convert.ToInt32(d001ValueHex, 16);


                Console.WriteLine($"D000 Value: {d000Value}");
                Console.WriteLine($"D001 Value: {d001Value}");

                if (d000Value == 1)
                {
                    Dispatcher.BeginInvoke(() => {
                        machineStatus.Text = "Running";
                        machineStatus.Foreground = Brushes.LimeGreen;
                    });
                }
                else if (d000Value == 2)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        machineStatus.Text = "Stop";
                        machineStatus.Foreground = Brushes.Red;
                    });
                }
                else if (d000Value == 3)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        machineStatus.Text = "E-Stop";
                        machineStatus.Foreground = Brushes.Red;
                    });
                }
                else if (d000Value == 4)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        machineStatus.Text = "Initializing";
                        machineStatus.Foreground = Brushes.Yellow;
                    });
                }
                else if (d000Value == 5)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        machineStatus.Text = "Standby";
                        machineStatus.Foreground = Brushes.Blue;
                    });
                }

                if (d001Value == 1)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        machineMode.Text = "Auto";
                        machineMode.Foreground = Brushes.Blue;
                    });
                }
                else if (d001Value == 2)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        machineMode.Text = "Manual";
                        machineMode.Foreground = Brushes.LimeGreen;
                    });
                }
            }
            else
            {
                Console.WriteLine("Insufficient data received.");
            }
        }
        private static string ExtractData(string message)
        {
            int startIndex = message.IndexOf("RD") + 4; 
            int endIndex = message.IndexOf('*');

            if (startIndex >= 0 && endIndex > startIndex)
            {
                return message.Substring(startIndex, endIndex - startIndex);
            }
            return string.Empty;
        }

        private void Main_Unloaded(object sender, RoutedEventArgs e)
        {
            arduinoLink.serialPort.DataReceived -= SerialPort_DataReceived;
            arduinoLink.ClosePort();
        }

        private void compressorMotorImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (registryManager.ReadDWORDValue("CompressorMotor") == 0)
                {
                    omronSerialHostLink.Writes(0, HeaderCode_Write.WD, (ushort)registryManager.ReadDWORDValue("CompressorMotorDM"), new short[] { (short)1 });
                    registryManager.WriteDWORDValue("CompressorMotor", 1);
                    compressorMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-on-96.png", UriKind.Relative));
                }
                else
                {
                    omronSerialHostLink.Writes(0, HeaderCode_Write.WD, (ushort)registryManager.ReadDWORDValue("CompressorMotorDM"), new short[] { (short)0 });
                    registryManager.WriteDWORDValue("CompressorMotor", 0);
                    compressorMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-off-96.png", UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                systemReportList.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm: ") + ex.Message);
            }
        }

        private void condenserMotorImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (registryManager.ReadDWORDValue("CondenserMotor") == 0)
                {
                    omronSerialHostLink.Writes(0, HeaderCode_Write.WD, (ushort)registryManager.ReadDWORDValue("CondenserMotorDM"), new short[] { (short)1 });
                    registryManager.WriteDWORDValue("CondenserMotor", 1);
                    condenserMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-on-96.png", UriKind.Relative));
                }
                else
                {
                    omronSerialHostLink.Writes(0, HeaderCode_Write.WD, (ushort)registryManager.ReadDWORDValue("CondenserMotorDM"), new short[] { (short)0 });
                    registryManager.WriteDWORDValue("CondenserMotor", 0);
                    condenserMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-off-96.png", UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                systemReportList.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm: ") + ex.Message);
            }
        }

        private void inductionMotorImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (registryManager.ReadDWORDValue("InductionMotor") == 0)
                {
                    omronSerialHostLink.Writes(0, HeaderCode_Write.WD, (ushort)registryManager.ReadDWORDValue("InductionMotorDM"), new short[] { (short)1 });
                    registryManager.WriteDWORDValue("InductionMotor", 1);
                    inductionMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-on-96.png", UriKind.Relative));
                }
                else
                {
                    omronSerialHostLink.Writes(0, HeaderCode_Write.WD, (ushort)registryManager.ReadDWORDValue("InductionMotorDM"), new short[] { (short)0 });
                    registryManager.WriteDWORDValue("InductionMotor", 0);
                    inductionMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-off-96.png", UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                systemReportList.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm: ") + ex.Message);
            }
        }

        private void damperMinusImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var damperControl = registryManager.ReadDWORDValue("DamperControl");
                if (damperControl > 0)
                {
                    var damperControlValue = registryManager.ReadDWORDValue("DamperControl") - 10;
                    registryManager.WriteDWORDValue("DamperControl", damperControlValue);
                    damper.Text = damperControlValue.ToString() + "%";
                    omronSerialHostLink.Writes(0, HeaderCode_Write.WD, (ushort)registryManager.ReadDWORDValue("DamperControlDM"), new short[] { (short)damperControlValue });
                }
            }
            catch (Exception ex)
            {
                systemReportList.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm: ") + ex.Message);
            }
        }

        private void damperPlusImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var damperControl = registryManager.ReadDWORDValue("DamperControl");
                if (damperControl < 100)
                {
                    var damperControlValue = registryManager.ReadDWORDValue("DamperControl") + 10;
                    registryManager.WriteDWORDValue("DamperControl", damperControlValue);
                    damper.Text = damperControlValue.ToString() + "%";
                    omronSerialHostLink.Writes(0, HeaderCode_Write.WD, (ushort)registryManager.ReadDWORDValue("DamperControlDM"), new short[] { (short)damperControlValue });
                }
            }
            catch (Exception ex)
            {
                systemReportList.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm: ") + ex.Message);
            }
        }

        private void motorFrequency_GotFocus(object sender, RoutedEventArgs e)
        {
            var oskProcess = Process.Start("osk.exe");

            Task.Run(() =>
            {
                oskProcess.WaitForExit();
                Dispatcher.Invoke(() =>
                {
                    systemReportList.Focus();
                    Keyboard.ClearFocus();
                });
            });
        }

        private void saveReportButton_Click(object sender, RoutedEventArgs e)
        {
            reportManager.SaveReport(systemReportList);
        }
    }
}
