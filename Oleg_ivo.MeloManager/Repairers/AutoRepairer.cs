using System;
using System.Collections.Generic;
using System.Data.Linq;
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
            var categoryToRepair = new Category();

            categoryToRepair.AddChildren(WinampM3UPlaylistFileAdapter.GetPlaylists().Select(playlist => playlist.CreatePlaylist()));

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
                categoryToRepair.Children.Cast<Playlist>()
                    .SelectMany(
                        playlist =>
                            playlist.Children.Cast<MediaFile>()
                                .SelectMany(mediaFile => mediaFile.MediaContainerFiles.Select(mcf => mcf.File.FileInfo))).ToList();

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

                categoryToRepair.BatchRepair(files, true, mediaCache);
                var files2 =
                    categoryToRepair.Children.Cast<Playlist>()
                        .SelectMany(
                            playlist =>
                                playlist.Children.Cast<MediaFile>()
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
                                Playlists = fi.MediaContainerFiles.Single().MediaContainer.Parents
                            })
                    .GroupBy(s => s.Dir, EqualityComparer<string>.Default)
                    .OrderBy(dir => dir.Key)
                    .Select(
                        group =>
                            string.Format("{0}: {1}\n{2}", group.Key, group.Count(),
                                group.Select(item => string.Format("{0}\nв плейлистах \n{1}", item.FileName, item.Playlists.Select(mc => mc.ToString()).JoinToString("\n")))
                                    .JoinToStringFormat("\n", "\t{0}")))
                    .ToList();

                log.Info("Всего файлов: {0}, было сломано: {1}, осталось сломано: {2}", count2, notExistsCount1, notExistsCount2);
                log.Debug(notExistsDirs.JoinToString("\n"));

                var playlists = categoryToRepair.Children.Cast<Playlist>().Where(playlist => playlist.IsRepaired).ToList();
                log.Info("Сохранение плейлистов {0}", playlists.Count);
                EnsureBackupPath("AutoRepair");
                foreach (var playlist in playlists)
                {
                    File.Copy(playlist.OriginalFileName, Path.Combine(BackupPath, Path.GetFileName(playlist.OriginalFileName)));
                    WinampM3UPlaylistFileAdapter.PlaylistToFile(playlist, playlist.OriginalFileName);
                }
            }

            //TODO:здесь может быть проблема: категория это сущность, при связывании её с плейлистами она может быть добавлена в БД вместе с другими вспомогательными сущностями
            //поэтому сбрасываем все изменения, которые могли произойти локально...
            //TODO:по-хорошоме, надо бы ещё лочить DataContext, чтобы и другие не могли случайно сохранить изменения, которые были зделаны здесь
            ((MediaDataContext)mediaCache).Refresh(RefreshMode.OverwriteCurrentValues);

            //Playlist playlist = playlistFileImporter.FileToPlaylist(@"D:\Oleg\ToRepair.m3u");
            log.Info("Починка завершена");
        }
    }
}
