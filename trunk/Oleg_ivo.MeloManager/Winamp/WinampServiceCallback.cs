using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Oleg_ivo.MeloManager.ServiceReference1;

namespace Oleg_ivo.MeloManager.Winamp
{
    class WinampServiceCallback : IWinampServiceCallback, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WinampServiceCallback()
        {
            CurrentSongSubject = new Subject<string>();
        }

        public Subject<string> CurrentSongSubject { get; private set; }

        public void OnCurrentSongChanged(string filename)
        {
            CurrentSongSubject.OnNext(filename);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (CurrentSongSubject == null) return;
            CurrentSongSubject.Dispose();
            CurrentSongSubject = null;
        }
    }
}
