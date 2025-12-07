using System.Text;

namespace FoldSync.Methods
{
    public class CommonTools
    {
        #region methods
        /// <summary>
        /// Print given message using Console.WriteLine or Console.Write method.
        /// </summary>
        /// <param name="message">Message to be printed.</param>
        /// <param name="writeLine">If true, uses Console.WriteLine, else Console.Write. Default true.</param>
        public void PrintMessage(string message, bool writeLine = true)
        {
            if (writeLine) Console.WriteLine(message);
            else Console.Write(message);
        }

        /// <summary>
        /// To handle Console.ReadLine method. Press [Esc] key to close application (only on empty lines).
        /// </summary>
        /// <returns>String value of user input.</returns>
        public string UserInput()
        {
            Logger logger = new Logger();
            var userInput = new StringBuilder();
			this.PrintMessage("> ", writeLine: false);
			while (true)
            {
				var pressedKey = Console.ReadKey(intercept: true);
				//Check if pressed key is ESC - if so, close application (only on empty lines)
				if (pressedKey.Key == ConsoleKey.Escape)
                {
                    if (userInput.Length == 0)
                    {
                        logger.GenerateCommonLog("INFO", "OK", "close", "application closed by user", console: true, file: true);
                        Environment.Exit(0);
                    }
                }
				//Check if pressed key is ENTER - if so, return user input
				else if (pressedKey.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return userInput.ToString();
                }
                //Check if pressed key is BACKSPACE - if so, remove last character from user input
				else if (pressedKey.Key == ConsoleKey.Backspace)
                {
                    if (userInput.Length > 0)
                    {
                        userInput.Remove(userInput.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
				//Build user input string
				else
				{
                    userInput.Append(pressedKey.KeyChar);
                    Console.Write(pressedKey.KeyChar);
                }
            }
        }

        /// <summary>
        /// Convert the given TimeSpan value to string
        /// </summary>
        /// <param name="ts">TimeSpan value</param>
        /// <returns>Amount of time in readable string format</returns>
		public string ConvertTimeSpanToString(TimeSpan ts)
        {
            var days = $"{ts.Days} day{Plural(ts.Days)}";
            var hours = $"{ts.Hours} hour{Plural(ts.Hours)}";
            var minutes = $"{ts.Minutes} minute{Plural(ts.Minutes)}";
            var seconds = $"{ts.Seconds} second{Plural(ts.Seconds)}";
            var milliSeconds = $"{ts.Milliseconds} millisecond{Plural(ts.Milliseconds)}";

            return $"{days} {hours} {minutes} {seconds} {milliSeconds}";
        }

        /// <summary>
        /// Adds plural "s" if given time value is greater than 1
        /// </summary>
        /// <param name="number">Number to check</param>
        /// <returns></returns>
		private static string Plural(int number)
        {
            return number > 1 ? "s" : string.Empty;
        }

        /// <summary>
        /// Asynchronously waits for a key press from the console and returns the key that was pressed.
        /// </summary>
        public static Task<ConsoleKey> WaitForKeyPressAsync()
        {
			return Task.Run(() => Console.ReadKey(true).Key);
		}
	}
    #endregion
}
