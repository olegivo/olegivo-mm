using System;
using System.IO;
using Autofac;
using NLog;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.MeloManager.Extensions;
using Oleg_ivo.MeloManager.PlaylistFileAdapters;
using Oleg_ivo.MeloManager.Prism;

namespace Oleg_ivo.MeloManager.Winamp
{
    public class WinampFilesMonitor//TODO: взять некоторые вещи из монитора папок?
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly MeloManagerOptions options;
        private readonly IComponentContext context;

        public void MonitorFilesChanges()
        {
            adapter = context.ResolveUnregistered<WinampM3UPlaylistFileAdapter>();
            var playlistsPath = options.PlaylistsPath;
            var fileWatcherPlaylistsContainer = new FileSystemWatcher(playlistsPath, "playlists.xml")
            {
                EnableRaisingEvents = true,
                //NotifyFilter = NotifyFilters.LastWrite
            };
            var fileWatcherPlaylists = new FileSystemWatcher(playlistsPath, "*.m3u8")
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Attributes | NotifyFilters.DirectoryName | NotifyFilters.Security
            };

            /*
             * Последовательность событий файлов Winamp:
             * При добавлении нового плейлиста: добавление файла плейлиста, изменение playlists.xml
             * При изменении плейлиста: изменение файла плейлиста, без изменения playlists.xml
             * При удалении плейлиста: удаление файла плейлиста, после закрытия Winamp происходит изменение playlists.xml (также можно отследить после того, как произошли любые другие изменения, затрагивающие playlists.xml)
             * Вывод - основной мониторинг - это мониторинг файлов плейлистов, при удалении плейлиста можно не мониторить playlists.xml
            */
            var playlistsContainerThrottleTime = TimeSpan.FromSeconds(0.5);
            var playlistFileThrottleTime = TimeSpan.FromSeconds(1);
            ObservablePlaylistsContainer = fileWatcherPlaylistsContainer.ToObservable(WatcherChangeTypes.Changed, playlistsContainerThrottleTime);
            ObservablePlaylistAdd = fileWatcherPlaylists.ToObservable(WatcherChangeTypes.Created, playlistFileThrottleTime)/*.Select(filename => )*/;//TODO: для того, чтобы взять актуальное значение название плейлиста из словаря, нужно, чтобы сначала сработал ObservablePlaylistsContainer, нужно его ждать
            ObservablePlaylistChange = fileWatcherPlaylists.ToObservable(WatcherChangeTypes.Changed, playlistFileThrottleTime);
            ObservablePlaylistDelete = fileWatcherPlaylists.ToObservable(WatcherChangeTypes.Deleted, playlistFileThrottleTime);

            playlistsContainerChanges = ObservablePlaylistsContainer.Subscribe(OnPlaylistXmlChanged);
            playlistAdding = ObservablePlaylistAdd.Subscribe(OnAdded);
            playlistChanging = ObservablePlaylistChange.Subscribe(OnChanged);
            playlistDeleting = ObservablePlaylistDelete.Subscribe(OnDeleted);
        }

        public IObservable<string> ObservablePlaylistsContainer { get; set; }

        public IObservable<string> ObservablePlaylistAdd { get; set; }
        
        public IObservable<string> ObservablePlaylistChange { get; set; }

        public IObservable<string> ObservablePlaylistDelete { get; set; }

        private void OnPlaylistXmlChanged(string f)
        {
            log.Debug("{0} changed", f);
            adapter.RefreshDic();
        }

        private void OnDeleted(string filename)
        {
            log.Debug("{0} deleted", filename);
        }

        private void OnChanged(string filename)
        {
            log.Debug("{0} changed", filename);
        }

        private void OnAdded(string filename)
        {
            log.Debug("{0} added", filename);
        }

        private bool isDisposed;
        private IDisposable playlistsContainerChanges;
        private IDisposable playlistAdding;
        private IDisposable playlistChanging;
        private IDisposable playlistDeleting;
        private WinampM3UPlaylistFileAdapter adapter;

        public WinampFilesMonitor(IComponentContext context, MeloManagerOptions options)
        {
            this.options = options;
            this.context = context;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed) return;

            if (adapter != null)
            {
                adapter = null;
                playlistsContainerChanges.Dispose();
                playlistAdding.Dispose();
                playlistChanging.Dispose();
                playlistDeleting.Dispose();
            }

            isDisposed = true;
        }

    }
}