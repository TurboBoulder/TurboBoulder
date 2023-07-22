using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace TurboBoulderCLI
{
    /// <summary>
    /// Provides functionality to download and unzip a release from GitHub.
    /// </summary>
    public static class GitHubReleaseDownloader
    {
        private static readonly string settingsFilePath;

        static GitHubReleaseDownloader()
        {
            string executableDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string relativePath = "downloadurl.conf";
            settingsFilePath = Path.Combine(executableDirectory, relativePath);
        }

        /// <summary>
        /// Downloads and unzips the release asset to the specified directory.
        /// </summary>
        /// <param name="unzipPath">The directory where the release asset should be unzipped. Use an empty string to indicate the working folder of the application.</param>
        /// <exception cref="FileNotFoundException">Thrown if the settings file is not found.</exception>
        /// <exception cref="Exception">Thrown if an error occurs during the release download and unzip process.</exception>
        public static async Task DownloadAndUnzipReleaseAsync(string unzipPath)
        {
            try
            {
                if (!File.Exists(settingsFilePath))
                {
                    throw new FileNotFoundException("Settings file not found.", settingsFilePath);
                }

                string downloadUrl;
                try
                {
                    downloadUrl = File.ReadAllText(settingsFilePath).Trim();
                    if (string.IsNullOrEmpty(downloadUrl))
                    {
                        throw new Exception("Settings file is empty.");
                    }
                }
                catch (IOException ex)
                {
                    throw new Exception("An error occurred while reading the settings file.", ex);
                }

                string unzipDirectory = string.IsNullOrEmpty(unzipPath)
                    ? Environment.CurrentDirectory  // Use current working directory if unzipPath is empty
                    : unzipPath;

                using (WebClient client = new WebClient())
                {
                    // Download the release asset
                    string zipFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
                    await client.DownloadFileTaskAsync(downloadUrl, zipFilePath);

                    // Unzip the release asset to the specified path
                    ZipFile.ExtractToDirectory(zipFilePath, unzipDirectory);

                    // Delete the temporary zip file
                    File.Delete(zipFilePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during the release download and unzip process: " + ex.Message);
            }
        }
    }
}
