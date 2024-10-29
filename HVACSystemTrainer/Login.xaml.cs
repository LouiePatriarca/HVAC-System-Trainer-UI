using HVACSystemTrainer.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HVACSystemTrainer
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        RegistryManager registryManager;
        public Login()
        {
            InitializeComponent();
            registryManager = new RegistryManager();
            password.Focus();
        }

        private void password_GotFocus(object sender, RoutedEventArgs e)
        {
            //var oskProcess = Process.Start("osk.exe");

            Task.Run(() =>
            {
                //oskProcess.WaitForExit();
                Dispatcher.Invoke(() =>
                {
                    loginButton.Focus();
                    Keyboard.ClearFocus();
                });
            });
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (password.Password == registryManager.ReadStringValue("Pin")) { 
                password.Clear();
                foreach (var process in Process.GetProcessesByName("osk"))
                {
                    process.Kill();
                }
                this.Hide();
                MainWindow mainWindow = new();  
                mainWindow.Show();
            }
        }
    }
}
