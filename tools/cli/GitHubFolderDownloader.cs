using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WebTemplateCLI
{
    public static class GitHubFolderDownloader
    {
        private static readonly HttpClient _client = new HttpClient();

        public static async Task DownloadFolderFromBranch(string branch, string folderPath)
        {
            string owner = "TurboBoulder"; // Replace with your GitHub username or organization name
            string repo = "TurboBoulder"; // Replace with the repository name

            string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/contents/{folderPath}?ref={branch}";

            _client.DefaultRequestHeaders.Add("User-Agent", "TurboBoulderCLI");

            HttpResponseMessage response = await _client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize the response content
                var folderContents = Newtonsoft.Json.JsonConvert.DeserializeObject<GitHubFolderContent[]>(content);

                // Download the files in the folder
                foreach (var item in folderContents)
                {
                    if (item.type == "file")
                    {
                        await DownloadFile(item.download_url, item.path);
                        Console.WriteLine($"Downloaded file: {item.path}");
                    }
                    else if (item.type == "dir") // If the item is a directory
                    {
                        // Create the subfolder locally
                        Directory.CreateDirectory(item.path);

                        // Recursively download the contents of the subfolder
                        await DownloadFolderFromBranch(branch, Path.Combine(folderPath, item.name));
                        Console.WriteLine($"Downloaded folder: {item.path}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Failed to retrieve folder contents. Status code: " + response.StatusCode);
            }
        }

        public static async Task DownloadFileFromBranch(string branch, string filePath)
        {
            string owner = "TurboBoulder"; // Replace with your GitHub username or organization name
            string repo = "TurboBoulder"; // Replace with the repository name

            string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/contents/{filePath}?ref={branch}";

            _client.DefaultRequestHeaders.Add("User-Agent", "TurboBoulderCLI");

            HttpResponseMessage response = await _client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize the response content
                var fileContent = Newtonsoft.Json.JsonConvert.DeserializeObject<GitHubFolderContent>(content);

                if (fileContent.type == "file")
                {
                    await DownloadFile(fileContent.download_url, fileContent.path);
                    Console.WriteLine($"Downloaded file: {fileContent.path}");
                }
            }
            else
            {
                Console.WriteLine("Failed to retrieve file contents. Status code: " + response.StatusCode);
            }
        }

        private static async Task DownloadFile(string url, string savePath)
        {
            HttpResponseMessage response = await _client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using (Stream fileStream = await response.Content.ReadAsStreamAsync())
                    using (FileStream outputStream = new FileStream(savePath, FileMode.Create))
                    {
                        await fileStream.CopyToAsync(outputStream);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    throw;
                }
            }
            else
            {
                Console.WriteLine("Failed to download file. Status code: " + response.StatusCode);
            }
        }
    }
}
