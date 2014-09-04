using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Codeplex.Reactive;
using NLog;
using Oleg_ivo.MeloManager.ServiceReference1;

namespace Oleg_ivo.MeloManager.Winamp
{
    public class WinampControl: IDisposable
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly CompositeDisposable disposer = new CompositeDisposable();
        private WinampServiceClient client;
        private IDisposable currentSongChangedSubscription;

        public void LaunchBind()
        {
            var endpointAddress = new EndpointAddress("net.tcp://localhost:9000/winamp_wcf");
            var netTcpBinding = new NetTcpBinding {OpenTimeout = TimeSpan.FromSeconds(1)};
            var winampServiceCallback = new WinampServiceCallback();
            CurrentSongSubject = winampServiceCallback.CurrentSongSubject;
            IsConnected =
                Observable.Interval(TimeSpan.FromSeconds(1))
                    .Select(l => !(client != null && client.State == CommunicationState.Opened))
                    .DistinctUntilChanged()
                    .ToReactiveProperty();

            var observableClient = IsConnected.Select(isConnected =>
            {
                if (!isConnected)
                {
                    client = Connect(winampServiceCallback, netTcpBinding, endpointAddress);
                    DisposeSubscription();
                }
                return client;
            }).ToReactiveProperty();
        }

        private WinampServiceClient Connect(WinampServiceCallback winampServiceCallback, Binding binding, EndpointAddress endpointAddress)
        {
            WinampServiceClient winampServiceClient = null;
            try
            {
                //var discoveryClient = new DiscoveryClient(new DiscoveryEndpoint(netTcpBinding, endpointAddress));
                //var findResponse = discoveryClient.Resolve(new ResolveCriteria(){Duration = TimeSpan.FromMilliseconds(500)});
                winampServiceClient = new WinampServiceClient(new InstanceContext(winampServiceCallback), binding, endpointAddress);
                try
                {
                    winampServiceClient.Open();
                    winampServiceClient.Ping();
                    log.Debug("Winamp на связи");
                    currentSongChangedSubscription =
                        winampServiceCallback.CurrentSongSubject.Subscribe(OnCurrentSongChanged);
                }
                catch (TimeoutException)
                {
                    winampServiceClient = null;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return winampServiceClient;
        }

        public ReactiveProperty<bool> IsConnected { get; private set; }

        public Subject<string> CurrentSongSubject { get; private set; }

        private void DisposeSubscription()
        {
            if (client == null && currentSongChangedSubscription != null)
            {
                currentSongChangedSubscription.Dispose();
                currentSongChangedSubscription = null;
            }
        }

        private void OnCurrentSongChanged(string filename)
        {
            log.Debug("Now playing: {0}", filename);
        }

        public void LoadPlaylist(string playlistFilename)
        {
            client.LoadPlaylist(playlistFilename);
        }

        public Task PreviousTrack()
        {
            return client.PreviousTrackAsync();
        }

        public Task NextTrack()
        {
            return client.NextTrackAsync();
        }

        public Task Play()
        {
            return client.PlayAsync();
        }

        public Task PlayPause()
        {
            return client.PlayPauseAsync();
        }
        public Task Stop()
        {
            return client.StopAsync();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            disposer.Dispose();
            if (client != null)
            {
                client.Close();
                client = null;
            }
            DisposeSubscription();
        }
    }
}
