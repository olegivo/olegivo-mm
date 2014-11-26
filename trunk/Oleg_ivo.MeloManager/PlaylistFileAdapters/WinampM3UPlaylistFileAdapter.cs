using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NLog;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.Prism;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    public class WinampM3UPlaylistFileAdapter : M3UPlaylistFileAdapter
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public WinampM3UPlaylistFileAdapter(MeloManagerOptions options, IMediaCache mediaCache):base(mediaCache)
        {
            Options = Enforce.ArgumentNotNull(options, "options");
            RefreshDic();
        }

        public void RefreshDic()
        {
            //TODO: var folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            var newDic = GetPlaylistsDictionary(Options.PlaylistsPath);
            if (Dic == null)
                Dic = newDic;
            else
            {
                var foj = Dic.Keys.FullOuterJoin(newDic.Keys).ToList();
                int added = 0, deleted = 0, updated = 0;
                foreach (var item in foj)
                {
                    if (item.Item1 == null)
                    {
                        Dic.Add(item.Item2, newDic[item.Item2]);
                        added++;
                    }
                    else if (item.Item2 == null)
                    {
                        Dic.Remove(item.Item1);
                        deleted++;
                    }
                    else
                    {
                        Dic[item.Item1] = newDic[item.Item1];
                        updated++;
                    }
                }
                log.Debug("Добавлено: {0}, удалено: {1}, обновлено: {2}", added, deleted, updated);
            }
        }

        public Dictionary<string, string> Dic { get; private set; }

        private Dictionary<string, string> GetPlaylistsDictionary(string playlistsPath)
        {
            return
                XDocument.Load(Path.Combine(playlistsPath, "playlists.xml"))
                    .Root.Elements()
                    .ToDictionary(xElement => xElement.Attribute("filename").Value,
                        xElement => xElement.Attribute("title").Value);
        }

        private MeloManagerOptions Options { get; set; }

        public override PrePlaylist FileToPlaylist(string filename, string playlistName = null)
        {
            var playlist = base.FileToPlaylist(filename, playlistName: Dic[Path.GetFileName(filename)]);
            return playlist;
        }

        public List<PrePlaylist> GetPlaylists()
        {
            var playlistFiles = GetPlaylistsFiles(PlaylistFilesSearchPatterns);
            log.Debug("Найдено файлов: {0}", playlistFiles.Count);

            log.Debug("Создание плейлистов из файлов");

            return playlistFiles.Select(filename => FileToPlaylist(filename)).ToList();
        }

        public List<string> GetPlaylistsFiles(string[] playlistFilesSearchPatterns)
        {
            log.Debug("Получение списка плейлистов");
            var playlistFiles = playlistFilesSearchPatterns.SelectMany(
                searchPattern => Directory.GetFiles(Options.PlaylistsPath, searchPattern, SearchOption.TopDirectoryOnly))
                .Distinct()
                .ToList();
            log.Debug("Получено плейлистов: {0}", playlistFiles.Count);
            return playlistFiles;
        }
    }
}