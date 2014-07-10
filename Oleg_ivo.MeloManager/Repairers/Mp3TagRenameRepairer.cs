using System;
using System.IO;
using System.Linq;
using Autofac;
using NLog;
using Oleg_ivo.MeloManager.PlaylistFileAdapters;
using Oleg_ivo.MeloManager.Prism;

namespace Oleg_ivo.MeloManager.Repairers
{
    public class Mp3TagRenameRepairer : RepairerBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public Mp3TagRenameRepairer(MeloManagerOptions options, IComponentContext context)
            : base(options, context)
        {
        }

        private static string GetString(string s)
        {
            if (s != null)
                s = s.Trim().Trim('"');
            return s;
        }

        public override void Repair()
        {
            log.Info("Замена имён файлов на новые после пеерименования с помощью Mp3Tag");
            log.Debug("Получение списка замен");
            var mp3TagRenamePreviewFileName = Options.Mp3TagRenamePreviewFileName;
            if (!File.Exists(mp3TagRenamePreviewFileName))
            {
                log.Error("Не найден файл, содержащий список замен [{0}]", mp3TagRenamePreviewFileName);
                return;
            }

            var newLine = Environment.NewLine;
            var replacements = File.ReadAllText(mp3TagRenamePreviewFileName)
                .Split(new[] {newLine + newLine}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Split(new[] {"->" + newLine}, StringSplitOptions.RemoveEmptyEntries))
                .Select(arr => new {Old = GetString(arr[0]), New = GetString(arr[1])})
                .ToList();
            log.Debug("Получено замен: {0}", replacements.Count);
            if (!replacements.Any())
                return;

            var playlistFiles =
                WinampM3UPlaylistFileAdapter.GetPlaylistsFiles(PlaylistFileAdapter.PlaylistFilesSearchPatterns);

            var items = playlistFiles.Select(playlistFile =>
            {
                var fileName = Path.GetFileName(playlistFile);
                var replacedPlaylistContent =
                    replacements.Aggregate(new
                    {
                        Content = File.ReadAllText(playlistFile),
                        Replaces = 0
                    },
                        (current, replacement) =>
                        {
                            var content = current.Content.Replace(replacement.Old, replacement.New);
                            var foo = new
                            {
                                Content = content,
                                Replaces = content == current.Content ? current.Replaces : current.Replaces + 1
                            };
                            return foo;
                        });
                return new {playlistFile, replacedPlaylistContent, fileName};
            }).Where(item => item.replacedPlaylistContent.Replaces > 0).ToList();
            foreach (var item in items)
            {
                log.Debug("Замена в файле [{0}] ({1})", item.playlistFile, WinampM3UPlaylistFileAdapter.Dic[item.fileName]);
                log.Debug("Сделано замен: {0}", item.replacedPlaylistContent.Replaces);
                EnsureBackupPath("Mp3TagRename");
                File.Copy(item.playlistFile, Path.Combine(BackupPath, item.fileName));
                File.WriteAllText(item.playlistFile, item.replacedPlaylistContent.Content);
            }
            log.Info("Замена имён файлов завершена");
        }
    }
}