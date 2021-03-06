using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace RazorPad.ViewModels
{
    internal static class MessageBoxHelpers
    {
        internal static MessageBoxResult ShowConfirmSaveDirtyDocumentMessageBox(RazorTemplateViewModel document)
        {
            return
                MessageBox.Show(
                    "You've made changes to this document - save them before closing?",
                    "SaveAs Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question
                    );
        }

        internal static void ShowErrorMessageBox(string message)
        {
            MessageBox.Show(
                message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
                );
        }

        internal static string ShowOpenFileDialog()
        {
            // Create OpenFileDialog
            var dlg = new OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".cshtml";
            dlg.Filter = "RazorPad Documents|*.razorpad";
            dlg.Filter = "C# Razor Documents|*.cshtml";
            dlg.Filter = "VB Razor Documents|*.vbhtml";
            dlg.Filter = "All Files|*.*";

            if (dlg.ShowDialog().GetValueOrDefault())
                return dlg.FileName;

            return null;
        }

        internal static string ShowSaveAsDialog(RazorTemplateViewModel template)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".razorpad";
            dlg.Filter = "RazorPad Documents|*.razorpad";
            dlg.Filter = "C# Razor Documents|*.cshtml";
            dlg.Filter = "VB Razor Documents|*.vbhtml";
            dlg.Filter = "All Files|*.*";

            if (!string.IsNullOrWhiteSpace(template.Filename))
            {
                string directory = Path.GetDirectoryName(template.Filename);

                if (!string.IsNullOrWhiteSpace(directory))
                    dlg.InitialDirectory = directory;
            }

            if (dlg.ShowDialog().GetValueOrDefault())
                return dlg.FileName;
            else
                return null;
        }
    }
}