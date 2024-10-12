using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace HVACSystemTrainer.Classes
{
    public class ReportManager
    {
        public void SaveReport(ListBox list)
        {
            try
            {
                // Create and configure the SaveFileDialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = "Report.txt", // Default file name
                    DefaultExt = ".txt", // Default file extension
                    Filter = "Text documents (.txt)|*.txt" // Filter files by extension
                };

                // Show the dialog and get the result
                bool? result = saveFileDialog.ShowDialog();

                // If the user selected a file
                if (result == true)
                {
                    // Get the selected file path
                    string filePath = saveFileDialog.FileName;

                    // Create the content to save
                    StringBuilder content = new();

                    foreach (var item in list.Items)
                    {
                        content.AppendLine(item.ToString());
                    }

                    // Write the content to the file
                    File.WriteAllText(filePath, content.ToString());
                    MessageBox.Show("Export completed!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Encountered: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
