using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Autofac;
using NLog;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.PlaylistFileAdapters;
using Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff;

namespace Oleg_ivo.MeloManager.Winamp
{
    public class WinampFileAdapterService : IFileAdapterService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly PlaylistImporter<WinampM3UPlaylistFileAdapter> importer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WinampFileAdapterService(IComponentContext context)
        {
            importer = context.ResolveUnregistered<PlaylistImporter<WinampM3UPlaylistFileAdapter>>();
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
            AcquireLock(exportFilename, LockState.Exporting, () => importer.Adapter.PlaylistToFile(playlist, exportFilename));
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

        public void Save()
        {
            importer.DbContext.SaveChanges();
        }
    }

}