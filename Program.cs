using FoldSync.Methods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FoldSync
{
    class Program
    {
		public static async Task Main()
		{
			#region class instances
			CommonTools commonTools = new CommonTools();
			FolderTools folderTools = new FolderTools();
			Synchronizer synchronizer = new Synchronizer();
			Logger logger = new Logger();
			#endregion

			#region variables declaration
			string sourcePath = string.Empty,
				targetPath = string.Empty,
				tempStr = string.Empty,
				result = string.Empty;
			int schedulerTime = -1;
			DateTime dtNow = new DateTime();
			string[,] sourceFiles;
			#endregion

			#region header
			commonTools.PrintMessage(@".-------------------------.                                         .-----------------------------------.
|                         |   This   program   will   synchronize   |                                   |
|   ____ ____ ____ ____   |   two        given        directories   |                                   |
|  ||F |||O |||L |||D ||  |   (source         with        target)   |                                   |
|  ||__|||__|||__|||__||  |   in    given    scheduler    period.   |   ____ ____ ____ ____ ____ ____   |
|  |/__\|/__\|/__\|/__\|  |                                         |  ||E |||N |||J |||O |||Y |||! ||  |
|   ____ ____ ____ ____   |   The    log    file   with   results   |  ||__|||__|||__|||__|||__|||__||  |
|  ||S |||Y |||N |||C ||  |   will be created in given directory.   |  |/__\|/__\|/__\|/__\|/__\|/__\|  |
|  ||__|||__|||__|||__||  |                                         |                                   |
|  |/__\|/__\|/__\|/__\|  |   Please follow the instructions below  |                                   |
|                         |                                  and... |                                   |
'-------------------------'                                         '-----------------------------------'");
			#endregion

			#region main program

			#region LOG directory
			logger.GenerateCommonLog("INFO", "OK", "start", message: "application start", console: true, file: true);
			commonTools.PrintMessage(Environment.NewLine + ".-------------------------------------------------L-O-G-------------------------------------------------.");
			commonTools.PrintMessage("Please provide absolute path to directory where the log file should be created:");
			logger.CreateLogFile();
			#endregion

			#region SOURCE directory
			commonTools.PrintMessage(Environment.NewLine + ".-----------------------------------------------S-O-U-R-C-E----------------------------------------------.");
			commonTools.PrintMessage("Please provide below the full path to the SOURCE directory:");
			do { sourcePath = commonTools.UserInput(); }
			while (!folderTools.VerifyFolder(sourcePath));
			#endregion

			#region TARGET directory
			commonTools.PrintMessage(Environment.NewLine + ".-----------------------------------------------T-A-R-G-E-T----------------------------------------------.");
			commonTools.PrintMessage("Please provide below the full path to the TARGET directory:");
			do targetPath = commonTools.UserInput();
			while (!folderTools.CreateFolder(targetPath));
			#endregion

			#region setting the SHEDULER
			commonTools.PrintMessage(Environment.NewLine + ".--------------------------------------------S-C-H-E-D-U-L-E-R-------------------------------------------.");
			commonTools.PrintMessage("Please provide how often the task should repeat (in minutes, type 0 to skip scheduler):");
			do {
				result = commonTools.UserInput();
				if (int.TryParse(result, out schedulerTime))
				{
					if (schedulerTime == 0)
					{
						commonTools.PrintMessage("Scheduler has not been set, the task will be performed one time");
						logger.GenerateCommonLog("INFO", "OK", "scheduler", "scheduler not set", console: true, file: true);
					}
					else if (schedulerTime < 0)
					{
						commonTools.PrintMessage($"Negative value cannot be use to set scheduler time, please enter the value again:");
						logger.GenerateCommonLog("ERROR", "RETRY", "scheduler", "negative number entered", console: true, file: true);
					}
					else
					{
						commonTools.PrintMessage($"Scheduler has been set, the task will be performed each '{schedulerTime}' minutes.");
						logger.GenerateCommonLog("INFO", "OK", "scheduler", $"scheduler set to '{schedulerTime}' minutes", console: true, file: true);
					}
				}
				else
				{
					commonTools.PrintMessage($"'{result}' is not a valid number, please enter the value again.");
					logger.GenerateCommonLog("ERROR", "RETRY", "scheduler", "invalid number entered", console: true, file: true);
					schedulerTime = -1;
				}
			} while (schedulerTime < 0);
			#endregion

			#region sync and repeat process
			bool userEscaped = false;
			do
			{
				dtNow = DateTime.Now;

				sourceFiles = folderTools.GenerateFilesList(sourcePath);
				synchronizer.SynchronizeFiles(sourceFiles, sourcePath, targetPath);

				if (schedulerTime == 0)
					break;

				commonTools.PrintMessage(Environment.NewLine + ".-------------------------------------W-A-I-T-I-N-G--F-O-R--N-E-X-T--R-U-N------------------------------------.");
				commonTools.PrintMessage("Please press [Enter] to start next synchronization cycle or [Esc] to close the application.");
				commonTools.PrintMessage(System.String.Join("", "Next synchronization cycle will start in ", schedulerTime, " minutes automatically (at ", DateTime.Now.AddMinutes(schedulerTime).ToString("HH:mm:ss"), ")"));
				var delayTask = synchronizer.WaitForNextRun(schedulerTime, dtNow);
				var keyTask = CommonTools.WaitForKeyPressAsync();

				var finisedTask = await Task.WhenAny(delayTask, keyTask);

				if (finisedTask == keyTask)
				{
					var key = keyTask.Result;
					if (key == ConsoleKey.Escape)
					{
						userEscaped = true;
						break;
					}
				}
			} while (!userEscaped);
			#endregion

			#region ending
			TimeSpan timeAmount = new TimeSpan();
            timeAmount = DateTime.Now - synchronizer.jobStartDT;

			logger.GenerateCommonLog("INFO", "OK", "end", System.String.Join(" ", "job completed, job take", tempStr), console: true, file: true);
			tempStr = commonTools.ConvertTimeSpanToString(timeAmount);
			commonTools.PrintMessage($"Current run of the job ended. Please [Esc] key to close the aplication.");
			while (true)
			{
				var keyInfo = Console.ReadKey(intercept: true);
				if (keyInfo.Key == ConsoleKey.Escape)
				{
					logger.GenerateCommonLog("INFO", "OK", "close", "application closed by user", console: true, file: true);
					break;
				}
			}
			#endregion
			
			#endregion
		}
    }
}