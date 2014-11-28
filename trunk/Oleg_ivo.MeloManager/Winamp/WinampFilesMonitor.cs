using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using Autofac;
using NLog;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.MeloManager.Extensions;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.PlaylistFileAdapters;
using Oleg_ivo.MeloManager.Prism;

namespace Oleg_ivo.MeloManager.Winamp
{
    public class WinampFilesMonitor//TODO: взять некоторые вещи из монитора папок?
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly MeloManagerOptions options;
        private readonly PlaylistImporter<WinampM3UPlaylistFileAdapter> importer;
        private readonly CompositeDisposable disposer = new CompositeDisposable();

        public WinampFilesMonitor(MeloManagerOptions options, IComponentContext context)
        {
            importer = context.ResolveUnregistered<PlaylistImporter<WinampM3UPlaylistFileAdapter>>();
            this.options = Enforce.ArgumentNotNull(options, "options");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            disposer.Dispose();
        }

        public IEnumerable<Playlist> RunImport(Category winampCategory, IEnumerable<string> changedPlaylists)
        {
            lock (importer)
            {
                var playlistFilenames = changedPlaylists ?? importer.Adapter.Dic.Keys.Select(f => Path.Combine(options.PlaylistsPath, f));
                return importer.RunImportAll(playlistFilenames, winampCategory);
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
            var observablePlaylistsContainer = fileWatcherPlaylistsContainer.ToObservable(WatcherChangeTypes.Changed, playlistsContainerThrottleTime);
            var observablePlaylistAdd = fileWatcherPlaylists.ToObservable(WatcherChangeTypes.Created, playlistFileThrottleTime)/*.Select(filename => )*/;//TODO: для того, чтобы взять актуальное значение название плейлиста из словаря, нужно, чтобы сначала сработал ObservablePlaylistsContainer, нужно его ждать
            var observablePlaylistChange = fileWatcherPlaylists.ToObservable(WatcherChangeTypes.Changed, playlistFileThrottleTime);
            var observablePlaylistDelete = fileWatcherPlaylists.ToObservable(WatcherChangeTypes.Deleted, playlistFileThrottleTime);

            disposer.Add(observablePlaylistsContainer.Subscribe(OnPlaylistXmlChanged));
            disposer.Add(observablePlaylistAdd.Subscribe(OnAdded));
            disposer.Add(observablePlaylistChange.Subscribe(OnChanged));
            disposer.Add(observablePlaylistDelete.Subscribe(OnDeleted));
        }

        private void OnPlaylistXmlChanged(string f)
        {
            lock (importer)
            {
                log.Debug("{0} changed", f);
                importer.Adapter.RefreshDic();
            }
        }

        private void OnDeleted(string filename)
        {
            log.Debug("{0} deleted", filename);
        }



        private void OnChanged(string filename)
        {
            lock (importer)
            {
                log.Debug("{0} changed", filename);
                importer.Import(filename);
                importer.DataContext.SubmitChanges();
            }
        }

        private void OnAdded(string filename)
        {
            lock (importer)
            {
                log.Debug("{0} added", filename);
                //TODO: импорт нового плейлиста, но перед этим нужно разобраться с ожиданием обновления словаря их xml (см. метод MonitorFilesChanges)
                //importer.Import(filename);
                //importer.DataContext.SubmitChanges();
            }
        }

        public List<string> GetChangedPlaylists()
        {
            var now = DateTime.Now;
            var changedPlaylists = options.LastPlaylistsImportDate.Equals(DateTime.MinValue)
                ? null
                : importer.Adapter.Dic.Keys.Select(f => new FileInfo(Path.Combine(options.PlaylistsPath, f)))
                    .Where(info => info.LastWriteTime > options.LastPlaylistsImportDate)
                    .Select(info => info.FullName)
                    .ToList();
            options.LastPlaylistsImportDate = now;
            return changedPlaylists;
        }
    }
}