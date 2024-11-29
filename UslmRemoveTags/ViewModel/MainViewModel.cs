using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace UslmRemoveTags.ViewModel
{
    internal class MainViewModel : Abstract.ViewModelBase
    {

        public DelegateCommand<DragEventArgs> DropFileCommand { get; set; }
        //public DelegateCommand SaveFileCommand { get; set; }

        public MainViewModel()
        {
            DropFileCommand = new DelegateCommand<DragEventArgs>(OnDrop);
            //SaveFileCommand = new DelegateCommand(SaveCleanedFile);
        }

        private string _fileContent;
        public string FileContent
        {
            get { return _fileContent; }
            set { _fileContent = value; OnPropertyChanged(); }
        }

        private string _fileFullPath;

        public string FileFullPath
        {
            get { return _fileFullPath; }
            set { _fileFullPath = value; OnPropertyChanged(); }
        }

        private string _cleanedFileContent;
        public string CleanedFileContent
        {
            get { return _cleanedFileContent; }
            set { _cleanedFileContent = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Get the drop file path and data (only .txt file)
        /// Call the Remove <normal> </normal> tags method and save cleaned method
        /// </summary>
        /// <param name="parameter"></param>
        private void OnDrop(DragEventArgs parameter)
        {
            try
            {
                if (parameter.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Get the list of dropped file paths
                    string[] filePaths = parameter.Data.GetData(DataFormats.FileDrop) as string[];

                    if (filePaths != null && filePaths.Length > 0)
                    {
                        var filePath = filePaths[0];

                        FileFullPath = filePath;

                        if (File.Exists(filePath) && System.IO.Path.GetExtension(filePath) == ".txt")
                        {
                            try
                            {
                                string fileContent = File.ReadAllText(filePath);
                                FileFullPath = System.IO.Path.GetFullPath(filePath);

                                //find and remove <normal> tag
                                CleanedFileContent = RemoveNormalTags(fileContent);


                                // Detect errors (empty and incomplete tags)
                                string errorMessages = FindErrorNormalTags(fileContent);
                                if (!string.IsNullOrEmpty(errorMessages))
                                {
                                    //FileContent = errorMessages;
                                    SaveErrorFile(errorMessages);
                                }
                                else
                                {
                                    FileContent = "No errors found.";
                                }

                                SaveCleanedFile(CleanedFileContent);

                            }
                            catch (Exception ex)
                            {
                                ErrorMessage(ex);
                            }
                        }
                        else
                        {
                            InformationMessage("Invalid file type. Please drop a .txt file.", "Invalid input file");
                            FileContent = "";
                            FileFullPath = "";
                        }
                    }
                    else
                    {
                        WarningMessage("No file dropped.");
                        FileContent = "";
                        FileFullPath = "";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }


        //TODO incomplete tags

        /// <summary>
        /// Method to remove <normal> and </normal> tags from the content
        /// </summary>
        /// Replacing all occurrences of the pattern defined by the regular expression
        /// </? Matches either an opening <normal> tag or a closing </normal> tag.
        /// [^>]* Matches any number of characters that are not > 
        /// > Matches the closing > of the tag.
        /// <param name="content"></param>
        /// <returns></returns>
        private string RemoveNormalTags(string content)
        {
            try
            {
                // Regex pattern to match and remove <normal> and </normal> tags
                //var pattern = @"<normal>(.*?)</normal>";
                //var pattern = @"<normal[^>]*>(.*?)</normal>";

                //var pattern = @"<normal[^>]*>(.*?)</normal\s*[^>]*>";
                //var pattern = @"<normal[^>]*>.*$";

                //var NormalTagPattern = @"<normal[^>]*>[^<]*$";
                //var NormalTagPattern = @"<normal[^>]*>.*?$";
                //var NormalTagPattern = @"<normal[^>]*>.*$";
                //var BTagPattern = @"</?b[^>]*>";


                //Regex pattern to match <normal> and </normal> tags (and any attributes within)
                var NormalTagPattern = @"</?normal[^>]*>";
                var BTagPattern = @"<b>(.*?)</b>";


                //var RemoveAllTags = new Regex(pattern, RegexOptions.IgnoreCase);
                var incompleteCloseTagRegex = new Regex(NormalTagPattern, RegexOptions.IgnoreCase);
                var BTagRegex = new Regex(BTagPattern, RegexOptions.IgnoreCase);

                //Remove all <normal>...</normal> tags
                string cleanedContent = incompleteCloseTagRegex.Replace(content, "");

                cleanedContent = BTagRegex.Replace(cleanedContent, "$1");

                // Check if the content is empty after cleaning
                if (string.IsNullOrWhiteSpace(cleanedContent))
                {
                    return null;
                }

                return cleanedContent;
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return null;
            }
        }

        // Method to find empty <normal> tags and return their line numbers
        private string FindErrorNormalTags(string content)
        {
            // Create the error message for both empty and incomplete tags
            List<string> errorMessages = new List<string>();
            List<string> emptyTagLines = new List<string>();
            //List<int> missingCloseTagLines = new List<int>();
            //List<int> missingOpenTagLines = new List<int>();

            // Split the content by lines
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Regex to match <normal> tags (with or without content)
            var normalpattern = @"<normal[^>]*>(.*?)</normal\s*[^>]*>";
            var normalregex = new Regex(normalpattern, RegexOptions.IgnoreCase);

            //// Regex to match <b> tags (with or without content)
            //var Bpattern = @"<b[^>]*>(.*?)</b\s*[^>]*>";
            //var Bregex = new Regex(Bpattern, RegexOptions.IgnoreCase);

            //// Regex to match <normal> tag only (missing close tag "</normal>")
            //var missingCloseTagPattern = @"<normal[^>]*>[^<]*$";

            //// Regex to match </normal> tag (missing opening <normal>)
            //var closingTagOnlyPattern = @"</normal[^>]*>";

            // Iterate through each line and check for empty <normal> tags
            for (int i = 0; i < lines.Length; i++)
            {
                var normalemptymatch = normalregex.Match(lines[i]);
                //var Bemptymatch = Bregex.Match(lines[i]);
                //var missingCloseTag = Regex.Match(lines[i], missingCloseTagPattern);
                //var closingTagOnlyMatch = Regex.Match(lines[i], closingTagOnlyPattern);

                // If the line contains a <normal> tag with no content between the tags
                if (normalemptymatch.Success && string.IsNullOrWhiteSpace(normalemptymatch.Groups[1].Value))
                {
                    //emptyTagLines.Add(i + 1); // Store line number (1-based index)
                    emptyTagLines.Add($"Line {i + 1} = <normal> Empty Tag");
                }

                //// If the line contains a <b> tag with no content between the tags
                //if (Bemptymatch.Success && string.IsNullOrWhiteSpace(Bemptymatch.Groups[1].Value))
                //{
                //    //emptyTagLines.Add(i + 1); // Store line number (1-based index)
                //    emptyTagLines.Add($"Line {i + 1} = <b> Empty Tag");
                //}
                

                //if (missingCloseTag.Success)
                //{
                //    missingCloseTagLines.Add(i + 1); // Store line number (1-based index)
                //}

                //// If the line contains a </normal> tag but no previous opening <normal> tag
                //if (closingTagOnlyMatch.Success && !lines.Take(i + 1).Any(line => line.Contains("<normal")))
                //{
                //    missingOpenTagLines.Add(i + 1); // Store line number (1-based index)
                //}
            }



            // Add empty tag errors if any
            //if (emptyTagLines.Count > 0)
            //{
            //    errorMessages.Add($"Empty <normal> tags found on line(s): {string.Join(", ", emptyTagLines)}");
            //}

            //// Add incomplete tag errors if any
            //if (missingCloseTagLines.Count > 0)
            //{
            //    errorMessages.Add($"Missing </normal> tags found on line(s): {string.Join(", ", missingCloseTagLines)}");
            //}

            //// Add missing opening <normal> tag errors if any (where </normal> exists without an opening <normal>)
            //if (missingOpenTagLines.Count > 0)
            //{
            //    errorMessages.Add($"Missing <normal> tags found before </normal> on line(s): {string.Join(", ", missingOpenTagLines)}");
            //}

            // Return combined error messages or null if no errors
            //return errorMessages.Count > 0 ? string.Join("\n", errorMessages) : null;
            //return string.Join(Environment.NewLine, emptyTagLines);
            return emptyTagLines.Count > 0 ? string.Join("\n", emptyTagLines) : null;
        }



        /// <summary>
        /// Automatically save the cleaned content to a new file
        /// </summary>
        /// <param name="cleanedContent"></param>
        private void SaveCleanedFile(string cleanedContent)
        {
            try
            {
                //Combine filename with date in output filename
                string cleanedFileName = System.IO.Path.GetFileNameWithoutExtension(FileFullPath) + $"_Cleaned_{DateTime.Now:MM-dd-yyyy_HH-mm-ss}.txt";
                string cleanedFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FileFullPath), cleanedFileName);

                //Create new cleaned .txt
                File.WriteAllText(cleanedFilePath, cleanedContent);

                InformationMessage($"Cleaned file saved to: {cleanedFilePath}", "Successful Process");
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private void SaveErrorFile(string errorContent)
        {
            try
            {
                //Combine filename with date in output filename
                string errorFileName = System.IO.Path.GetFileNameWithoutExtension(FileFullPath) + $"_ErrorReport_{DateTime.Now:MM-dd-yyyy_HH-mm-ss}.txt";
                string errorFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FileFullPath), errorFileName);

                //Create new cleaned .txt
                File.WriteAllText(errorFilePath, errorContent);

                //InformationMessage($"Error file saved to: {errorFilePath}", "Successful");
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }


        // Method to save the cleaned text to a new file
        //private void SaveCleanedFile()
        //{
        //    try
        //    {
        //        // Prompt the user to select a location to save the file
        //        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        //        {
        //            Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
        //            DefaultExt = ".txt"
        //        };

        //        bool? result = saveFileDialog.ShowDialog();
        //        if (result == true)
        //        {
        //            // Get the path of the file where the user wants to save
        //            string savePath = saveFileDialog.FileName;

        //            // Write the cleaned content to the selected file
        //            File.WriteAllText(savePath, CleanedFileContent);

        //            // Optionally, notify the user that the file was saved
        //            InformationMessage($"File saved successfully to: {savePath}", "Successfull");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WarningMessage($"Error saving file: {ex.Message}");
        //    }
        //}

        // Method to find the text between <normal> and </normal> tags
        //private string FindNormalTagsContent(string content)
        //{
        //    // Regex pattern to match content between <normal> and </normal>
        //    //var pattern = @"<normal>(.*?)</normal>";
        //    //var pattern = @"<normal[^>]*>(.*?)</normal>";
        //    var pattern = @"<normal[^>]*>(.*?)</normal\s*[^>]*>";
        //    var regex = new Regex(pattern, RegexOptions.IgnoreCase);

        //    // Find matches
        //    var match = regex.Match(content);

        //    if (match.Success)
        //    {
        //        // Return the text between the <normal> and </normal> tags
        //        return match.Groups[1].Value;
        //    }
        //    else
        //    {
        //        return null; // No match found
        //    }
        //}
    }
}
