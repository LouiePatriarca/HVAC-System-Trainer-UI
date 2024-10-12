using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HVACSystemTrainer.Classes
{
    public class RegistryManager
    {
        private string _registryPath = @"SOFTWARE\HVACSystemTrainer";

        public RegistryManager()
        {
            CreateDefaultValues();
        }

        // Method to create default values in the registry
        public void CreateDefaultValues()
        {
            try
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(_registryPath, true);
                if (key == null)
                {
                    // Key does not exist, create it
                    using RegistryKey newKey = Registry.CurrentUser.CreateSubKey(_registryPath);
                    if (newKey != null)
                    {
                        newKey.SetValue("ArduinoSerialPort", "COM2", RegistryValueKind.String);
                        newKey.SetValue("ArduinoBaudRate", 9600, RegistryValueKind.DWord);
                        newKey.SetValue("ArduinoParity", "None", RegistryValueKind.String);
                        newKey.SetValue("ArduinoDataBits", 8, RegistryValueKind.DWord);
                        newKey.SetValue("ArduinoStopBits", 1, RegistryValueKind.DWord);

                        newKey.SetValue("PLCSerialPort", "COM4", RegistryValueKind.String);
                        newKey.SetValue("PLCBaudRate", 9600, RegistryValueKind.DWord);
                        newKey.SetValue("PLCParity", "None", RegistryValueKind.String);
                        newKey.SetValue("PLCDataBits", 8, RegistryValueKind.DWord);
                        newKey.SetValue("PLCStopBits", 1, RegistryValueKind.DWord);

                        newKey.SetValue("CompressorMotorDM", 11, RegistryValueKind.DWord);
                        newKey.SetValue("CondenserMotorDM", 12, RegistryValueKind.DWord);
                        newKey.SetValue("InductionMotorDM", 13, RegistryValueKind.DWord);
                        newKey.SetValue("DamperControlDM", 5, RegistryValueKind.DWord);
                        newKey.SetValue("MachineStatusDM", 0, RegistryValueKind.DWord);
                        newKey.SetValue("MachineModeDM", 1, RegistryValueKind.DWord);
                        newKey.SetValue("TemperatureDM", 10, RegistryValueKind.DWord);
                        newKey.SetValue("Pin", "88888888", RegistryValueKind.String);

                        newKey.SetValue("CompressorMotor", 0, RegistryValueKind.DWord);
                        newKey.SetValue("CondenserMotor", 0, RegistryValueKind.DWord);
                        newKey.SetValue("InductionMotor", 0, RegistryValueKind.DWord);
                        newKey.SetValue("DamperControl", 0, RegistryValueKind.DWord);
                        newKey.SetValue("MachineStatus", 0, RegistryValueKind.DWord);
                        newKey.SetValue("MachineMode", 0, RegistryValueKind.DWord);
                        newKey.SetValue("Temperature", 0, RegistryValueKind.DWord);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Method to read a string value from the registry
        public string ReadStringValue(string valueName)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(_registryPath))
            {
                if (key != null)
                {
                    return key.GetValue(valueName, "NotFound").ToString();
                }
                else
                {
                    return "NotFound";
                }
            }
        }

        // Method to read a DWORD value from the registry
        public int ReadDWORDValue(string valueName)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(_registryPath))
            {
                if (key != null)
                {
                    return (int)key.GetValue(valueName, -1);
                }
                else
                {
                    return -1;
                }
            }
        }

        // Method to write a string value to the registry
        public void WriteStringValue(string valueName, string value)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(_registryPath))
            {
                if (key != null)
                {
                    key.SetValue(valueName, value, RegistryValueKind.String);
                }
            }
        }

        // Method to write a DWORD value to the registry
        public void WriteDWORDValue(string valueName, int value)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(_registryPath))
            {
                if (key != null)
                {
                    key.SetValue(valueName, value, RegistryValueKind.DWord);
                }
            }
        }
    }
}
