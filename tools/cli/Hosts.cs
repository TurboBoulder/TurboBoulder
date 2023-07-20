using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebTemplateCLI
{
    public class Hosts
    {
        public static bool AddEntry(string ip, string domain)
        {
            string hostsPath = GetHostsPath();
            string newEntry = $"{ip} {domain}";

            if (DomainExists(domain)) return false;

            using (StreamWriter writer = File.AppendText(hostsPath))
            {
                writer.WriteLine(newEntry);
            }

            return true;
        }

        public static bool DomainExists(string domain)
        {
            string hostsPath = GetHostsPath();

            return File.ReadLines(hostsPath)
                .Select(line => line.Split(' ', '\t')) // Split each line into words
                .Any(words => words.Contains(domain)); // Check if any word matches the domain
        }

        private static string GetHostsPath()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    return "/etc/hosts";
                case PlatformID.Win32NT:
                    return @"C:\Windows\System32\drivers\etc\hosts";
                default:
                    throw new NotSupportedException("Unsupported platform");
            }
        }
    }
}
