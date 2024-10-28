using HVACSystemTrainer.Classes;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace HVACSystemTrainer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.NavigationService.Navigate(new Pages.Main());
        }
        private void dashboard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MainFrame.NavigationService.CanGoBack)
            {
                MainFrame.NavigationService.GoBack();
            }
        }

        private void settings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new Pages.Settings());
        }

        private void logoutImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Login login = new();
            login.Show();
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void restartImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var currentProcess = Process.GetCurrentProcess();
            Process.Start(currentProcess.MainModule.FileName);
            Application.Current.Shutdown();
        }
    }
}