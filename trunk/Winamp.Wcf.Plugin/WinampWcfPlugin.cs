using System;
using System.Reactive.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Windows;
using Daniel15.Sharpamp;
using NLog;

namespace Winamp.Wcf.Plugin
{
    public class WinampWcfPlugin : GeneralPlugin, IDisposable
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private ServiceHost host;
        private WinampService winampService;
        private IDisposable loadPlaylistObserver;

        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name
        {
            get { return "WCF"; }
        }

        /// <summary>
        /// Called when the plugin should be initialized
        /// </summary>
        public override void Initialize()
        {
            winampService = new WinampService();
            var uri = new Uri("net.tcp://localhost:9000/winamp_wcf");
            host = new ServiceHost(winampService, uri);
            var smb = host.Description.Behaviors.Find<ServiceMetadataBehavior>() ?? new ServiceMetadataBehavior();
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            host.Description.Behaviors.Add(smb);
            host.AddServiceEndpoint("Winamp.Wcf.IWinampService", new NetTcpBinding(), uri);
            host.Open();
            MessageBox.Show("Ready to attach debugger?");
            winampService.CurrentFileChanges = 
                Observable.FromEventPattern<SongChangedEventArgs>(h => Winamp.SongChanged += h, h => Winamp.SongChanged -= h)
                            .Select(e => e.EventArgs.Song.Filename);
            loadPlaylistObserver = 
                winampService.LoadPlaylistSubject.Subscribe(filename => Winamp.LoadPlaylist(filename), 
                                                            exception => log.Error(exception));
        }

        /// <summary>
        /// Quit the plugin
        /// </summary>
        public override void Quit()
        {
            Dispose();
            base.Quit();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            loadPlaylistObserver.Dispose();
            if (host != null)
            {
                if (host.State != CommunicationState.Opened)
                    host.Close(TimeSpan.FromSeconds(10));
            }
        }
    }
}