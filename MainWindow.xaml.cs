using System.Windows;
using System.IO;
using System.Text.RegularExpressions;

namespace MultiFileTextReplacer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    
        private OpenFileDialog selectFilesDialog = new();
        
#region Add Files
        public void addFiles_Click(object sender, RoutedEventArgs e)
        {
            //Style/Assign Properties
            selectFilesDialog.Filter = "Text Files|*.txt|All File Types|*.*";
            selectFilesDialog.Multiselect = true;
            selectFilesDialog.Title = "Select Files";
            //Show the dialog
            selectFilesDialog.ShowDialog();
            //Hide the label defining selection
            lblSelectInfo.Opacity = 0;
            //After items are selected, output them to the fileSelectionBox
            foreach (string fileName in selectFilesDialog.FileNames)
            {
                fileSelectionBox.Items.Add(fileName);
            }
        }
#endregion

#region Clear Selection Box and Clear FileNames
        public void clearFiles_Click(object sender, RoutedEventArgs e)
        {
            selectFilesDialog.Reset();
            fileSelectionBox.Items.Clear();
            System.Windows.Forms.MessageBox.Show("All Files Cleared. You may add more using the Add File(s) Button.", "Files Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //Make the selection help lbl visible again until new selection
            lblSelectInfo.Opacity = 100;
       
        }
        #endregion

#region Replace Text in Specified Files and Close Streams
        public void btnGo_Click(object sender, RoutedEventArgs e)
        {
            foreach (string file in selectFilesDialog.FileNames)
            {
                try
                {
                    StreamReader reader = new StreamReader(file);
                    string content = reader.ReadToEnd();
                    reader.Close();
                    content = Regex.Replace(content, findBox.Text, replaceBox.Text);
                    StreamWriter writer = new StreamWriter(file);
                    writer.Write(content);
                    writer.Close();
                }
                catch
                {
                    // The user lacks appropriate permissions to read files, discover paths, etc. 
                    System.Windows.Forms.MessageBox.Show("There has been an error. Please try again.");
                };
            }
            
            if (selectFilesDialog.FileNames.Length == 0)
            {
                System.Windows.Forms.MessageBox.Show("No Files Selected. You may select files using the Add File(s) Button.", "No Files Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else {
                System.Windows.Forms.MessageBox.Show("Replacing Complete!\nYou may now exit the application or perform more tasks.", "Tasks Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        private void aboutBtn_Click(object sender, RoutedEventArgs e)
        {
            Window aboutWin = new AboutWindow();
            aboutWin.ShowDialog();
            
        }
    };

}


