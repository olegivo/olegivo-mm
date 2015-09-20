using System;
using System.Windows.Input;
using Reactive.Bindings;
using NLog;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.WPF.Extensions;
using Oleg_ivo.Base.WPF.ViewModels;

namespace Oleg_ivo.MeloManager.Winamp.Tracking
{
    public class TrackingViewModel : ViewModelBase
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private string filename;

        /// <summary>
        /// Initializes a new instance of the ViewModelBase class.
        /// </summary>
        public TrackingViewModel(WinampControl winampControl)
        {
            Enforce.ArgumentNotNull(winampControl, "winampControl");
            Disposer.Add(winampControl.CurrentSong.Subscribe(OnCurrentSongChanged));
            InitCommands(winampControl);
        }

        private void InitCommands(WinampControl winampControl)
        {
            CommandPreviousTrack = new ReactiveCommand(winampControl.IsConnected).AddHandler(() => winampControl.PreviousTrack());
            CommandNextTrack = new ReactiveCommand(winampControl.IsConnected).AddHandler(() => winampControl.NextTrack());
            CommandPlay = new ReactiveCommand(winampControl.IsConnected).AddHandler(() => winampControl.Play());
            CommandPlayPause = new ReactiveCommand(winampControl.IsConnected).AddHandler(() => winampControl.PlayPause());
            CommandStop = new ReactiveCommand(winampControl.IsConnected).AddHandler(() => winampControl.Stop());
        }

        private void OnCurrentSongChanged(string filename)
        {
            log.Debug("Now playing: {0}", filename);
            Filename = filename;
        }

        public string Filename
        {
            get { return filename; }
            set
            {
                if(filename == value) return;
                filename = value;
                RaisePropertyChanged("Filename");
            }
        }

        public ICommand CommandPreviousTrack { get; private set; }

        public ICommand CommandPlay { get; private set; }

        public ICommand CommandPlayPause { get; private set; }

        public ICommand CommandStop { get; private set; }

        public ICommand CommandNextTrack { get; private set; }
    }
}
