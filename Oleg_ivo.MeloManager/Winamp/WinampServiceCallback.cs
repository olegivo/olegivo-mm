using System;
using System.Reactive.Disposables;
using Reactive.Bindings;
using Oleg_ivo.MeloManager.ServiceReference1;

namespace Oleg_ivo.MeloManager.Winamp
{
    class WinampServiceCallback : IWinampServiceCallback, IDisposable
    {
        private readonly CompositeDisposable disposer = new CompositeDisposable();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WinampServiceCallback()
        {
            CurrentSong = new ReactiveProperty<string>();
            disposer.Add(CurrentSong);
        }

        public ReactiveProperty<string> CurrentSong { get; private set; }

        public void OnCurrentSongChanged(string filename)
        {
            CurrentSong.Value = filename;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            disposer.Dispose();
        }
    }
}
