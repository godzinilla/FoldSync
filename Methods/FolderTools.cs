using System.Security.Cryptography;
using System.Text;

namespace FoldSync.Methods
{
    public class FolderTools
    {
		#region class instances
        Logger logger = new Logger();
		CommonTools commonTools = new CommonTools();
		#endregion

		#region methods
		/// <summary>
		/// Creates directory in given path when directory does not exists.
		/// </summary>
		/// <param name="path">Absolute path to the directory.</param>
		/// <returns>True if directory does not exists and was created.</returns>
		public bool CreateFolder(string path, bool generateLog = true)
        {
			//check if given path is not empty
			if (string.IsNullOrWhiteSpace(path))
            {
				commonTools.PrintMessage("Empty path is not a correct path!");
				commonTools.PrintMessage("Please provide the full path again:");
				if (generateLog) logger.GenerateCommonLog("WARNING", "RETRY", "input", "empty path provided", path, console: true, file: true);
				return false;
            }
			//check if directory exists
            else if (!Directory.Exists(path))
            {
                if (VerifyPath(path, generateLog))
                {
                    if (TryToCreateFolder(path, generateLog))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
			//confirm that path is correct with ability to enter the path again
            else
            {
				if (generateLog) logger.GenerateCommonLog("INFO", "OK", "verify", "correct path provided", path, console: true, file: true);
				commonTools.PrintMessage($"Path '{path}' already exists. Press any key if you want to use this path, othwerwise press [Esc] to enter the path again.");
                var input = Console.ReadKey(true);
				if (input.Key == ConsoleKey.Escape)
				{
					commonTools.PrintMessage("# [Esc] key has been pressed #");
					commonTools.PrintMessage("Please provide the full path again:");
					if (generateLog) logger.GenerateCommonLog("INFO", "OK", "input", "retrying entry", path, console: true, file: true);
					return false;
				}
				else
				{
					if (generateLog) logger.GenerateCommonLog("INFO", "OK", "input", "path selected", path, console: true, file: true);
					return true;
				}
            }
        }

		/// <summary>
		/// Verifies if given path to directory exists and is correct.
		/// </summary>
		/// <param name="path">Absolute path to the directory.</param>
		/// <returns>True if directory exists and the path to it is correct.</returns>
		public bool VerifyFolder(string path)
        {
			//check if given path is not empty
			if (string.IsNullOrWhiteSpace(path))
            {
				commonTools.PrintMessage("Empty path is not a correct path!");
				commonTools.PrintMessage("Please provide the full path again:");
				logger.GenerateCommonLog("WARNING", "RETRY", "input", "empty path", path, console: true, file: true);
				return false;
            }

			//path verification
            if (VerifyPath(path, false))
            {
				//check if directory exists - not existing directroy is not allowed for a source path
                if (!Directory.Exists(path))
                {
					commonTools.PrintMessage($"Given path '{path}' does not exists!");
					commonTools.PrintMessage("Please provide the full path again:");
					logger.GenerateCommonLog("WARNING", "RETRY", "input", "path does not exists", path, console: true, file: true);
					return false;
                }
                else
                {
					//confirm that path is correct with ability to enter the path again
					commonTools.PrintMessage($"Path '{path}' has been found. Press any key if you want to use this path, othwerwise press [Esc] to enter the path again.");
                    var input = Console.ReadKey(true);
                    if (input.Key == ConsoleKey.Escape)
                    {
                        commonTools.PrintMessage("# [Esc] key has been pressed #");
                        commonTools.PrintMessage("Please provide the full path again:");
						logger.GenerateCommonLog("INFO", "OK", "input", "retrying entry", path, console: true, file: true);
						return false;
                    }
                    else
                    {
						logger.GenerateCommonLog("INFO", "OK", "input", "path selected", path, console: true, file: true);
						return true;
                    }
                }
            }
            else
            {
				//incorrect path provided
				commonTools.PrintMessage($"Given path '{path}' is incorrect!");
				commonTools.PrintMessage("Please provide the correct path again:");
				logger.GenerateCommonLog("WARNING", "RETRY", "input", "incorrect path", path, console: true, file: true);
				return false;
            }
        }

        /// <summary>
        /// Verifies if given path is correct.
        /// </summary>
        /// <param name="path">Absolute path to the directory.</param>
        /// <param name="showMessage">Printing output message, default true.</param>
        /// <returns>True if given path is correct.</returns>
        private bool VerifyPath(string path, bool showMessage = true, bool generateLog = true)
        {
			//check if given path is not empty
			if (string.IsNullOrWhiteSpace(path))
            {
				//the showMessage statement is used to preventing duplicating the messages on console
                if (showMessage)
                {
					commonTools.PrintMessage($"Given path '{path}' is incorrect!");
					commonTools.PrintMessage("Please provide the correct path again:");
				}
				//the generateLog statemen is used not print the log message to a file when the file not exists yet (eg. in Logger.CreateLogFile method) 
				if (generateLog) logger.GenerateCommonLog("WARNING", "RETRY", "verify", "incorrect path provided", path, console: true, file: true);
				return false;
            }
			//check if given path is no a network path
            if (path.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
            {
                if (showMessage)
                {
					commonTools.PrintMessage($"Given path '{path}' is incorrect! Network paths are not supported.");
					commonTools.PrintMessage("Please provide the correct path again:");
				}
				if (generateLog) logger.GenerateCommonLog("WARNING", "RETRY", "verify", "network path provided", path, console: true, file: true);
				return false;
            }
			//check that the given path is absolute to avoid creating the folder in an unintended directory eg. where the program is executed
			if (path.Length < 3 || path[1] != ':' || (path[2] != '\\' && path[2] != '/'))
            {
                if (showMessage)
                {
					commonTools.PrintMessage($"Given path '{path}' is incorrect! Use absolute path like 'drive_letter:\\folder1\\folder2'.");
					commonTools.PrintMessage("Please provide the correct path again:");
				}
				if (generateLog) logger.GenerateCommonLog("WARNING", "RETRY", "verify", "non-absolute path provided", path, console: true, file: true);
				return false;
            }

			//check if given path contains drive letter exists in the system
            string driveLetter = char.ToUpperInvariant(path[0]) + @":\";
            if (DriveInfo.GetDrives()
                .Any(driveInfo => string.Equals(driveInfo.Name, driveLetter, StringComparison.OrdinalIgnoreCase)))
            {
				//confirm that the correct path is provided
				if (generateLog) logger.GenerateCommonLog("INFO", "OK", "verify", "correct path provided", path, console: true, file: true);
				return true;
            }
            else
            {
                if (showMessage)
                {
					commonTools.PrintMessage($"Given path '{path}' contains drive letter which does not exists in the system!");
					commonTools.PrintMessage("Please provide the correct path again:");
				}
				if (generateLog) logger.GenerateCommonLog("WARNING", "RETRY", "verify", "incorrect drive provided", path, console: true, file: true);
				return false;
            }
        }

        /// <summary>
        /// Tries to create the directory in given path using DirectoryInfo class.
        /// </summary>
        /// <param name="path">Absolute path to the directory.</param>
        /// <returns>True if directory was created correctly.</returns>
        private bool TryToCreateFolder(string path, bool generateLog = true)
        {
			//used for dealing with the possible exceptions encountred while creating a directories
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            try
            {
                dirInfo.Create();
                commonTools.PrintMessage($"Directory '{path}' has been created");
				if (generateLog) logger.GenerateCommonLog("INFO", "OK", "create", "directory created", path, console: true, file: true);
				return true;
            }
            catch (Exception ex)
            {
                commonTools.PrintMessage($"Given path '{path}' caused the following exception: '{ex.Message}'!");
                commonTools.PrintMessage("Please provide other path:");
				if (generateLog) logger.GenerateCommonLog("WARNING", "RETRY", "create", "fail to create directory", path, ex, console: true, file: true);
				return false;
            }
		}

		/// <summary>
		/// Creates list of all files in given directory and its subdirectories.
		/// </summary>
		/// <param name="path">Absolute path to the root directory.</param>
		/// <returns>Two dimensional Array with following data of the file: name, child directories, SHA256 value, absolute path. If there is an empty directory the SHA256 value will be empty.</returns>
		public string[,] GenerateFilesList(string path)
		{
			//getting all the files, directories and subdirectories, including the empty ones
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			FileInfo[] allFIles = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
			DirectoryInfo[] subDirectories = directoryInfo.GetDirectories("*", SearchOption.AllDirectories);
			List<DirectoryInfo> allDirectories = new List<DirectoryInfo>(subDirectories.Length + 1) { directoryInfo };
			allDirectories.AddRange(subDirectories);
			List<DirectoryInfo> emptyDirectories = new List<DirectoryInfo>();
			foreach (DirectoryInfo dir in allDirectories)
			{
				if (!dir.EnumerateFiles("*", SearchOption.AllDirectories).Any())
					emptyDirectories.Add(dir);
			}

			int totalRows = allFIles.Length + emptyDirectories.Count;
			string[,] files = new string[totalRows, 4];

			//collecting the data for all found files
			for (int i = 0; i < allFIles.Length; i++)
			{
				files[i, 0] = allFIles[i].Name;
				files[i, 1] = allFIles[i].DirectoryName;
				files[i, 2] = GetFileHash(allFIles[i]);
				files[i, 3] = allFIles[i].FullName;
			}

			//collecting the data for all found empty directories
			//setting the empty hash value for empty directories for identification in further steps
			for (int i = 0; i < emptyDirectories.Count; i++)
			{
				DirectoryInfo directoryInfoEmptyDirs = emptyDirectories[i];
				files[i, 0] = directoryInfoEmptyDirs.Name;
				files[i, 1] = directoryInfoEmptyDirs.Parent != null ? directoryInfoEmptyDirs.Parent.FullName : directoryInfoEmptyDirs.FullName;
				files[i, 2] = string.Empty;
				files[i, 3] = directoryInfoEmptyDirs.FullName;
			}
			return files;
		}

		/// <summary>
		/// Generates SHA256 hash value for given file.
		/// </summary>
		/// <param name="file">File to generate a SHA256 value for.</param>
		/// <returns>SHA256 value as string for the given file.</returns>
		public string GetFileHash(FileInfo file)
        {
			SHA256 hash = SHA256.Create();
			FileStream fileStream = new FileStream(file.FullName, FileMode.Open);
			//computing the hash value for given file
            byte[] value = hash.ComputeHash(fileStream);
			//generating the hash value for given file in readable string
			StringBuilder hashValue = new StringBuilder();
			for (int i = 0; i < value.Length; i++)
			{
				//x2 means the byte value should be converted to hex value and for single byte value use two hex values
				hashValue.Append(value[i].ToString("x2"));
			}
            fileStream.Close();
			return hashValue.ToString();
		}
        #endregion
    }
}
