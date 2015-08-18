using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using NLog;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.MeloManager.Extensions;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.PlaylistFileAdapters;
using Oleg_ivo.MeloManager.Prism;

namespace Oleg_ivo.MeloManager.Winamp
{
    public class WinampFilesMonitor : IDisposable//TODO: взять некоторые вещи из монитора папок?
    {
        private readonly IFileAdapterService fileAdapterService;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly MeloManagerOptions options;
        private readonly CompositeDisposable disposer = new CompositeDisposable();

        public WinampFilesMonitor(MeloManagerOptions options, IFileAdapterService fileAdapterService)
        {
            this.fileAdapterService = Enforce.ArgumentNotNull(fileAdapterService, "fileAdapterService");
            this.options = Enforce.ArgumentNotNull(options, "options");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            disposer.Dispose();
        }

        public bool RunImport(Category winampCategory, bool onlyChanged)
        {
            lock (fileAdapterService)
            {
                var playlistsToImport = fileAdapterService.GetPlaylistFiles(onlyChanged);
                return playlistsToImport != null && playlistsToImport.Any() &&
                       fileAdapterService.RequestImport(playlistsToImport, winampCategory).Result;
            }
        }

        public void MonitorFilesChanges()
        {
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
            var playlistsContainerThrottleTime = TimeSpan.FromSeconds(0.3);
            var playlistFileThrottleTime = TimeSpan.FromSeconds(1);
            var observablePlaylistsContainerChanged = fileWatcherPlaylistsContainer.ToObservable(WatcherChangeTypes.Changed, playlistsContainerThrottleTime).Do(OnPlaylistXmlChanged);
            //TODO: для того, чтобы взять актуальное значение название плейлиста из словаря, нужно, чтобы сначала сработал ObservablePlaylistsContainerChanged, нужно его ждать
            var observablePlaylistAdd =
                fileWatcherPlaylists.ToObservable(WatcherChangeTypes.Created, playlistFileThrottleTime)
                    .Window(() => observablePlaylistsContainerChanged)
                    .SelectMany(window => window/*.TakeLast(1)*/);//TODO: в случае можем потерять часть элементов
            var observablePlaylistChange = fileWatcherPlaylists.ToObservable(WatcherChangeTypes.Changed, playlistFileThrottleTime);
            var observablePlaylistDelete = fileWatcherPlaylists.ToObservable(WatcherChangeTypes.Deleted, playlistFileThrottleTime);

            //disposer.Add(observablePlaylistsContainerChanged.Subscribe(OnPlaylistXmlChanged));
            disposer.Add(observablePlaylistAdd.Subscribe(OnAdded));//TODO: почему-то не срабатывает сразу
            disposer.Add(observablePlaylistChange.Subscribe(OnChanged));
            disposer.Add(observablePlaylistDelete.Subscribe(OnDeleted));
        }

        private void OnPlaylistXmlChanged(string filename)
        {
            lock (fileAdapterService)
            {
                log.Debug("{0} changed", filename);
                fileAdapterService.RefreshPlaylistFilesContainer();
            }
        }

        private void OnDeleted(string filename)
        {
            lock (fileAdapterService)
            {
                log.Debug("{0} deleted", filename);
            }
        }

        private void OnChanged(string filename)
        {
            lock (fileAdapterService)
            {
                log.Debug("{0} changed", filename);
                fileAdapterService.RequestImport(filename);
            }
        }

        private void OnAdded(string filename)
        {
            lock (fileAdapterService)
            {
                log.Debug("{0} added", filename);
                //TODO: импорт нового плейлиста, но перед этим нужно разобраться с ожиданием обновления словаря их xml (см. метод MonitorFilesChanges)
                fileAdapterService.RequestImport(filename);
            }
        }

    }
}