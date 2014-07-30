using System;
using System.Reactive.Linq;
using System.ServiceModel;
using NLog;
using Oleg_ivo.MeloManager.ServiceReference1;
using WinampProxyNS;

namespace Oleg_ivo.MeloManager.Winamp
{
    public class WinampControl: IDisposable
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private WinampProxy wp;
        //private WinampPlugin winamp;
        private IDisposable disposable;
        private WinampServiceClient client;

        public void LaunchBind()
        {
            disposable = Observable.Interval(TimeSpan.FromSeconds(1))
                .Where(l => client==null || client.State != CommunicationState.Opened)
                .Subscribe(l =>
                {
                    try
                    {
                        var winampServiceCallback = new WinampServiceCallback();
                        var callbackInstance = new InstanceContext(winampServiceCallback);
                        client = new WinampServiceClient(callbackInstance, new NetTcpBinding { OpenTimeout = TimeSpan.FromSeconds(1) },
                            new EndpointAddress("net.tcp://localhost:9000/winamp_wcf"));
                        client.Open();
                        client.Ping();
                        log.Debug("Winamp на связи");
                        winampServiceCallback.CurrentSongSubject.Subscribe(filename => log.Debug("Now playing: {0}", filename));
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                });
            //SetupWinampProxy();
        }

        public void LoadPlaylist(string playlistFilename)
        {
            client.LoadPlaylist(playlistFilename);
        }
/*
        private void SetupWinampProxy()
        {
            wp = new WinampProxy();
            wp.SetPollingFrequency(1000); //TODO: 2 Rx
            wp.eventWinampInstanceCreated += wp_WinampInstanceCreated;
            wp.eventWinampInstanceClosed += wp_WinampInstanceClosed;
            wp.eventPlayStarted += wp_PlayStarted;
            wp.eventPlayStopped += wp_PlayStopped;
            wp.eventTrackChanged += wp_TrackChanged;
            wp.eventEnqueuePrompt += wp_EnqueuePrompt;
        }

        private void wp_WinampInstanceCreated()
        {
            log.Debug("Winamp instance created");
        }

        private void wp_WinampInstanceClosed()
        {
            log.Debug("Winamp instance closed");
        }

        private void wp_PlayStarted()
        {
            log.Debug("Winamp is now playing");
        }

        private void wp_PlayStopped()
        {
            log.Debug("Winamp has stopped playing");
        }

        private void wp_TrackChanged()
        {
            log.Debug("Now playing track {0}: {1}", (wp.iPlaylistTrackNumber + 1), wp.GetTrackFilename());
        }

        private void wp_EnqueuePrompt()
        {
            log.Debug("Enqueue prompt: Queue has {0} milliseconds left", wp.iEnqueuePromptMilliseconds);
        }
*/

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (disposable == null) return;
            disposable.Dispose();
            disposable = null;
            if (client != null)
            {
                client.Close();
                client = null;
            }
        }
    }
}
