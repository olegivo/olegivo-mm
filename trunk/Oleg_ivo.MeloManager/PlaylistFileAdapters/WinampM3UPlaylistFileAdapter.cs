using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NLog;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.Prism;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    public class WinampM3UPlaylistFileAdapter : M3UPlaylistFileAdapter
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public WinampM3UPlaylistFileAdapter(MeloManagerOptions options)
        {
            Options = Enforce.ArgumentNotNull(options, "options");
            Dic = GetPlaylistsDictionary(options.PlaylistsPath);
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

        public List<Playlist> GetPlaylists()
        {
            var playlistFiles = GetPlaylistsFiles(PlaylistFilesSearchPatterns);
            log.Debug("Найдено файлов: {0}", playlistFiles.Count);

            log.Debug("Создание плейлистов из файлов");
            List<Playlist> playlists = new List<Playlist>();
            foreach (var playlist in playlistFiles.Select(FileToPlaylist))
            {
                playlist.Name = Dic[Path.GetFileName(playlist.OriginalFileName)];
                playlists.Add(playlist);
            }

            return playlists;
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