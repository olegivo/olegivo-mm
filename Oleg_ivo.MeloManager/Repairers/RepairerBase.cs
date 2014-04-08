using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.MeloManager.Prism;

namespace Oleg_ivo.MeloManager.Repairers
{
    public abstract class RepairerBase
    {
        public abstract void Repair();

        protected readonly static string[] PlaylistFilesSearchPatterns = { "*.m3u8", "*.m3u" };
        protected static readonly string[] MusicFilesSearchPatterns = { "*.mp3", "*.wma", "*.ogg" };

        protected readonly MeloManagerOptions Options;
        protected readonly Dictionary<string, string> Dic;
        protected string BackupPath;

        protected RepairerBase(MeloManagerOptions options)
        {
            Options = Enforce.ArgumentNotNull(options, "options");
            Dic = GetPlaylistsDictionary(options.PlaylistsPath);
        }

        protected void EnsureBackupPath(string suffix)
        {
            if (BackupPath == null)
                BackupPath = Path.Combine(Path.Combine(Options.PlaylistsPath, "Backup" + suffix),
                    DateTime.Now.ToString("O").Replace(':', '-'));
            if (!Directory.Exists(BackupPath))
                Directory.CreateDirectory(BackupPath);
        }

        protected Dictionary<string,string> GetPlaylistsDictionary(string playlistsPath)
        {
            return
                XDocument.Load(Path.Combine(playlistsPath, "playlists.xml"))
                    .Root.Elements()
                    .ToDictionary(xElement => xElement.Attribute("filename").Value,
                        xElement => xElement.Attribute("title").Value);
        }
    }
}