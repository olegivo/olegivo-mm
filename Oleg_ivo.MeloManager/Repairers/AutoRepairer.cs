using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NLog;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.MeloManager.DependencyInjection;
using Oleg_ivo.MeloManager.MediaObjects;
using File = System.IO.File;

namespace Oleg_ivo.MeloManager.Repairers
{
    public class AutoRepairer : RepairerBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IMediaCache mediaCache;

        public AutoRepairer(MeloManagerOptions options, IComponentContext context, IMediaCache mediaCache)
            : base(options, context)
        {
            this.mediaCache = mediaCache;
        }

        public override void Repair()
        {
            log.Info("Автоматическая починка путей файлов");
            log.Debug("Получение списка плейлистов");
            //var categoryToRepair = new Category();

            /*TODO: PrePlaylist.CreatePlaylist() неявно добавляет в MediaDbContext новые сущности, 
             * благо, что AutoRepairer используется в отдельном режиме работы программы и программа после этого закрывается.
             Если бы MediaDbContext использовался и дальше, то все неявно импортированные через AutoRepairer данные могли случайно сохраниться (хотя это по сути мусор)*/
            var winampPlaylists = WinampM3UPlaylistFileAdapter.GetPlaylists()/*.Select(playlist => playlist.CreatePlaylist()).ToList()*/;
            //categoryToRepair.AddChildren(winampPlaylists);

            /*var playlistFiles =
                PlaylistFilesSearchPatterns.SelectMany(
                    searchPattern => Directory.GetFiles(playlistsPath, searchPattern, SearchOption.TopDirectoryOnly))
                    .Distinct()
                    .ToList();
            log.Debug("Найдено файлов: {0}", playlistFiles.Count);

            log.Debug("Создание плейлистов из файлов");
            foreach (
                var playlist in
                    playlistFiles
                        .Select(file => playlistFileAdapter.FileToPlaylist(file)))
            {
                playlist.Name = Dic[Path.GetFileName(playlist.OriginalFileName)];
                categoryToRepair.AddChild(playlist);
            }*/

            var winampMediaFiles =
                winampPlaylists.SelectMany(playlist => playlist.MediaFiles).ToList();
            var filesSelector = winampMediaFiles.SelectMany(mediaFile => mediaFile.Files);
            var fileInfosSelector = filesSelector.Select(file => file.FileInfo);
            // ReSharper disable PossibleMultipleEnumeration
            var fileInfos = fileInfosSelector.ToList();//multyple enumeration - это норма, т.к. она должна вычисляться каждый раз заново (при починке файлы заменяются)
            // ReSharper restore PossibleMultipleEnumeration

            var count1 = fileInfos.Count();
            var distinctFilesCount = fileInfos.Select(fi => fi.FullName).Distinct().Count();
            //var existsCount1 = fileInfos.Count(fi => fi.Exists);

            var notExists = fileInfos.Where(fi => !fi.Exists).ToList();
            var notExistsCount1 = notExists.Count;

            log.Info("Всего файлов: {0} (уникальных {1}), сломано: {2}", count1, distinctFilesCount, notExistsCount1);
            if (notExistsCount1 > 0)
            {
                for (int i = 0; i < notExistsCount1; i++)
                    log.Trace(notExists[i].FullName);

                log.Debug("Получение списка файлов, используемых для починки");
                string musicFilesSource = Options.MusicFilesSource;
                var files =
                    musicFilesSource.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .SelectMany(
                            path =>
                                MusicFilesSearchPatterns.SelectMany(
                                    searchPattern => Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories)))
                        .ToList();
                log.Debug("Получено файлов: {0}", files.Count);

                foreach (var winampPlaylist in winampPlaylists)
                {
                    winampPlaylist.BatchRepair(files, true, mediaCache);
                }
                var files2 =
                    winampPlaylists
                        .SelectMany(
                            playlist =>
                                playlist.MediaFiles
                                    .SelectMany(mediaFile => mediaFile.Files).Select(file => new { PlaylistName = playlist.Name, File = file })).ToList();
                // ReSharper disable PossibleMultipleEnumeration
                fileInfos = fileInfosSelector.ToList();//multyple enumeration - это норма, т.к. она должна вычисляться каждый раз заново (при починке файлы заменяются)
                // ReSharper restore PossibleMultipleEnumeration
                var count2 = fileInfos.Count();
                var existsCount2 = fileInfos.Count(fi => fi.Exists);
                var notExistsCount2 = fileInfos.Count(fi => !fi.Exists);
                var notExistsFiles = files2.Where(f => !f.File.FileInfo.Exists).GroupBy(f => f.File).ToList();
                var notExistsDirs = notExistsFiles
                    .Select(
                        f =>
                            new
                            {
                                Dir = f.Key.FileInfo.Directory.FullName,
                                FileName = f.Key.FileInfo.FullName,
                                Playlists = f.Select(item => item.PlaylistName).Distinct().JoinToStringFormat("\n", "\t{0}")
                            })
                    .GroupBy(s => s.Dir, EqualityComparer<string>.Default)
                    .OrderBy(dir => dir.Key)
                    .Select(
                        group => string.Format("В папке {0}: {1} шт.\n{2}",
                            group.Key,
                            group.Count(),
                            group.Select(item => string.Format("{0}\nв плейлистах \n{1}", item.FileName, item.Playlists))
                                .JoinToString("\n")))
                    .ToList();

                log.Info("Всего файлов: {0}, было сломано: {1}, осталось сломано: {2}", count2, notExistsCount1, notExistsCount2);
                log.Debug(notExistsDirs.JoinToString("\n"));

                var playlists = winampPlaylists.Where(playlist => playlist.IsRepaired).ToList();
                log.Info("Сохранение плейлистов {0}", playlists.Count);
                EnsureBackupPath("AutoRepair");
                foreach (var playlist in playlists)
                {
                    var originalFileName = playlist.Filename;
                    File.Copy(originalFileName, Path.Combine(BackupPath, Path.GetFileName(originalFileName)));
                    WinampM3UPlaylistFileAdapter.PlaylistToFile(playlist, originalFileName);
                }
            }

            //TODO:здесь может быть проблема: категория это сущность, при связывании её с плейлистами она может быть добавлена в БД вместе с другими вспомогательными сущностями
            //поэтому сбрасываем все изменения, которые могли произойти локально...
            //TODO:по-хорошоме, надо бы ещё лочить DataContext, чтобы и другие не могли случайно сохранить изменения, которые были зделаны здесь
            //((MediaDataContext)mediaCache).Refresh(RefreshMode.OverwriteCurrentValues);//TODO: может оно и не нужно больше

            //Playlist playlist = playlistFileImporter.FileToPlaylist(@"D:\Oleg\ToRepair.m3u");
            log.Info("Починка завершена");
        }
    }
}
