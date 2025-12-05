namespace FoldSync.Methods
{
    public class Synchronizer
    {
		#region class instances
		CommonTools commonTools = new CommonTools();
		FolderTools folderTools = new FolderTools();
		Logger logger = new Logger();
		#endregion

		#region variables
		public DateTime jobStartDT = new DateTime();
		#endregion

		#region methods
		/// <summary>
		/// Synchronize files from source directory to target directory based on the given array of source files.
		/// </summary>
		/// <param name="sourceFiles">Two dimensional Array with all source files and folders (including empty directories)</param>
		/// <param name="sourcePath">Absolute path to source directory</param>
		/// <param name="targetPath">Absolute path to target directory</param>
		public void SynchronizeFiles(string[,] sourceFiles, string sourcePath, string targetPath)
        {
			#region variables
			int count = 0;
			bool success = false;
			FileInfo fileInfoSource;
			FileInfo fileInfoTarget;
			string relativePath = string.Empty;
			string childDirectories = string.Empty;
			string targetFileHash = string.Empty;
			#endregion

			commonTools.PrintMessage(Environment.NewLine + ".------------------------------------S-Y-N-C--P-R-O-C-E-S-S--S-T-A-R-T-----------------------------------.");
			jobStartDT = DateTime.Now;
			logger.GenerateCommonLog("INFO", "OK", "start", String.Join("", "job started, start time:", jobStartDT.ToString("HH:mm:ss:ffff")), console: true, file: true);
			for (int i = 0; i < sourceFiles.Length / 4; i++)
            {
				//check if file is an empty directory (empty directory has empty hash code)
				if (string.IsNullOrEmpty(sourceFiles[i, 2]))
                {
                    var dirFullPath = sourceFiles[i, 3];
                    var relativeDir = Path.GetRelativePath(sourcePath, dirFullPath);
                    var targetDir = Path.Combine(targetPath, relativeDir);

					//create empty directory in target path
					if (!Directory.Exists(targetDir))
                    {
						do
						{
							try
							{
								Directory.CreateDirectory(targetDir);
								success = true;
								break;
							}
							catch (Exception ex)
							{
								logger.GenerateFileLog("WARNING", "RETRY", "create", targetDir, sourceFiles[i, 1], ex);
								count++;
								if (count == 4)
								{
									logger.GenerateFileLog("ERROR", "FAIL", "create", targetDir, sourceFiles[i, 1], ex);
									success = false;
								}
							}
						}
						while (count < 5);
						count = 0;
						if (!success)
						{
							continue;
						}
						success = false;
						if (Directory.Exists(targetDir))
						{
							logger.GenerateFileLog("INFO", "OK", "create", targetDir, sourceFiles[i, 1]);
						}
						else
						{
							logger.GenerateFileLog("ERROR", "FAIL", "create", targetDir, sourceFiles[i, 1]);
						}
					}
                    else
                    {
						logger.GenerateFileLog("INFO", "SKIP", "create", targetDir, sourceFiles[i, 1]);
					}
                    continue;
                }

				fileInfoSource = new FileInfo(sourceFiles[i, 3]);
                relativePath = Path.GetRelativePath(sourcePath, fileInfoSource.FullName);
				childDirectories = Path.GetDirectoryName(relativePath);

				//check if directory name attribute is empty (file is in the root of source path)
				if (String.IsNullOrEmpty(childDirectories))
                {
					//verify if the file with the same name as source file exists in target path
					if (File.Exists(Path.Combine(targetPath, sourceFiles[i, 0])))
                    {
						//verify hash codes of both files
						fileInfoTarget = new FileInfo(Path.Combine(targetPath, sourceFiles[i, 0]));
						targetFileHash = folderTools.GetFileHash(fileInfoTarget);
						if (targetFileHash != sourceFiles[i, 2])
						{
							//if hash codes are different - overwrite target file with source file
							do
							{
								try
								{
									File.Copy(sourceFiles[i, 3], Path.Combine(targetPath, sourceFiles[i, 0]), true);
									success = true;
									break;
								}
								catch (Exception ex)
								{
									logger.GenerateFileLog("WARNING", "RETRY", "replace", sourceFiles[i, 0], sourceFiles[i, 1], ex);
									count++;
									if (count == 4)
									{
										logger.GenerateFileLog("ERROR", "FAIL", "replace", sourceFiles[i, 0], sourceFiles[i, 1], ex);
										success = false;
									}
								}
							}
							while (count < 5);
							count = 0;
							if (!success)
							{
								continue;
							}
							success = false;
							//verify if file has been copied to target path and the hash codes are the same
							if (File.Exists(Path.Combine(targetPath, sourceFiles[i, 0])))
							{
								fileInfoTarget = new FileInfo(Path.Combine(targetPath, sourceFiles[i, 0]));
								targetFileHash = folderTools.GetFileHash(fileInfoTarget);
								if (targetFileHash == sourceFiles[i, 2])
								{
									logger.GenerateFileLog("INFO", "OK", "replaced", sourceFiles[i, 0], sourceFiles[i, 1]);
								}
								else
								{
									logger.GenerateFileLog("ERROR", "SHA256", "replaced", sourceFiles[i, 0], sourceFiles[i, 1]);
								}
							}
							else
							{
								logger.GenerateFileLog("ERROR", "MISSING", "replace", sourceFiles[i, 0], sourceFiles[i, 1]);
							}
						}
						else
						{
							logger.GenerateFileLog("INFO", "SKIP", "replace", sourceFiles[i, 0], sourceFiles[i, 1]);
						}
                    }
                    else
                    {
						//copy source file to target path
						do
						{
							try
							{
								File.Copy(sourceFiles[i, 3], Path.Combine(targetPath, sourceFiles[i, 0]), false);
								success = true;
								break;
							}
							catch (Exception ex)
							{
								logger.GenerateFileLog("WARNING", "RETRY", "copy", sourceFiles[i, 0], sourceFiles[i, 1], ex);
								count++;
								if (count == 4)
								{
									logger.GenerateFileLog("ERROR", "FAIL", "copy", sourceFiles[i, 0], sourceFiles[i, 1], ex);
									success = false;
								}
							}
						}
						while (count < 5);
						count = 0;
						if (!success)
						{
							continue;
						}
						success = false;
						//verify if file has been copied to target path and the hash codes are the same
						if (File.Exists(Path.Combine(targetPath, sourceFiles[i, 0])))
						{
							fileInfoTarget = new FileInfo(Path.Combine(targetPath, sourceFiles[i, 0]));
							targetFileHash = folderTools.GetFileHash(fileInfoTarget);
							if (targetFileHash == sourceFiles[i, 2])
							{
								logger.GenerateFileLog("INFO", "OK", "copied", sourceFiles[i, 0], sourceFiles[i, 1]);
							}
							else
							{
								logger.GenerateFileLog("ERROR", "SHA256", "copied", sourceFiles[i, 0], sourceFiles[i, 1]);
							}
						}
						else
						{
							logger.GenerateFileLog("ERROR", "MISSING", "copy", sourceFiles[i, 0], sourceFiles[i, 1]);
						}
					}
				}
                else
                {
					//check if child directories exist in target path, if not - create them
					if (!Directory.Exists(Path.Combine(targetPath, childDirectories)))
                    {
						//create directory and copy source file to target path
						do
						{
							try
							{
								Directory.CreateDirectory(Path.Combine(targetPath, childDirectories));
								success = true;
								break;
							}
							catch (Exception ex)
							{
								logger.GenerateFileLog("WARNING", "RETRY", "create", sourceFiles[i, 0], sourceFiles[i, 1], ex);
								count++;
								if (count == 4)
								{
									logger.GenerateFileLog("ERROR", "FAIL", "create", sourceFiles[i, 0], sourceFiles[i, 1], ex);
									success = false;
								}
							}
						}
						while (count < 5);
						count = 0;
						if (!success)
						{
							continue;
						}
						success = false;
						do
						{
							try
							{
								File.Copy(sourceFiles[i, 3], Path.Combine(targetPath, childDirectories, sourceFiles[i, 0]), false);
								success = true;
								break;
							}
							catch (Exception ex)
							{
								logger.GenerateFileLog("WARNING", "RETRY", "copy", sourceFiles[i, 0], sourceFiles[i, 1], ex);
								count++;
								if (count == 4)
								{
									logger.GenerateFileLog("ERROR", "FAIL", "copy", sourceFiles[i, 0], sourceFiles[i, 1], ex);
									success = false;
								}
							}
						}
						while (count < 5);
						count = 0;
						if (!success)
						{
							continue;
						}
						success = false;
						//verify if file has been copied to target path and the hash codes are the same
						if (File.Exists(Path.Combine(targetPath, childDirectories, sourceFiles[i, 0])))
						{
							fileInfoTarget = new FileInfo(Path.Combine(targetPath, childDirectories, sourceFiles[i, 0]));
							targetFileHash = folderTools.GetFileHash(fileInfoTarget);
							if (targetFileHash == sourceFiles[i, 2])
							{
								logger.GenerateFileLog("INFO", "OK", "copied", sourceFiles[i, 0], sourceFiles[i, 1]);
							}
							else
							{
								logger.GenerateFileLog("ERROR", "SHA256", "copied", sourceFiles[i, 0], sourceFiles[i, 1]);
							}
						}
						else
						{
							logger.GenerateFileLog("ERROR", "MISSING", "copy", sourceFiles[i, 0], sourceFiles[i, 1]);
						}
					}
                    else
                    {
						//verify if the file with the same name as source file exists in target path
						if (File.Exists(Path.Combine(targetPath, childDirectories, sourceFiles[i, 0])))
						{
							//verify hash codes of both files
							fileInfoTarget = new FileInfo(Path.Combine(targetPath, childDirectories, sourceFiles[i, 0]));
							targetFileHash = folderTools.GetFileHash(fileInfoTarget);
							if (targetFileHash != sourceFiles[i, 2])
							{
								//if hash codes are different - overwrite target file with source file
								do
								{
									try
									{
										File.Copy(sourceFiles[i, 3], Path.Combine(targetPath, childDirectories, sourceFiles[i, 0]), true);
										success = true;
										break;
									}
									catch (Exception ex)
									{
										logger.GenerateFileLog("WARNING", "RETRY", "replace", sourceFiles[i, 0], sourceFiles[i, 1], ex);
										count++;
										if (count == 4)
										{
											logger.GenerateFileLog("ERROR", "FAIL", "replace", sourceFiles[i, 0], sourceFiles[i, 1], ex);
											success = false;
										}
									}
								}
								while (count < 5);
								count = 0;
								if (!success)
								{
									continue;
								}
								success = false;
								//verify if file has been copied to target path and the hash codes are the same
								if (File.Exists(Path.Combine(targetPath, childDirectories, sourceFiles[i, 0])))
								{
									fileInfoTarget = new FileInfo(Path.Combine(targetPath, childDirectories, sourceFiles[i, 0]));
									targetFileHash = folderTools.GetFileHash(fileInfoTarget);
									if (targetFileHash == sourceFiles[i, 2])
									{
										logger.GenerateFileLog("INFO", "OK", "replaced", sourceFiles[i, 0], sourceFiles[i, 1]);
									}
									else
									{
										logger.GenerateFileLog("ERROR", "SHA256", "replaced", sourceFiles[i, 0], sourceFiles[i, 1]);
									}
								}
								else
								{
									logger.GenerateFileLog("ERROR", "MISSING", "replace", sourceFiles[i, 0], sourceFiles[i, 1]);
								}
							}
							else
							{
								logger.GenerateFileLog("INFO", "SKIP", "replace", sourceFiles[i, 0], sourceFiles[i, 1]);
							}
						}
						else
						{
							//copy source file to target path
							do
							{
								try
								{
									File.Copy(sourceFiles[i, 3], Path.Combine(targetPath, childDirectories, sourceFiles[i, 0]), false);
									success = true;
									break;
								}
								catch (Exception ex)
								{
									logger.GenerateFileLog("WARNING", "RETRY", "copy", sourceFiles[i, 0], sourceFiles[i, 1], ex);
									count++;
									if (count == 4)
									{
										logger.GenerateFileLog("ERROR", "FAIL", "copy", sourceFiles[i, 0], sourceFiles[i, 1], ex);
										success = false;
									}
								}
							}
							while (count < 5);
							count = 0;
							if (!success)
							{
								continue;
							}
							success = false;
							//verify if file has been copied to target path and the hash codes are the same
							if (File.Exists(Path.Combine(targetPath, childDirectories, sourceFiles[i, 0])))
							{
								fileInfoTarget = new FileInfo(Path.Combine(targetPath, childDirectories, sourceFiles[i, 0]));
								targetFileHash = folderTools.GetFileHash(fileInfoTarget);
								if (targetFileHash == sourceFiles[i, 2])
								{
									logger.GenerateFileLog("INFO", "OK", "copied", sourceFiles[i, 0], sourceFiles[i, 1]);
								}
								else
								{
									logger.GenerateFileLog("ERROR", "SHA256", "copied", sourceFiles[i, 0], sourceFiles[i, 1]);
								}
							}
							else
							{
								logger.GenerateFileLog("ERROR", "MISSING", "copy", sourceFiles[i, 0], sourceFiles[i, 1]);
							}
						}
					}
				}
			}
			commonTools.PrintMessage(Environment.NewLine + ".--------------------------------------S-Y-N-C--P-R-O-C-E-S-S--E-N-D-------------------------------------.");

			CleanUp(sourcePath, targetPath);
		}

		/// <summary>
		/// Scans the given source and target directories to retrieve their subdirectory structures and all files and remove unnecessary ones from the target directory.
		/// </summary>
		/// <param name="sourcePath">Absolute path to the source directory.</param>
		/// <param name="targetPath">Absolute path to the target directory.</param>
		public void CleanUp(string sourcePath, string targetPath)
		{
			#region variables
			int count = 0;
			DirectoryInfo directoryInfoSource, directoryInfoTarget;
			DirectoryInfo[] sourceDirectories, targetDirectories;
			FileInfo[] sourceFiles, targetFiles;
			#endregion

			commonTools.PrintMessage(Environment.NewLine + ".-------------------------------C-L-E-A-N-I-N-G--P-R-O-C-E-S-S--S-T-A-R-T--------------------------------.");

			//get all directories and files from both source and target paths
			directoryInfoSource = new DirectoryInfo(sourcePath);
			sourceDirectories = directoryInfoSource.GetDirectories("*", SearchOption.AllDirectories);
			directoryInfoTarget = new DirectoryInfo(targetPath);
			targetDirectories = directoryInfoTarget.GetDirectories("*", SearchOption.AllDirectories);
			sourceFiles = directoryInfoSource.GetFiles("*", SearchOption.AllDirectories);
			targetFiles = directoryInfoTarget.GetFiles("*", SearchOption.AllDirectories);

			//check if file from target path exists in source path, if not - delete it from target path
			foreach (FileInfo targetFile in targetFiles)
			{
				if (!File.Exists(Path.Combine(sourcePath, Path.GetRelativePath(targetPath, targetFile.FullName))))
				{
					do
					{
						try
						{
							File.Delete(targetFile.FullName);
							logger.GenerateFileLog("INFO", "OK", "deleted", targetFile.Name, sourcePath);
							break;
						}
						catch (Exception ex)
						{
							logger.GenerateFileLog("WARNING", "RETRY", "delete", targetFile.Name, sourcePath, ex);
							count++;
							if (count == 4)
							{
								logger.GenerateFileLog("ERROR", "FAIL", "delete", targetFile.Name, sourcePath, ex);
							}
						}
					}
					while (count < 5);
					count = 0;
				}
			}
			//check if directory from target path exists in source path, if not - delete it from target path
			foreach (DirectoryInfo targetDirectory in targetDirectories)
			{
				if (!Directory.Exists(Path.Combine(sourcePath, Path.GetRelativePath(targetPath, targetDirectory.FullName))))
				{
					do
					{
						try
						{
							Directory.Delete(targetDirectory.FullName, true);
							logger.GenerateFileLog("INFO", "OK", "deleted", targetDirectory.Name, sourcePath);
							break;
						}
						catch (DirectoryNotFoundException)
						{
							logger.GenerateFileLog("INFO", "SKIP", "delete", targetDirectory.Name, sourcePath);
							break;
						}
						catch (Exception ex)
						{
							logger.GenerateFileLog("WARNING", "RETRY", "delete", targetDirectory.Name, sourcePath, ex);
							count++;
							if (count == 4)
							{
								logger.GenerateFileLog("ERROR", "FAIL", "delete", targetDirectory.Name, sourcePath, ex);
							}
						}
					}
					while (count < 5);
					count = 0;
				}
			}
			commonTools.PrintMessage(Environment.NewLine + ".---------------------------------C-L-E-A-N-I-N-G--P-R-O-C-E-S-S--E-N-D----------------------------------.");
		}

		/// <summary>
		/// Asynchronously waits until the next scheduled run time, based on the specified interval and current time.
		/// </summary>
		/// <param name="shedulerTime">Time interval in minutes to wait before the next scheduled run.</param>
		/// <param name="dateTimeNow">The current date and time used as the reference point for calculating the next run time.</param>
		async public Task WaitForNextRun(int shedulerTime, DateTime dateTimeNow)
		{
			DateTime nextRunDateTime = dateTimeNow.AddMinutes(shedulerTime);
			TimeSpan delay = nextRunDateTime - DateTime.Now;
			if (delay > TimeSpan.Zero)
			{
				await Task.Delay(delay);
			}
		}
		#endregion
	}
}
