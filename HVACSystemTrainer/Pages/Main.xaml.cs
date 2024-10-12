using HVACSystemTrainer.Classes;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HVACSystemTrainer.Pages
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        OmronHostLink omronHostLink;
        ArduinoLink arduinoLink;
        BackgroundWorker backgroundWorker;
        ReportManager reportManager;
        RegistryManager registryManager;
        CancellationTokenSource cancellationTokenSource;
        MainWindow mainWindow;
        public Main()
        {
            Application.Current.MainWindow = mainWindow;
            InitializeComponent();
            reportManager = new ReportManager();
            registryManager = new RegistryManager();
            arduinoLink = new ArduinoLink();
            omronHostLink = new OmronHostLink();
            arduinoLink.serialPort.DataReceived += SerialPort_DataReceived;
            omronHostLink.DataReceived += PLC_DataReceived;
            if (!omronHostLink.isOpen)
            {
                omronHostLink.Open();
            }
            this.Unloaded += Main_Unloaded;
            mainWindow = (MainWindow)Application.Current.MainWindow;
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerAsync();
            compressorMotorImage.Source = (registryManager.ReadDWORDValue("CompressorMotor") == 1) ? new BitmapImage(new Uri("/Images/icons8-toggle-on-96.png", UriKind.Relative)) : compressorMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-off-96.png", UriKind.Relative));
            condenserMotorImage.Source = (registryManager.ReadDWORDValue("CondenserMotor") == 1) ? new BitmapImage(new Uri("/Images/icons8-toggle-on-96.png", UriKind.Relative)) : compressorMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-off-96.png", UriKind.Relative));
            inductionMotorImage.Source = (registryManager.ReadDWORDValue("InductionMotor") == 1) ? new BitmapImage(new Uri("/Images/icons8-toggle-on-96.png", UriKind.Relative)) : compressorMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-off-96.png", UriKind.Relative));
            PopulateListBox();
        }

        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (omronHostLink._serialPort.IsOpen)
                {
                    omronHostLink.ReadMemory(registryManager.ReadDWORDValue("MachineStatusDM").ToString(), 2);
                    Thread.Sleep(5000);
                }
            }
        }

        private void PopulateListBox()
        {
            List<string> dummyData = new List<string>
            {
                "Item 1",
                "Item 2",
                "Item 3",
                "Item 4",
                "Item 5",
                "Item 6",
                "Item 7",
                "Item 8",
                "Item 9",
                "Item 10"
            };

            systemReportList.ItemsSource = dummyData;
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
                    Dispatcher.BeginInvoke(new Action(() => {
                        temperature1.Text = SensorsData[0] + " °C";
                        temperature2.Text = SensorsData[1] + " °C";
                        temperature3.Text = SensorsData[2] + " °C";
                        temperature4.Text = SensorsData[3] + " °C";
                        airVelocity.Text = SensorsData[5] + " m/s";
                    }));
                    if (SensorsData[0] == "20")
                    {
                        omronHostLink.WriteMemory(registryManager.ReadDWORDValue("TemperatureDM").ToString(), "1");
                        systemReportList.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm: ") + "Temperature reached the 20°C set point");
                    }
                    if (SensorsData[3] == "28")
                    {
                        omronHostLink.WriteMemory(registryManager.ReadDWORDValue("TemperatureDM").ToString(), "2");
                        systemReportList.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm: ") + "Temperature reached the 28°C set point");
                    }
                }
                else
                {
                    systemReportList.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm: ") + "Arduino serial port is closed.");
                }
            }
            catch (Exception ex)
            {
                systemReportList.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm: ") + ex.Message);
            }
        }
        private void PLC_DataReceived(object? sender, string e)
        {
            string processedData = ExtractData(e);

            if (processedData.Length >= 8)
            {
                string d000ValueHex = processedData.Substring(0, 4);
                string d001ValueHex = processedData.Substring(4, 4);

                int d000Value = Convert.ToInt32(d000ValueHex, 16);
                int d001Value = Convert.ToInt32(d001ValueHex, 16);

                if (d000Value > 32767) d000Value -= 65536;
                if (d001Value > 32767) d001Value -= 65536;

                Console.WriteLine($"D000 Value: {d000Value}");
                Console.WriteLine($"D001 Value: {d001Value}");

                if (d000Value == 1)
                {
                    machineStatus.Text = "Running";
                    machineStatus.Foreground = Brushes.LimeGreen;
                }
                else if (d000Value == 2)
                {
                    machineStatus.Text = "Stop";
                    machineStatus.Foreground = Brushes.Red;
                }
                else if (d000Value == 3)
                {
                    machineStatus.Text = "E-Stop";
                    machineStatus.Foreground = Brushes.Red;
                }
                else if (d000Value == 4)
                {
                    machineStatus.Text = "Initializing";
                    machineStatus.Foreground = Brushes.Yellow;
                }
                else if (d000Value == 5)
                {
                    machineStatus.Text = "Standby";
                    machineStatus.Foreground = Brushes.Blue;
                }

                if (d001Value == 1)
                {
                    machineStatus.Text = "Auto";
                    machineStatus.Foreground = Brushes.Blue;
                }
                else if (d001Value == 2)
                {
                    machineStatus.Text = "Manual";
                    machineStatus.Foreground = Brushes.LimeGreen;
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
            omronHostLink.DataReceived -= PLC_DataReceived;
            arduinoLink.ClosePort();
            omronHostLink.Close();
        }

        private void compressorMotorImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (registryManager.ReadDWORDValue("CompressorMotor") == 0)
                {
                    omronHostLink.WriteMemory(registryManager.ReadDWORDValue("CompressorMotorDM").ToString(), "1");
                    registryManager.WriteDWORDValue("CompressorMotor", 1);
                    compressorMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-on-96.png", UriKind.Relative));
                }
                else
                {
                    omronHostLink.WriteMemory(registryManager.ReadDWORDValue("CompressorMotorDM").ToString(), "0");
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
                    omronHostLink.WriteMemory(registryManager.ReadDWORDValue("CondenserMotorDM").ToString(), "1");
                    registryManager.WriteDWORDValue("CondenserMotor", 1);
                    condenserMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-on-96.png", UriKind.Relative));
                }
                else
                {
                    omronHostLink.WriteMemory(registryManager.ReadDWORDValue("CondenserMotorDM").ToString(), "0");
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
                    omronHostLink.WriteMemory(registryManager.ReadDWORDValue("InductionMotorDM").ToString(), "1");
                    registryManager.WriteDWORDValue("InductionMotor", 1);
                    inductionMotorImage.Source = new BitmapImage(new Uri("/Images/icons8-toggle-on-96.png", UriKind.Relative));
                }
                else
                {
                    omronHostLink.WriteMemory(registryManager.ReadDWORDValue("InductionMotorDM").ToString(), "0");
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
            var damperControl = registryManager.ReadDWORDValue("DamperControl");
            if (damperControl > 0)
            {
                var damperControlValue = registryManager.ReadDWORDValue("DamperControl") - 10;
                registryManager.WriteDWORDValue("DamperControl", damperControlValue);
                damper.Text = damperControlValue.ToString()+"%";
                omronHostLink.WriteMemory(registryManager.ReadDWORDValue("DamperControlDM").ToString(), damperControlValue.ToString());
            }
        }

        private void damperPlusImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var damperControl = registryManager.ReadDWORDValue("DamperControl");
            if (damperControl < 100)
            {
                var damperControlValue = registryManager.ReadDWORDValue("DamperControl") + 10;
                registryManager.WriteDWORDValue("DamperControl", damperControlValue);
                damper.Text = damperControlValue.ToString() + "%";
                omronHostLink.WriteMemory(registryManager.ReadDWORDValue("DamperControlDM").ToString(), damperControlValue.ToString());
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
