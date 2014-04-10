using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NLog;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.Prism;
using File = System.IO.File;

namespace Oleg_ivo.MeloManager.Repairers
{
    public class AutoRepairer : RepairerBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public AutoRepairer(MeloManagerOptions options, IComponentContext context)
            : base(options, context)
        {
        }

        public override void Repair()
        {
            log.Info("Автоматическая починка путей файлов");
            log.Debug("Получение списка плейлистов");
            var categoryToRepair = new Category();

            categoryToRepair.AddChildren(WinampM3UPlaylistFileAdapter.GetPlaylists());

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

            var fileInfos =
                categoryToRepair.Childs.Cast<Playlist>()
                    .SelectMany(
                        playlist =>
                            playlist.Childs.Cast<MediaFile>()
                                .SelectMany(mediaFile => mediaFile.MediaContainerFiles.Select(mcf => mcf.File.FileInfo))).ToList();

            var count1 = fileInfos.Count();
            var distinctFilesCpount = fileInfos.Select(fi => fi.FullName).Distinct().Count();
            //var existsCount1 = fileInfos.Count(fi => fi.Exists);
            var notExistsCount1 = fileInfos.Count(fi => !fi.Exists);

            log.Info("Всего файлов: {0} (уникальных {1}), сломано: {2}", count1, distinctFilesCpount, notExistsCount1);
            if (notExistsCount1 > 0)
            {
                log.Debug("Получение списка файлов, используемых для починки");
                string musicFilesSource = Options.MusicFilesSource;
                var files =
                    musicFilesSource.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .SelectMany(
                            path =>
                                MusicFilesSearchPatterns.SelectMany(
                                    searchPattern => Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories)))
                        .ToList();
                log.Debug("Полчено файлов: {0}", files.Count);

                categoryToRepair.BatchRepair(files, true);
                var files2 =
                    categoryToRepair.Childs.Cast<Playlist>()
                        .SelectMany(
                            playlist =>
                                playlist.Childs.Cast<MediaFile>()
                                    .SelectMany(
                                        mediaFile =>
                                            mediaFile.MediaContainerFiles.Where(mcf => mcf.File != null)
                                                .Select(mcf => mcf.File))).ToList();
                fileInfos = files2.Select(f => f.FileInfo).ToList();
                var count2 = fileInfos.Count();
                var existsCount2 = fileInfos.Count(fi => fi.Exists);
                var notExistsCount2 = fileInfos.Count(fi => !fi.Exists);
                var notExistsFiles = files2.Where(fi => !fi.FileInfo.Exists).Distinct().ToList();
                var notExistsDirs = notExistsFiles
                    .Select(
                        fi =>
                            new
                            {
                                Dir = fi.FileInfo.Directory.FullName,
                                FileName = fi.FileInfo.FullName,
                                Name = fi.MediaContainerFiles.Single().MediaContainer.Parents.Single().ToString()
                            })
                    .GroupBy(s => s.Dir, EqualityComparer<string>.Default)
                    .OrderBy(dir => dir.Key)
                    .Select(
                        group =>
                            string.Format("{0}: {1}\n{2}", group.Key, group.Count(),
                                group.Select(item => string.Format("{0}: {1}", item.Name, item.FileName))
                                    .JoinToStringFormat("\n", "\t{0}")))
                    .ToList();

                log.Info("Всего файлов: {0}, было сломано: {1}, осталось сломано: {2}", count2, notExistsCount1, notExistsCount2);
                log.Debug(notExistsDirs.JoinToString("\n"));

                log.Info("Сохраненеи плейлистов");
                EnsureBackupPath("AutoRepair");
                foreach (var playlist in categoryToRepair.Childs.Cast<Playlist>())
                {
                    File.Copy(playlist.OriginalFileName, Path.Combine(BackupPath, Path.GetFileName(playlist.OriginalFileName)));
                    WinampM3UPlaylistFileAdapter.PlaylistToFile(playlist, playlist.OriginalFileName);
                }
            }

            //Playlist playlist = playlistFileImporter.FileToPlaylist(@"D:\Oleg\ToRepair.m3u");
            log.Info("Починка завершена");
        }
    }
}
