using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    /// <summary>
    /// 
    /// </summary>
    public class M3UPlaylistFileAdapter : PlaylistFileAdapter
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected readonly IMediaCache MediaCache;

        protected M3UPlaylistFileAdapter(IMediaCache mediaCache)
        {
            MediaCache = Enforce.ArgumentNotNull(mediaCache, "mediaCache");
        }

        public override Playlist FileToPlaylist(string filename, string playlistName = null)
        {
            log.Info("Создание плейлиста из файла [{0}]", filename);
            //var regex =
            //    new Regex(
            //        @"<div\b[^>]*\sclass=""custom_param.custom_param_n"".+<label>(?<paramName>.+?)</label>.+<div\b[^>]*\sclass=""custom_param_error""[^>]*>(?<tagContent>.+?)</div>",
            //        RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

            List<ExtInfo> infos = GetExtInfosFromM3UFile(filename);
            Playlist playlist = null;
            if (infos != null)
            {
                playlist = new Playlist {MediaCache = MediaCache, OriginalFileName = filename, Name = playlistName};
                foreach (var extInfo in infos)
                {
                    var mediaFile = GetOrAddCachedMediaFile(extInfo.filename);
                    playlist.AddChildMediaFile(mediaFile);
                }
            }

            return playlist;
        }

        private MediaFile GetOrAddCachedMediaFile(string filename)
        {
            return MediaCache.GetOrAddCachedMediaFile(filename);
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

        private List<ExtInfo> GetExtInfosFromM3UFile(String playlistFileName)
        {
            var lines =
                System.IO.File.ReadAllLines(playlistFileName, Encoding.UTF8)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            var extInfos = new List<ExtInfo>();
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

            var duplicates = extInfos.GroupBy(info => info.filename.ToLower()).Where(group => group.Count() > 1).SelectMany(group => group.Skip(1)).ToList();
            if(duplicates.Any())
                log.Debug("Обнаружены дубликаты, которые будут исключены из импорта:\n{0}", duplicates.Select(info => info.filename).JoinToString("\n "));

            return extInfos.Except(duplicates).ToList();
        }
    }
}