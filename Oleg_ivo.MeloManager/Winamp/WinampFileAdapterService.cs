using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Autofac;
using NLog;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.PlaylistFileAdapters;
using Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff;
using Oleg_ivo.MeloManager.Prism;
using File = System.IO.File;

namespace Oleg_ivo.MeloManager.Winamp
{
    public class WinampFileAdapterService : IFileAdapterService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly MeloManagerOptions options;
        private readonly MediaDbContext dbContext;
        private readonly PlaylistImporter<WinampM3UPlaylistFileAdapter> importer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WinampFileAdapterService(IComponentContext context, MediaDbContext dbContext, MeloManagerOptions options)
        {
            this.options = options;
            importer = context.ResolveUnregistered<PlaylistImporter<WinampM3UPlaylistFileAdapter>>();
            this.dbContext = Enforce.ArgumentNotNull(dbContext, "dbContext");
            dbContext.BeforeSave += dbContext_BeforeSave;
        }

        private readonly ConcurrentDictionary<string, LockState> locks = new ConcurrentDictionary<string, LockState>();

        private enum LockState
        {
            Exporting,
            Importing
        }

        public WinampM3UPlaylistFileAdapter Adapter { get { return importer.Adapter; } }

        private TResult AcquireLock<TResult>(string filename, LockState lockState, Func<TResult> action)
        {
            if (locks.TryAdd(filename, lockState))
            {
                try
                {
                    return action();
                }
                finally
                {
                    if (!locks.TryRemove(filename, out lockState))
                    {
                        throw new InvalidOperationException("Невозможно освободить очередь на импорт/экспорт");
                    }
                }
            }
            
            log.Warn("Запрос на {0} плейлиста {1} отклонён: файл занят ({2})",
                lockState == LockState.Importing ? "импорт" : "экспорт",
                filename,
                locks[filename]);
            return default(TResult);
        }

        private TResult AcquireLock<TResult>(IEnumerable<string> filenames, LockState lockState, Func<TResult> action)
        {
            var lockResults =
                filenames.Select(filename => new {filename, acquired = locks.TryAdd(filename, lockState)})
                    .ToList();
            try
            {
                if (lockResults.All(item => item.acquired))
                {
                    return action();
                }
                else
                {
                    var failed =
                        lockResults.Where(item => !item.acquired)
                            .Select(item => string.Format("файл {0} занят ({1})", item.filename, locks[item.filename]))
                            .ToList();
                    log.Warn("Запрос на {0} плейлистов ({1} шт.) отклонён:\n({2})",
                        lockState == LockState.Importing ? "импорт" : "экспорт",
                        lockResults.Count,
                        failed.JoinToString("\n\t"));
                    return default(TResult);
                }
            }
            finally
            {
                if (
                    lockResults.Where(item => item.acquired)
                        .Select(item => item.filename)
                        .Any(filename => !locks.TryRemove(filename, out lockState)))
                {
                    throw new InvalidOperationException("Невозможно освободить очередь на импорт/экспорт");
                }
            }
        }

        private void AcquireLock(string filename, LockState lockState, Action action)
        {
            AcquireLock(filename,
                lockState,
                () =>
                {
                    action();
                    return Unit.Default;
                });
        }

        public void RequestImport(string importFilename)
        {
            AcquireLock(importFilename, LockState.Importing, () => Import(importFilename));
        }

        public void RequestExport(Playlist playlist, string exportFilename)
        {
            AcquireLock(exportFilename, LockState.Exporting, () => Export(playlist, exportFilename));
        }

        public bool RequestExport(IEnumerable<Tuple<Playlist, string>> playlists)
        {
            var list = playlists as IList<Tuple<Playlist, string>> ?? playlists.ToList();
            return
                AcquireLock(list.Select(item=>item.Item2),
                    LockState.Exporting,
                    () =>
                    {
                        foreach (var tuple in list)
                        {
                            Export(tuple.Item1, tuple.Item2);
                        }
                        return true;
                    });
        }


        public IList<string> GetPlaylistFiles(bool onlyChanged)
        {
            return onlyChanged
                ? GetChangedPlaylists()
                : Adapter.Dic.Keys.Select(f => Path.Combine(options.PlaylistsPath, f))
                    .Where(File.Exists)
                    .ToList();
        }

        public void RefreshPlaylistFilesContainer()
        {
            Adapter.RefreshDic();
        }

        private List<string> GetChangedPlaylists()
        {
            var now = DateTime.Now;
            List<string> changedPlaylists;
            if (options.LastPlaylistsImportDate.Equals(DateTime.MinValue))
                changedPlaylists = null;
            else
            {
                changedPlaylists =
                    Adapter.Dic.Keys.Select(f => new FileInfo(Path.Combine(options.PlaylistsPath, f)))
                        .Where(info => info.LastWriteTime > options.LastPlaylistsImportDate)
                        .Select(info => info.FullName)
                        .ToList();
                log.Debug("Обнаружено изменённых плейлистов: {0} (с {1})\n{2}", changedPlaylists.Count,
                    options.LastPlaylistsImportDate, changedPlaylists.JoinToString("\n"));
            }
            options.LastPlaylistsImportDate = now;
            return changedPlaylists;
        }


        private void Export(Playlist playlist, string exportFilename)
        {
            var diffAction = importer.GetExportDiffAction(playlist, exportFilename);
            if (diffAction.DiffType == DiffType.None)
                log.Debug("При попытке экспорта плейлиста {0} в файл [{1}] не обнаружилось различий", playlist, exportFilename);
            else
                diffAction.Apply();
        }

/*
        private class Temp : IMediaFilesContainer//TODO: костыль на будущее: если в классе Playlist реализовать IEnumerable<MediaFile>, перестанут работать запросы на фильтрацию
        {
            private readonly IEnumerable<MediaFile> mediaFiles;
            private readonly Playlist playlist;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public Temp(Playlist playlist)
            {
                this.mediaFiles = playlist.MediaFiles;
                this.playlist = playlist;
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// An enumerator that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<MediaFile> GetEnumerator()
            {
                return mediaFiles.GetEnumerator();
            }

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>
            /// A string that represents the current object.
            /// </returns>
            public override string ToString()
            {
                return playlist.ToString();
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)mediaFiles).GetEnumerator();
            }
        }
*/


        public Task<bool> RequestImport(IEnumerable<string> playlistFilenames, Category winampCategory)
        {
            var enumerable = playlistFilenames as IList<string> ?? playlistFilenames.ToList();
            return
                AcquireLock(enumerable,
                    LockState.Importing,
                    () => Task.Run(() => Import(enumerable, winampCategory).Any()))
                ?? Task.FromResult(false);
        }

        private IEnumerable<Playlist> Import(IEnumerable<string> playlistFilenames, Category winampCategory)
        {
            return importer.Import(playlistFilenames, winampCategory);
        }

        private void Import(string filename)
        {
            var diffAction = importer.GetImportDiffAction(filename);
            if (diffAction.DiffType == DiffType.None)
                log.Debug("При попытке импорта файла плейлиста {0} не обнаружилось различий", filename);
            else
                diffAction.Apply();
        }

        void dbContext_BeforeSave(object sender, MediaDbContext.SaveEventArgs e)
        {
            var affectedPlaylists = dbContext.GetAffectedPlaylists()
                .Select(playlist => new Tuple<Playlist, string>(playlist, playlist.GetOriginalFileName(dbContext)))
                .ToList();

            if (affectedPlaylists.Any())
            {
                var exportResult = RequestExport(affectedPlaylists);
                log.Debug("Результат экспорта плейлистов в файлы ({0} шт.): {1}", affectedPlaylists.Count, exportResult ? "успешно" : "неудачно");
                e.Cancel = !exportResult;
            }
        }
    }

}