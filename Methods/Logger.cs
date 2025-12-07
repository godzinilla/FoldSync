namespace FoldSync.Methods
{
    public class Logger
    {
		#region class instances
		CommonTools commonTools = new CommonTools();
		#endregion

		#region variables
		public static string logFilePath = string.Empty;
		#endregion

		#region methods
		/// <summary>
		/// Creates a new log file in a given directory.
		/// If the log file already exists, it will be used (and not be overwritten).
		/// </summary>
		public void CreateLogFile()
		{
			FolderTools folderTools = new FolderTools();
			string
				logFileName = "fslog_" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt",
				logFolderPath = string.Empty;
			do
			{
				//waiting for valid folder path
				do logFolderPath = commonTools.UserInput();
				while (!folderTools.CreateFolder(logFolderPath, false));

				//veryfing log file creation (new or existing)
				string newLogFilePath = Path.Combine(logFolderPath, logFileName);
				bool newFileCreated = false;
				try
				{
					if (!File.Exists(newLogFilePath))
					{
						File.Create(newLogFilePath).Close();
						newFileCreated = true;
					}
					logFilePath = Path.GetFullPath(newLogFilePath);
				}
				catch (UnauthorizedAccessException)
				{
					commonTools.PrintMessage("You don't have write access to this directory!");
					commonTools.PrintMessage("Please provide other absolute path to directory for the log file:");
					continue;
				}
				catch (Exception ex)
				{
					commonTools.PrintMessage($"Unexpected error when creating the log file: '{ex.Message}'");
					commonTools.PrintMessage("Please provide other absolute path to directory for the log file:");
					continue;
				}

				//confirming log file creation
				if (File.Exists(logFilePath))
				{
					this.GenerateCommonLog("INFO", "OK", "start", "program started", "", console: false, file: true);
					this.GenerateCommonLog("INFO", newFileCreated ? "OK" : "SKIP", "create", newFileCreated ? "log: file created" : "log: file exists", logFilePath, console: true, file: true);
					commonTools.PrintMessage($"LOG file '{logFileName}' {(newFileCreated ? "has been created" : "already exists")} in directory '{logFolderPath}'");
				}
			}
			while (!File.Exists(logFilePath));
		}

		/// <summary>
		/// Generate log entry for file operations
		/// </summary>
		/// <param name="level">Log level</param>
		/// <param name="status">Task status</param>
		/// <param name="action">Performed action</param>
		/// <param name="fileName">File name</param>
		/// <param name="dir">Directory</param>
		/// <param name="ex">Exception, default null</param>
		/// <param name="console">Decide if log entry should be printed in the Console, default true</param>
		/// <param name="file">Decide if log entry should be printed in the log file, default true</param>
		public void GenerateFileLog(string level, string status, string action, string fileName, string dir, Exception? ex = null, bool console = true, bool file = true)
		{
			//time stamp for the log entry, e.g. [14:23:45.1234] (4 digits of miliseconds used for better precision)
			string logTimeStamp = String.Join("", "[", DateTime.Now.ToString("HH:mm:ss.ffff"), "]");

			//building the log entry with fixed width columns for better readability
			string logEntry = string.Format(
				"{0,-15} {1,-5} {2,-7} {3,-10} name={4} dir={5}",
				logTimeStamp,
				level,
				status,
				action,
				fileName,
				dir
			);

			//adding an exception message if provided
			if (ex != null)
			{
				logEntry += $" | ex: {ex.GetType().Name}: {ex.Message}";
			}

			//print message to the console and/or log file based on parameters
			if (console)
			{
				commonTools.PrintMessage(logEntry);
			}
			if (file)
			{
				if (File.Exists(logFilePath))
				{
					File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
				}
			}
		}

		/// <summary>
		/// Generate common log entry
		/// </summary>
		/// <param name="level">Log level</param>
		/// <param name="status">Task status</param>
		/// <param name="action">Performed action</param>
		/// <param name="message">Custom message</param>
		/// <param name="directory">Directory, default empty</param>
		/// <param name="ex">Exception, default null</param>
		/// <param name="console">Decide if log entry should be printed in the Console, default true</param>
		/// <param name="file">Decide if log entry should be printed in the log file, default true</param>
		public void GenerateCommonLog(string level, string status, string action, string message, string directory = "", Exception? ex = null, bool console = true, bool file = true)
		{
			//time stamp for the log entry, e.g. [14:23:45.1234] (4 digits of miliseconds used for better precision)
			string logTimeStamp = String.Join("", "[", DateTime.Now.ToString("HH:mm:ss.ffff"), "]");

			//building the log entry with fixed width columns for better readability
			string logEntry = string.Format(
				"{0,-15} {1,-5} {2,-7} {3,-10} message={4}",
				logTimeStamp,
				level,
				status,
				action,
				message
			);

			//adding directory info if provided
			if (!string.IsNullOrEmpty(directory))
			{
				logEntry += $" dir={directory}";
			}

			//adding an exception message if provided
			if (ex != null)
			{
				logEntry += $" | ex: {ex.GetType().Name}: {ex.Message}";
			}

			//print message to the console and/or log file based on parameters
			if (console)
			{
				commonTools.PrintMessage(logEntry);
			}
			if (file)
			{
				if (File.Exists(logFilePath))
				{
					File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
				}
			}
		}
		#endregion
	}
}
