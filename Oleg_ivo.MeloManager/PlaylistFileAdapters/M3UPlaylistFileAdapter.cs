﻿using System;
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

        public override PrePlaylist FileToPlaylist(string filename, string playlistName = null)
        {
            log.Info("Создание плейлиста {0} из файла [{1}]", playlistName != null ? String.Format("[{0}]", playlistName) : null, filename);
            //var regex =
            //    new Regex(
            //        @"<div\b[^>]*\sclass=""custom_param.custom_param_n"".+<label>(?<paramName>.+?)</label>.+<div\b[^>]*\sclass=""custom_param_error""[^>]*>(?<tagContent>.+?)</div>",
            //        RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var infos = GetExtInfosFromM3UFile(filename);
            PrePlaylist playlist = null;
            if (infos != null)
            {
                playlist = new PrePlaylist(MediaCache)
                {
                    Filename = filename, 
                    Name = playlistName,
                    MediaFiles = infos.Select(extInfo => MediaCache.GetOrAddCachedMediaFile(extInfo.filename)).ToList()
                };
            }

            return playlist;
        }

        public override void MediaFilesToFile(string filename, IEnumerable<MediaFile> mediaFiles)
        {
            var files = mediaFiles
                .Select(mf =>
                    mf.Files
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
            if (!lines.Any())
                return extInfos;

            if (lines.First().Trim().ToUpper() == "#EXTM3U")
            {
                String title = String.Empty;
                for (int i = 0; i < lines.Count; i++)
                {
                    string s = lines[i];
                    if (s.StartsWith("#EXTINF", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string[] info = s.Split(new[] {":", ","}, StringSplitOptions.None);
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