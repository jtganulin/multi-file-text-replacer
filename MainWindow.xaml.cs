using System.Windows;
using System.IO;
using System.Text.RegularExpressions;
using MessageBox = System.Windows.Forms.MessageBox;

namespace MultiFileTextReplacer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static OpenFileDialog selectFilesDialog = new();

    public MainWindow()
    {
        InitializeComponent();

        //Style/Assign properties of file selection dialog
        selectFilesDialog.Filter = "Text Files|*.txt|All File Types|*.*";
        selectFilesDialog.Multiselect = true;
        selectFilesDialog.Title = "Select Files";
    }

    private void ShowFileSelectDialog(object sender, EventArgs e)
    {
		//Show the dialog
		if (selectFilesDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
		    //After items are selected, output them to the fileSelectionBox
		    foreach (string fileName in selectFilesDialog.FileNames)
		    {
			    fileSelectionBox.Items.Add(fileName);
		    }
        }
	}

    public void AddFiles_Click(object sender, RoutedEventArgs e)
    {
        ShowFileSelectDialog(sender, e);
    }

    // Clear Selection Box and Clear FileNames
    public void ClearFiles_Click(object sender, RoutedEventArgs e)
    {
        selectFilesDialog.Reset();
        fileSelectionBox.Items.Clear();
        //MessageBox.Show("All Files Cleared. You may add more using the Add File(s) Button.", "Files Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    #region Replace Text in Specified Files and Close Streams
    public void btnGo_Click(object sender, RoutedEventArgs e)
    {
		if (selectFilesDialog.FileNames.Length == 0)
		{
			MessageBox.Show("No files selected. You may select files using the Add File(s) Button.", "No Files Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
		}

		foreach (string file in selectFilesDialog.FileNames)
        {
            try
            {
                using StreamReader reader = new(file);
                using StreamWriter writer = new(file);

                string content = reader.ReadToEnd();
                content = Regex.Replace(content, findBox.Text, replaceBox.Text);
                writer.Write(content);
            }
            catch(Exception ex)
            {
                MessageBox.Show("There has been an error. Please try again. \n" + ex.Message.ToString());
                return;
            };
        }

        MessageBox.Show("Replacing Complete!\nYou may now exit the application or perform more tasks.", "Replacing Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    #endregion

    private void aboutBtn_Click(object sender, RoutedEventArgs e)
    {
        Window aboutWin = new AboutWindow();
        aboutWin.ShowDialog();
    }

    private void fileSelectionBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
		ShowFileSelectDialog(sender, e);
	}

    // Upon "Delete" press we'd like to remove the currently selected files
    private void fileSelectionBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Delete)
        {
            var selectedItems = fileSelectionBox.SelectedItems.OfType<string>().ToList();
            for (int i = fileSelectionBox.Items.Count - 1; i >= 0;  i--)
            {
                if (selectedItems.Contains(fileSelectionBox.Items[i])) 
                    fileSelectionBox.Items.RemoveAt(i);
            }

		} else return;
    }
};

