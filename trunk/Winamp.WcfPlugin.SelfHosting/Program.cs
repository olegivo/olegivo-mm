using System;
using Winamp.Wcf.Plugin;

namespace Winamp.WcfPlugin.SelfHosting
{
    class Program
    {
        static void Main(string[] args)
        {
            var winampPlugin = new WinampWcfPlugin();
            winampPlugin.Initialize();
            Console.WriteLine("Plugin started. press any key to stop it");
            Console.ReadLine();
            winampPlugin.Quit();
            //var uri = new Uri("net.tcp://localhost:9000/winamp_wcf");
            ////using (var host = new ServiceHost(typeof(WinampService), uri))
            //using (var host = new ServiceHost(new WinampService(), uri))
            //{
            //    //var smb = host.Description.Behaviors.Find<ServiceMetadataBehavior>() ?? new ServiceMetadataBehavior();
            //    //smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            //    //host.Description.Behaviors.Add(smb);
            //    host.AddServiceEndpoint("Winamp.Wcf.IWinampService", new NetTcpBinding(), uri);
            //    //host.
            //    host.Open();

            //    Console.WriteLine("Service started. press any key to stop it");
            //    Console.ReadLine();

            //    host.Close();
            //}
        }
    }
}
//InstanceContextMode = InstanceContextMode.Single