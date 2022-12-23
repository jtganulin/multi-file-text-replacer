# Multi-File Text Replacer
This program finds and replaces a text string or regular expression pattern with a replacement throughout multiple text files.


## Instructions for Use
After downloading the latest release or building from source, you're ready to take advantahge of this program's features. 

Designed to be simple and intuitive, the program is easy to use. The steps to use are as follows:
* Add files to the list by clicking the "Add Files" button and selecting the files you want to add. You can also drag and drop files into the list.
* Specify the text to find and the text to replace it with in the "Find text" and "Replace text with" text boxes, respectively.
  * If using Regular Expressions search mode, you can enter an RE pattern in the Find box to use as your search query. Otherwise, Normal Search mode will search for the exact text you enter.
* Use the Backup files... checkbox to create a backup of each file before replacing text in it.
  * Currently, each backup file will be saved alongside the modified file with the extension ".bak" in the location of the original file.
* Finally, click Go and the replacement operation will begin.


## Notes
Created using WPF, upgraded from .NET Framework 4.0 to .NET 6.0 and built using Visual Studio 2022.
