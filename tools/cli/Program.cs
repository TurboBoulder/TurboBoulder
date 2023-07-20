// See https://aka.ms/new-console-template for more information
using Spectre.Console;
using System.Net;
using WebTemplateCLI;


await Setup.Run();


//// Get the host name of the current machine
//string hostName = Dns.GetHostName();

//// Get the IP addresses associated with the host name
//IPAddress[] addresses = Dns.GetHostAddresses(hostName);

//foreach (var item in addresses)
//{
//	if (item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
//	{
//		Console.WriteLine(item.ToString()); 
//	}
//}
