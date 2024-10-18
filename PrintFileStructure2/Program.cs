using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class Program
{
    // A list of ignored files or directories
    static List<string> ignoredFiles = new List<string>();

    static void Main(string[] args)
    {
        // Display the start page
        DisplayStartPage();

        string currentPath = @"C:\";  // Default starting path (can be changed)
        Console.WriteLine("Start browsing your file system. Type a drive letter or directory name.");

        while (true)
        {
            // Show current directory path
            Console.WriteLine($"\nCurrent Directory: {currentPath}");

            Console.Write($"{currentPath}> ");
            string input = ReadInputWithTabCompletion(ref currentPath);

            if (string.IsNullOrWhiteSpace(input))
                continue; // Do nothing if input is blank or contains only spaces

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            // Handle 'ls' command to show only the current directory contents
            if (input.Equals("ls", StringComparison.OrdinalIgnoreCase))
            {
                ListCurrentDirectoryContents(currentPath);
                continue;
            }

            // Handle 'print ignore' command to list ignored files
            if (input.Equals("print ignore", StringComparison.OrdinalIgnoreCase))
            {
                PrintIgnoredFiles();
                continue;
            }

            // Handle 'clear ignore' command to clear the ignore list
            if (input.Equals("clear ignore", StringComparison.OrdinalIgnoreCase))
            {
                ignoredFiles.Clear();  // Clear the ignore list
                Console.WriteLine("Ignore list cleared.");
                continue;
            }

            // Handle '..' command to go up one directory
            if (input.Equals("..", StringComparison.OrdinalIgnoreCase))
            {
                currentPath = Directory.GetParent(currentPath)?.FullName ?? currentPath;
                continue;
            }

            // Handle 'ignore <filenames>' command (supporting multiple files)
            if (input.StartsWith("ignore ", StringComparison.OrdinalIgnoreCase))
            {
                var filesToIgnore = input.Substring(7).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                IgnoreFiles(filesToIgnore, currentPath);
                continue;
            }

            // Handle 'unignore <filename>' command
            if (input.StartsWith("unignore ", StringComparison.OrdinalIgnoreCase))
            {
                string fileToUnignore = input.Substring(9).Trim();
                if (ignoredFiles.Contains(fileToUnignore))
                {
                    ignoredFiles.Remove(fileToUnignore);
                    Console.WriteLine($"Unignored file: {fileToUnignore}");
                }
                continue;
            }

            // Handle 'print' command to clear console and recursively list files
            if (input.Equals("print", StringComparison.OrdinalIgnoreCase))
            {
                Console.Clear();  // Clear the console
                PrintDirectoryContents(currentPath);
                continue;
            }

            // If it's a drive letter, change to that drive
            if (input.Length == 2 && char.IsLetter(input[0]) && input.EndsWith(":"))
            {
                string potentialDrive = input.ToUpper() + "\\";
                if (Directory.Exists(potentialDrive))
                {
                    currentPath = potentialDrive;
                }
                else
                {
                    Console.WriteLine("Invalid drive.");
                }
                continue;
            }
        }
    }

    static void DisplayStartPage()
    {
        // Updated welcome page design
        Console.Clear();
        Console.WriteLine("---------------------------------------------------------");
        Console.WriteLine("Welcome to the File System Explorer!");
        Console.WriteLine("Here are the available commands:");
        Console.WriteLine();
        Console.WriteLine("  ls             - List the files and folders in the current directory.");
        Console.WriteLine("  print          - Show the entire directory structure recursively.");
        Console.WriteLine("  ignore <file(s)>  - Ignore specified files or folders.");
        Console.WriteLine("  unignore <file>- Remove the specified file or folder from the ignore list.");
        Console.WriteLine("  print ignore   - Show the currently ignored files.");
        Console.WriteLine("  clear ignore   - Clear the ignore list.");
        Console.WriteLine("  ..             - Go up one directory.");
        Console.WriteLine("  exit           - Exit the program.");
        Console.WriteLine();
        Console.WriteLine("---------------------------------------------------------");
        Console.WriteLine("Enjoy exploring your file system! Type a command to begin.");
    }

    // Prints only the current directory contents without recursion
    static void ListCurrentDirectoryContents(string path)
    {
        try
        {
            var entries = Directory.GetFileSystemEntries(path);
            if (entries.Length == 0)
            {
                Console.WriteLine("(Empty Directory)");
            }
            else
            {
                Console.WriteLine("Directory Contents (excluding ignored files):");
                foreach (var entry in entries)
                {
                    string fileName = Path.GetFileName(entry);
                    if (!ignoredFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                    {
                        Console.WriteLine(fileName);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing contents: {ex.Message}");
        }
    }

    // Prints the directory contents recursively (for 'print' command) and ensures no duplicates
    static void PrintDirectoryContents(string path, string indent = "")
    {
        HashSet<string> printedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var entries = Directory.GetFileSystemEntries(path);
            if (entries.Length == 0)
            {
                Console.WriteLine($"{indent}(Empty Directory)");
            }
            else
            {
                foreach (var entry in entries)
                {
                    string fileName = Path.GetFileName(entry);
                    if (!ignoredFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase) && !printedFiles.Contains(fileName))
                    {
                        if (Directory.Exists(entry))
                        {
                            // Print the directory and then recursively print its contents
                            Console.WriteLine($"{indent}{fileName}");
                            printedFiles.Add(fileName); // Mark the folder as printed
                            PrintDirectoryContents(entry, indent + "       - ");
                        }
                        else
                        {
                            // Print the file
                            Console.WriteLine($"{indent}{fileName}");
                            printedFiles.Add(fileName); // Mark the file as printed
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing contents: {ex.Message}");
        }
    }

    static void IgnoreFiles(string[] filesToIgnore, string currentPath)
    {
        var entries = Directory.GetFileSystemEntries(currentPath);
        var entrySet = new HashSet<string>(entries.Select(Path.GetFileName), StringComparer.OrdinalIgnoreCase);

        foreach (var file in filesToIgnore)
        {
            if (entrySet.Contains(file, StringComparer.OrdinalIgnoreCase))
            {
                // Handle case-sensitivity if both versions exist (e.g., admin.txt and Admin.txt)
                var exactMatches = entries.Where(e => string.Equals(Path.GetFileName(e), file, StringComparison.Ordinal));
                if (exactMatches.Count() > 1 || !ignoredFiles.Contains(file, StringComparer.OrdinalIgnoreCase))
                {
                    ignoredFiles.Add(file);
                    Console.WriteLine($"Ignoring file: {file}");
                }
            }
            else
            {
                Console.WriteLine($"File '{file}' does not exist in the current directory.");
            }
        }
    }

    static void PrintIgnoredFiles()
    {
        if (ignoredFiles.Count == 0)
        {
            Console.WriteLine("No files are currently ignored.");
        }
        else
        {
            Console.WriteLine("Ignored files:");
            foreach (var file in ignoredFiles)
            {
                Console.WriteLine(file);
            }
        }
    }

    static string ReadInputWithTabCompletion(ref string currentPath)
    {
        string input = "";
        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            // If the Tab key is pressed, attempt auto-completion
            if (key.Key == ConsoleKey.Tab)
            {
                if (string.IsNullOrWhiteSpace(input) || input.EndsWith("\\"))
                {
                    // Do nothing if the input is empty or ends with a backslash
                    continue;
                }

                string[] directories = Directory.GetDirectories(currentPath, input + "*");

                if (directories.Length == 1)
                {
                    string completion = Path.GetFileName(directories.First());
                    Console.Write(new string('\b', input.Length)); // Clear current input
                    Console.Write(completion + "\\"); // Append a backslash for continued typing
                    input = ""; // Clear input to allow further navigation
                    currentPath = Path.Combine(currentPath, completion); // Update current path to the completed directory
                }
                else if (directories.Length > 1)
                {
                    Console.WriteLine("\nMultiple matches found:");
                    foreach (var dir in directories)
                    {
                        Console.WriteLine(Path.GetFileName(dir));
                    }
                    Console.Write($"\n{currentPath}> {input}"); // Reprint current input to allow continuation
                }
                else
                {
                    Console.WriteLine("\nNo matches found.");
                                       Console.Write($"{currentPath}> {input}"); // Reprint current input
                }
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                if (input.EndsWith("\\"))
                {
                    // Automatically list the directory contents if Enter is pressed after a backslash
                    ListCurrentDirectoryContents(currentPath);
                }
                return input;
            }
            else if (key.Key == ConsoleKey.Backspace && input.Length > 0)
            {
                input = input[0..^1]; // Remove the last character
                Console.Write("\b \b"); // Remove from console display
            }
            else if (key.Key == ConsoleKey.Spacebar)
            {
                Console.Write(' ');
                input += ' ';
            }
            else if (key.KeyChar == '.')
            {
                Console.Write(key.KeyChar);
                input += key.KeyChar;
            }
            else if (key.KeyChar == ':')
            {
                Console.Write(key.KeyChar);
                input += key.KeyChar; // Allows typing the colon character to switch drives (e.g., D:)
            }
            else if (char.IsLetterOrDigit(key.KeyChar) || key.KeyChar == '\\')
            {
                Console.Write(key.KeyChar);
                input += key.KeyChar;
            }
        }
    }
}

