// See https://aka.ms/new-console-template for more information

using Renci.SshNet;

var c = new SshClient("172.17.27.71", "pi", "pi");
c.Connect();    
var res = c.RunCommand("/home/pi/RpiTestBlazor").Result;

Console.Write(res);