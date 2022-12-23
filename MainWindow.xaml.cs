using System.Windows;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;

namespace MultiFileTextReplacer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	// Declare the extensions that we know can be opened and modified as text vs binary executable?
	// A whitelist of sorts; would it be better to instead blacklist known executable extensions
	// TODO: We still should ideally determine the format from the actual byte content in the file to be sure it's "text"
	private static readonly string[] ValidExtensions = new string[] { "txt", "bat", "xml", "xhtml", "html", "js", "pl", "sh", "cmd", "h", "ini", "plist", "xaml", "csv", "log", "json", "yaml", "md", "css", "php", "c", "cs", "cpp", "jsx", "ts", "tsx", "tex", "asp", "aspx", "htm", "shtml", "shtm", "cfm", "cfml", "pl", "py", "rb", "rhtml", "rhtm", "rxml", "rss", "xsl", "xslt", "java", "jsp", "asp", "aspx", "ascx", "cshtml", "vbhtml", "config", "csproj", "sln", "resx", "csx", "vb", "vbx", "fs", "fsx", "fsi", "fsproj", "vbproj", "vcxproj" };

	private static Microsoft.Win32.OpenFileDialog selectFilesDialog = new();

	// Represents the image shown on the background of the file selection ListBox before any files are added.
	private static readonly ImageBrush ListBoxBG = new ImageBrush(new ImageSourceConverter().ConvertFromString("pack://application:,,,/ListBoxBG.gif") as ImageSource);


	public MainWindow()
	{
		InitializeComponent();

		ListBoxBG.Stretch = Stretch.UniformToFill;
		FileSelectionBox.Background = ListBoxBG;

		// Could just add *. to the entries in the array
		string formattedExts = new Regex("/;$/i").Replace(string.Join("", ValidExtensions.Select(s => "*." + s + ";").ToArray()), "");

		// Style/Assign properties of file selection dialog
		selectFilesDialog.Filter = "Text Files (" + formattedExts + ")|" + formattedExts + "|All Files|*.*";
		selectFilesDialog.Multiselect = true;
		selectFilesDialog.Title = "Select Files...";
	}


	// Produces the Open File dialog and adds the selected files to the ListBox after checking their extension
	private void ShowFileSelectDialog(object sender, EventArgs e)
	{
		// Show the dialog
		if (selectFilesDialog?.ShowDialog() == true)
		{
			// After items are selected, output them to the FileSelectionBox
			foreach (string fileName in selectFilesDialog.FileNames)
			{
				if (CheckForDuplicateFile(fileName) && CheckFileExtension(fileName))
					FileSelectionBox.Items.Add(fileName);
			}
			ToggleListBoxBG();
		}
	}


	// Get the extension of the file, and check if the extension is in the list of valid extensions
	private static bool CheckFileExtension(string fileName)
	{
		if (!ValidExtensions.Contains(Path.GetExtension(fileName).Replace(".", "", StringComparison.InvariantCultureIgnoreCase)))
		{
			// If the extension isn't valid we want to alert the user and prompt to discard this file or continue
			if (MessageBox.Show(Application.Current.MainWindow, $"The file {fileName} doesn't have a common text file extension, and might be a binary file. Modifying it could cause corruption. Would you like to continue with this file?", "Possible Non-text file", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
			{
				return true;
			}
			else return false; // If they've chosen to discard the file
		}
		else return true; // If the file has an extension that we've accounted for in ValidExtensions
	}


	// Check the ListBox's Items to see if a newly added file has already been added; ignore it if so
	private bool CheckForDuplicateFile(string fileName)
	{
		if (FileSelectionBox.Items.Contains(fileName))
		{
			MessageBox.Show(Application.Current.MainWindow, $"The file {fileName} has already been added to the list.", "Duplicate File", MessageBoxButton.OK, MessageBoxImage.Information);
			return false;
		}
		else return true;
	}


	// Allow the Add Files button to bring up the Open files dialog
	public void AddFiles_Click(object sender, RoutedEventArgs e)
	{
		ShowFileSelectDialog(sender, e);
	}


	// Clear FileSelection ListBox and re-show the info BG
	public void ClearFiles_Click(object sender, RoutedEventArgs e)
	{
		// If there are no files in the selection we don't need to warn the user
		if (FileSelectionBox.Items.Count > 0)
		{
			if (MessageBox.Show(Application.Current.MainWindow, "Are you sure you want to clear the list of selected files?", "Clear File Selection List", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				FileSelectionBox.Items.Clear();
				ToggleListBoxBG();
			}
		}
	}


	// Replaces Text in Specified Files, After checking for valid extensions and backing up as necessary
	private void btnGo_Click(object sender, RoutedEventArgs e)
	{
		if (!FileSelectionBox.HasItems)
		{
			MessageBox.Show(Application.Current.MainWindow, "No files selected. You may select files using the Add File(s) button or by dragging and dropping files into the file selection box.", "No Files Selected", MessageBoxButton.OK, MessageBoxImage.Information);
			return;
		}

		// Need to check find and replace box for valid input
		// For find box, the input should be at least non-empty
		if (string.IsNullOrEmpty(findBox.Text))
		{
			MessageBox.Show(Application.Current.MainWindow, "The Find Text box is empty. Please enter a string or regular expression to search for.", "No Find String", MessageBoxButton.OK, MessageBoxImage.Information);
			return;
		}

		foreach (string file in FileSelectionBox.Items)
		{
			try
			{
				using StreamReader reader = new(file);
				string content = reader.ReadToEnd();
				reader.Close();

				// If the user has chosen to backup the file, we do that before replacing
				if (bckCheckBox.IsChecked == true)
				{
					MessageBox.Show(Application.Current.MainWindow, "Backing up before replacing text. The original files will be stored with .bak extensions in the same location as the newly modified files.", "File Backups Enabled", MessageBoxButton.OK, MessageBoxImage.Information);

					// Create a path for the file with ".bak" in the suffix
					string bckPath = Regex.Replace(file, new Regex(Path.GetExtension(file) + "$").ToString(), ".bak" + Path.GetExtension(file));

					// If a file with the backup path is already there we must create a new one
					// Alternatively could prompt for overwrite or have savefiledialog to allow
					// the user to choose a name & path, although this would be tedious for many files
					if (File.Exists(bckPath))
					{
						// Geneerate a new path with a random number in the suffix, ideally for consistency
						// with how Windows deals with duplicates we should append (1 or 2...) to the end of the file
						bckPath = bckPath.Replace(".bak", ".bak" + new Random().Next(128, 1024), StringComparison.InvariantCultureIgnoreCase);

						// Alert the user to what the backup will be named
						MessageBox.Show(Application.Current.MainWindow, "A backup for this file seems to already exist. A new backup file will be named: " + bckPath + "\nIt will be saved in the same location as the newly modified file.", "Backup File Exists", MessageBoxButton.OK, MessageBoxImage.Information);
					}
					// Create or open the file at the backup path, and write the original file's contents to it
					using StreamWriter bckSW = File.CreateText(bckPath);
					bckSW.Write(content);
				}

				// Check if the user is searching via RE or string based search
				// TODO: Allow replacing with regex substitutions?
				if (radioModeRegex.IsChecked == true)
					content = Regex.Replace(content, findBox.Text, replaceBox.Text);
				else
					content = Regex.Replace(content, Regex.Escape(findBox.Text).ToString(), replaceBox.Text);

				// Write the modified text contents back to the file
				using StreamWriter writer = new(file);
				writer.Write(content);
			}
			catch (RegexParseException ex)
			{
				MessageBox.Show(Application.Current.MainWindow, "The regular expression you've entered is invalid. Please check your input and try again.\n\n" + ex.Message, "Invalid Regular Expression", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show(Application.Current.MainWindow, "There has been an error. \n\n" + ex.Message + "Try again or click the About / Help button for more information.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			};
		}

		MessageBox.Show(Application.Current.MainWindow, "Replacing Complete!\nYou may now exit the application or perform more tasks.", "Replacing Completed", MessageBoxButton.OK, MessageBoxImage.Information);
	}


	// Display the About & Help window
	private void AboutBtn_Click(object sender, RoutedEventArgs e)
	{
		new AboutWindow().ShowDialog();
	}


	// Want to toggle the informative BG image depending on if there are files selected or not
	private void ToggleListBoxBG()
	{
		if (FileSelectionBox.Items.Count > 0)
			FileSelectionBox.Background = new SolidColorBrush(Colors.WhiteSmoke);
		else
			FileSelectionBox.Background = ListBoxBG;
	}


	// Allow a double-click on the ListBox to open the file selection dialog
	private void FileSelectionBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		ShowFileSelectDialog(sender, e);
	}


	// Upon "Delete" press we'd like to remove the currently selected files
	private void FileSelectionBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		// TODO: Reflect Backspace for Delete in the ListBox info bg and Help screen?
		if (e.Key == System.Windows.Input.Key.Delete || e.Key == System.Windows.Input.Key.Back)
		{
			var selectedItems = FileSelectionBox.SelectedItems.OfType<string>().ToList();
			for (int i = FileSelectionBox.Items.Count - 1; i >= 0; i--)
			{
				if (selectedItems.Contains(FileSelectionBox.Items[i]))
					FileSelectionBox.Items.RemoveAt(i);
			}
			ToggleListBoxBG();
		}
		else return;
	}


	// Change the cursor when a user is dragging an item into the ListBox
	private void FileSelectionBox_DragOver(object sender, System.Windows.DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
			e.Effects = DragDropEffects.Copy;
		else
			e.Effects = DragDropEffects.None;

		e.Handled = true;
	}


	// Handle files being dropped inside the ListBox
	private void FileSelectionBox_Drop(object sender, System.Windows.DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
			foreach (string f in files)
			{
				if (CheckForDuplicateFile(f) && CheckFileExtension(f))
					FileSelectionBox.Items.Add(f);
			}
		}

		ToggleListBoxBG();
		e.Handled = true;
	}
}
