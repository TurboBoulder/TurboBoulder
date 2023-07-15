using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                }
            }
            else
            {
                Console.WriteLine("Failed to retrieve folder contents. Status code: " + response.StatusCode);
            }
        }

        private static async Task DownloadFile(string url, string savePath)
        {
            HttpResponseMessage response = await _client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                using (Stream fileStream = await response.Content.ReadAsStreamAsync())
                using (FileStream outputStream = new FileStream(savePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(outputStream);
                }
            }
            else
            {
                Console.WriteLine("Failed to download file. Status code: " + response.StatusCode);
            }
        }
    }
}
