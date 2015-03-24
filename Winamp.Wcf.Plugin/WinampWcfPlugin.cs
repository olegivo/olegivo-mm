//#define ATTACH_TO_DEBUG_MODE

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using Daniel15.Sharpamp;
using NLog;

namespace Winamp.Wcf.Plugin
{
    public class WinampWcfPlugin : GeneralPlugin, IDisposable
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private ServiceHost host;
        private WinampService winampService;
        private readonly CompositeDisposable disposer = new CompositeDisposable();

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
#if ATTACH_TO_DEBUG_MODE
            System.Windows.MessageBox.Show("Ready to attach debugger?");
#endif
            winampService.CurrentFileChanges = 
                Observable.FromEventPattern<SongChangedEventArgs>(h => Winamp.SongChanged += h, h => Winamp.SongChanged -= h)
                            .Select(e => e.EventArgs.Song.Filename);
            disposer.Add(winampService.LoadPlaylistSubject.Subscribe(filename => Winamp.LoadPlaylist(filename), 
                exception => log.Error("Ошибка при попытке загрузить плейлист", exception)));
            disposer.Add(winampService.ActionSubject.Subscribe(DoAction, 
                exception => log.Error("Ошибка при попытке выполнить действие", exception)));
        }

        private void DoAction(ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.Play:
                    Winamp.Play();
                    break;
                case ActionType.PlayPause:
                    Winamp.PlayPause();
                    break;
                case ActionType.Stop:
                    Winamp.Stop();
                    break;
                case ActionType.PreviousTrack:
                    Winamp.PrevTrack();
                    break;
                case ActionType.NextTrack:
                    Winamp.NextTrack();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("actionType");
            }
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
            disposer.Dispose();
            if (host != null)
            {
                if (host.State != CommunicationState.Opened)
                    host.Close(TimeSpan.FromSeconds(10));
            }
        }
    }
}