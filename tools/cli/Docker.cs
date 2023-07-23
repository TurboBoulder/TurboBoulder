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

                // Create and start a process for "docker compose pull"
                Process pullProcess = new Process();
                pullProcess.StartInfo.WorkingDirectory = currentDirectory;
                pullProcess.StartInfo.FileName = command;
                pullProcess.StartInfo.Arguments = "compose pull";

                pullProcess.Start();
                pullProcess.WaitForExit();

                // Create and start a process for "docker compose up -d"
                Process upProcess = new Process();
                upProcess.StartInfo.WorkingDirectory = currentDirectory;
                upProcess.StartInfo.FileName = command;
                upProcess.StartInfo.Arguments = "compose up -d";

                upProcess.Start();
                upProcess.WaitForExit();

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
