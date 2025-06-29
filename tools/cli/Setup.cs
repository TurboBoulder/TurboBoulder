﻿using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TurboBoulderCLI;

namespace WebTemplateCLI
{
    public static class Setup
    {
        public async static Task<bool> Run()
        {
            string baseurl;
            string projectName;
            string hostIP;
            string databaseUsername;
            bool allowExternalAccessToWebManagementInterface = false;
            string projectNiceName;

            projectName = InputProjectName();
            projectNiceName = projectName.Replace(" ", "");

            try
            {
                await DownloadProjectFiles();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("Installation cancelled. " + ex.Message);
                return false;
            }

            hostIP = InputHostIP();

            baseurl = InputBaseURL();

            SetHosts(baseurl, projectName, hostIP);

            databaseUsername = InputDBUsername(baseurl);

            string randomPassword = CreatePassword();

            EnvironmentSetup(baseurl, projectName.Replace(" ", ""), randomPassword);

            AnsiConsole.MarkupLine("Docker actions...");
            if (!Docker.ExecuteCompose("docker-compose.yaml")) return false;

            AnsiConsole.MarkupLine("Setting up Web Management Interface...");
            // set up web management interface      
            AnsiConsole.MarkupLine("Access Web CLI: https://" + baseurl + ":7842/webcli");

            return true;
        }

        private static async Task DownloadProjectFiles()
        {
            AnsiConsole.MarkupLine("Downloading project files...");

            List<string> foldersToCheck = new List<string>
            {
                "api",
                "frontend",
                "shared"
            };

            List<string> existingFolders = foldersToCheck.Where(Directory.Exists).ToList();

            if (existingFolders.Count > 0)
            {
                AnsiConsole.MarkupLine("The following folders already exist:");
                foreach (string folder in existingFolders)
                {
                    AnsiConsole.MarkupLine($"[red]{folder}[/]");
                }


                bool continueDownload = AnsiConsole.Confirm("Continuing will overwrite existing files in these folders. Do you want to continue?", false);

                if (!continueDownload)
                {
                    AnsiConsole.MarkupLine("Download canceled by user.");
                    return;
                }
            }

            await GitHubReleaseDownloader.DownloadAndUnzipReleaseAsync("");
        }


        private static string CreatePassword()
        {
            string randomPassword = GenerateRandomPassword(16);
            AnsiConsole.MarkupLine("The database password is [bold yellow]" + randomPassword + "[/]. Keep it in a safe place!");
            return randomPassword;
        }

        private static string InputDBUsername(string baseurl)
        {
            string databaseUsername = SanitizeUsername(AnsiConsole.Ask<string>("Database username: ", "dbuser"));
            while (databaseUsername == string.Empty)
            {
                AnsiConsole.MarkupLine("The username can only consist of [bold yellow]letters[/], [bold yellow]numbers[/], or [bold yellow]underscores[/].");
                databaseUsername = SanitizeUsername(AnsiConsole.Ask<string>("Database username: ", "dbuser"));
            }

            return databaseUsername;
        }

        private static void SetHosts(string baseurl, string projectName, string hostIP)
        {
            if (AnsiConsole.Confirm("Add development hosts to your hosts file? ", true))
            {
                AddHosts(baseurl, projectName.Replace(" ", ""), hostIP);
            }
        }

        private static string InputBaseURL()
        {
            string baseurl = SanitizeDomainName(AnsiConsole.Ask<string>("Base [bold]URL[/]?", "turboboulder.internal"));
            while (baseurl == string.Empty)
            {
                AnsiConsole.MarkupLine("Please enter a valid domain");
                baseurl = SanitizeDomainName(AnsiConsole.Ask<string>("Base [bold]URL[/]?", "turboboulder.internal"));
            }

            return baseurl;
        }

        private static string InputHostIP()
        {
            string hostIP = SanitizeIPv4Address(AnsiConsole.Ask<string>("Host [bold]IP[/]: ", "127.0.0.1"));
            while (hostIP == string.Empty)
            {
                AnsiConsole.MarkupLine("Please enter a valid IP address");
                hostIP = SanitizeIPv4Address(AnsiConsole.Ask<string>("Host [bold]IP[/]: ", "127.0.0.1"));
            }

            return hostIP;
        }

        private static string InputProjectName()
        {
            string projectName = SanitizeProjectNameString(AnsiConsole.Ask<string>("Project [bold]name[/]: ", "My Project"));
            while (projectName == string.Empty)
            {
                AnsiConsole.MarkupLine("Project name may only contain letters, numbers, spaces, and underscores");
                projectName = SanitizeProjectNameString(AnsiConsole.Ask<string>("Project [bold]name[/]: ", "My Project"));
            }

            return projectName;
        }

        private static void EnvironmentSetup(string baseurl, string projectName, string randomPassword)
        {
            AnsiConsole.MarkupLine("Certificates setup");
            // ask for
            // country
            // location
            // state
            // organization


            // create env files for docker images
            EnvironmentSetupCA(baseurl, projectName);

            EnvironmentSetupSqlServer(randomPassword);

            //string envFilePath = "./webserver/dev.env";
            //Dictionary<string, string> envVariables = new Dictionary<string, string>
            //{
            //    { "MSSQL_SA_PASSWORD", "dfgolih" },
            //};

            //CreateEnvFile(envFilePath, envVariables);
        }

        private static void EnvironmentSetupSqlServer(string randomPassword)
        {
            string envFilePath = "./docker/sqlserver/dev.env";
            Dictionary<string, string> envVariables = new Dictionary<string, string>
        {
            { "MSSQL_SA_PASSWORD", randomPassword },
        };

            CreateEnvFile(envFilePath, envVariables);
        }

        private static void EnvironmentSetupCA(string baseurl, string projectName)
        {
            string envFilePath = "./docker/ca/dev.env";
            Dictionary<string, string> envVariables = new Dictionary<string, string>
            {
                { "COUNTRY", "SE" },
                { "STATEPROVINCE", "Denial" },
                { "LOCATION", "Stockholm" },
                { "ORGANISATION", "Organisation Name Here" },
                { "COMMONNAME", projectName.ToLower() + "." + baseurl }
            };

            CreateEnvFile(envFilePath, envVariables);
        }

        private static void AddHosts(string baseurl, string projectName, string hostIP)
        {
            if (!Hosts.AddEntry(hostIP, projectName.ToLower() + "." + baseurl)) // CA
                AnsiConsole.MarkupLine(projectName.ToLower() + "." + baseurl + " already present in hosts");

            if (!Hosts.AddEntry(hostIP, "webserver." + projectName.ToLower() + "." + baseurl)) // API
                AnsiConsole.MarkupLine("webserver." + projectName.ToLower() + "." + baseurl + " already present in hosts");

            if (!Hosts.AddEntry(hostIP, "sqlserver." + projectName.ToLower() + "." + baseurl)) // Database
                AnsiConsole.MarkupLine("sqlserver." + projectName.ToLower() + "." + baseurl + " already present in hosts");

            if (!Hosts.AddEntry(hostIP, "client." + projectName.ToLower() + "." + baseurl)) // Frontend client
                AnsiConsole.MarkupLine("client." + projectName.ToLower() + "." + baseurl + " already present in hosts");
        }

        private static string RunPowerShellScript(string scriptPath)
        {
            // Create a new process to run PowerShell
            Process process = new Process();

            // Set the filename of the PowerShell executable
            process.StartInfo.FileName = "powershell.exe";

            // Specify the script to be executed
            process.StartInfo.Arguments = $"-ExecutionPolicy Bypass -File {scriptPath}";

            // Set the output encoding
            process.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;

            // Redirect the standard output of the process
            process.StartInfo.RedirectStandardOutput = true;

            // Enable the process to raise events
            process.StartInfo.UseShellExecute = false;

            // Start the process
            process.Start();

            // Read the standard output of the process
            string output = process.StandardOutput.ReadToEnd();

            // Wait for the process to exit
            process.WaitForExit();

            return output;
        }

        private static string SanitizeIPv4Address(string input)
        {
            if (IPAddress.TryParse(input, out IPAddress ipAddress) && ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ipAddress.ToString();
            }

            return string.Empty;
        }

        private static string SanitizeDomainName(string input)
        {
            const int MaxLength = 255; // Maximum length for a domain name

            // Trim leading/trailing whitespace and limit the length
            input = input.Trim().Substring(0, Math.Min(input.Length, MaxLength));

            // Validate domain name format using regex pattern
            const string DomainNamePattern = @"^(?:(?:[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9]\.)+[a-zA-Z]{2,})$";
            if (Regex.IsMatch(input, DomainNamePattern))
            {
                return input;
            }

            return string.Empty;
        }

        private static string SanitizeProjectNameString(string input)
        {
            // Trim leading/trailing whitespace
            input = input.Trim();

            // Replace consecutive spaces with a single space
            input = Regex.Replace(input, @"\s+", " ");

            // Validate the string using regex pattern
            const string Pattern = @"^(?![\s_])[a-zA-Z0-9_ ]*(?<![\s_])$";
            if (Regex.IsMatch(input, Pattern))
            {
                return input;
            }

            return string.Empty;
        }

        private static string SanitizeUsername(string input)
        {
            const string Pattern = @"^[a-zA-Z0-9_]+$";

            // If the input matches the pattern, it is valid. Otherwise return an empty string.
            return Regex.IsMatch(input, Pattern) ? input : string.Empty;
        }

        private static string GenerateRandomPassword(int length)
        {
            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,-_";

            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[length];
                rngCryptoServiceProvider.GetBytes(randomBytes);

                var passwordChars = randomBytes.Select(b => allowedChars[b % allowedChars.Length]);
                return new string(passwordChars.ToArray());
            }
        }

        private static void CreateEnvFile(string filePath, Dictionary<string, string> envVariables)
        {
            try
            {
                // TODO: sanitize filepath
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var variable in envVariables)
                    {
                        writer.WriteLine($"{variable.Key}={variable.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to write " + filePath + ": " + ex.ToString());
                throw;
            }

            Console.WriteLine("Environment file created.");
        }
    }
}
