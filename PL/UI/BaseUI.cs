namespace PL.UI
{
    /// <summary>
    /// Base class for all UI handlers
    /// </summary>
    public abstract class BaseUI
    {
        protected readonly IServiceProvider _serviceProvider;

        public BaseUI(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets a controller from the DI container
        /// </summary>
        protected T GetController<T>() where T : class
        {
            return _serviceProvider.GetService(typeof(T)) as T
                ?? throw new InvalidOperationException($"Controller {typeof(T).Name} not found in the container.");
        }

        /// <summary>
        /// Displays a header for the menu
        /// </summary>
        protected void DisplayHeader(string title)
        {
            Console.Clear();
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"{title}");
            Console.WriteLine(new string('=', 50));
        }

        /// <summary>
        /// Waits for user to press a key before continuing
        /// </summary>
        protected void PressAnyKeyToContinue()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Displays an error message
        /// </summary>
        protected void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Displays a success message
        /// </summary>
        protected void DisplaySuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Success: {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Gets an integer input from the user
        /// </summary>
        protected int GetIntInput(string prompt, int? minValue = null, int? maxValue = null)
        {
            int value;
            bool isValid;

            do
            {
                Console.Write(prompt);
                isValid = int.TryParse(Console.ReadLine(), out value);

                if (!isValid)
                {
                    DisplayError("Please enter a valid number.");
                    continue;
                }

                if (minValue.HasValue && value < minValue.Value)
                {
                    DisplayError($"Value must be at least {minValue.Value}.");
                    isValid = false;
                }

                if (maxValue.HasValue && value > maxValue.Value)
                {
                    DisplayError($"Value must be at most {maxValue.Value}.");
                    isValid = false;
                }

            } while (!isValid);

            return value;
        }

        /// <summary>
        /// Gets a decimal input from the user
        /// </summary>
        protected decimal GetDecimalInput(string prompt, decimal? minValue = null, decimal? maxValue = null)
        {
            decimal value;
            bool isValid;

            do
            {
                Console.Write(prompt);
                isValid = decimal.TryParse(Console.ReadLine(), out value);

                if (!isValid)
                {
                    DisplayError("Please enter a valid decimal number.");
                    continue;
                }

                if (minValue.HasValue && value < minValue.Value)
                {
                    DisplayError($"Value must be at least {minValue.Value}.");
                    isValid = false;
                }

                if (maxValue.HasValue && value > maxValue.Value)
                {
                    DisplayError($"Value must be at most {maxValue.Value}.");
                    isValid = false;
                }

            } while (!isValid);

            return value;
        }

        /// <summary>
        /// Gets a string input from the user
        /// </summary>
        protected string GetStringInput(string prompt, bool allowEmpty = false)
        {
            string value;
            bool isValid;

            do
            {
                Console.Write(prompt);
                value = Console.ReadLine() ?? string.Empty;
                isValid = allowEmpty || !string.IsNullOrWhiteSpace(value);

                if (!isValid)
                {
                    DisplayError("Input cannot be empty.");
                }

            } while (!isValid);

            return value;
        }

        /// <summary>
        /// Gets confirmation from the user (Y/N)
        /// </summary>
        protected bool GetConfirmation(string prompt)
        {
            Console.Write($"{prompt} (Y/N): ");
            var key = Console.ReadKey().Key;
            Console.WriteLine();
            return key == ConsoleKey.Y;
        }

        /// <summary>
        /// The main menu to be implemented by subclasses
        /// </summary>
        public abstract void ShowMenu();
    }
}