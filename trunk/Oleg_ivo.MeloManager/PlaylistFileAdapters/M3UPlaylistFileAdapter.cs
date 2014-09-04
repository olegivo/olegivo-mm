using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    /// <summary>
    /// 
    /// </summary>
    public class M3UPlaylistFileAdapter : PlaylistFileAdapter
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override Playlist FileToPlaylist(string filename)
        {
            log.Info("Создание плейлиста из файла [{0}]", filename);
            //var regex =
            //    new Regex(
            //        @"<div\b[^>]*\sclass=""custom_param.custom_param_n"".+<label>(?<paramName>.+?)</label>.+<div\b[^>]*\sclass=""custom_param_error""[^>]*>(?<tagContent>.+?)</div>",
            //        RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

            List<ExtInfo> infos = GetExtInfosFromFile_m3u(filename);
            Playlist playlist = null;
            if (infos != null)
            {
                playlist = new Playlist {OriginalFileName = filename};
                foreach (var extInfo in infos)
                {
                    playlist.AddChildMediaFile(extInfo.GetMediaFile());
                }
            }

            return playlist;
        }

        public override void MediaFilesToFile(string filename, IEnumerable<MediaFile> mediaFiles)
        {
            var files = mediaFiles
                .Select(mf =>
                    mf.MediaContainerFiles.Select(mcf => mcf.File)
                        .OrderBy(file => file.FileInfo.Exists)
                        .FirstOrDefault())
                .Select(file => string.Format("#EXTINF:-1,{0}\n{1}", file.FileNameWithoutExtension, file.FullFileName))
                .Distinct()
                .ToList();
            files.Insert(0, "#EXTM3U");
            System.IO.File.WriteAllLines(filename, files, Encoding.UTF8);
        }

        private List<ExtInfo> GetExtInfosFromFile_m3u(String playlistFileName)
        {
            List<string> lines =
                System.IO.File.ReadAllLines(playlistFileName, Encoding.UTF8)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            List<ExtInfo> extInfos = new List<ExtInfo>();
            if (lines[0].Trim().ToUpper() == "#EXTM3U")
            {
                String title = String.Empty;
                for (int i = 0; i < lines.Count; i++)
                {
                    string s = lines[i];
                    if (s.StartsWith("#EXTINF", StringComparison.CurrentCultureIgnoreCase))
                    {
                        String[] info = s.Split(new[] {":", ","}, StringSplitOptions.None);
                        if (info.Length > 2)
                            title = info[2];
                        extInfos.Add(new ExtInfo(title, lines[++i]));
                    }
                    else if (s.StartsWith("# ", StringComparison.CurrentCultureIgnoreCase))
                    {
                        title = s.Substring(2);
                        extInfos.Add(new ExtInfo(title, lines[++i]));
                    }
                    title = String.Empty;
                }
            }
            else
            {
                extInfos.AddRange(lines.Select(t => new ExtInfo(String.Empty, t.Trim())));
            }

            return extInfos;
        }
    }
}