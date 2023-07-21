using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebTemplateCLI
{
    public class Docker
    {
        public static bool ExecuteCompose(string composeFilePath)
        {
            if (!IsPathRelativeToProgram(composeFilePath))
            {
                Console.WriteLine("Invalid path.");
                //return false;
            }

            try
            {
                string command = "docker"; // GetDockerComposeCommand();
                string currentDirectory = "./docker/";
                string absolutePath = Path.GetFullPath(Path.Combine(currentDirectory, composeFilePath));

                Process process = new Process();
                process.StartInfo.WorkingDirectory = currentDirectory;
                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = "compose up -d";

                process.Start();
                process.WaitForExit();

                Console.WriteLine("Docker Compose execution completed.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during Docker Compose execution: {ex.Message}");
                return false;
            }
        }

        private static bool IsPathRelativeToProgram(string path)
        {
            string fullPath = Path.GetFullPath(path);
            string programDirectory = GetProgramDirectory();

            return fullPath.StartsWith(programDirectory, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetDockerComposeCommand()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "docker compose";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "docker compose";
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported operating system. Only Windows and Linux are supported.");
            }
        }

        private static string GetProgramDirectory()
        {
            string assemblyLocation = Assembly.GetEntryAssembly().Location;
            return Path.GetDirectoryName(assemblyLocation);
        }
    }
}
