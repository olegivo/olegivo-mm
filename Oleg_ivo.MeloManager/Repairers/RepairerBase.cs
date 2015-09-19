using System;
using System.IO;
using Autofac;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.MeloManager.DependencyInjection;
using Oleg_ivo.MeloManager.PlaylistFileAdapters;

namespace Oleg_ivo.MeloManager.Repairers
{
    public abstract class RepairerBase
    {
        public abstract void Repair();

        protected static readonly string[] MusicFilesSearchPatterns = { "*.mp3", "*.wma", "*.ogg", "*.flac", "*.aac" };

        protected readonly MeloManagerOptions Options;
        protected string BackupPath;
        protected readonly WinampM3UPlaylistFileAdapter WinampM3UPlaylistFileAdapter;

        protected RepairerBase(MeloManagerOptions options, IComponentContext context)
        {
            Options = Enforce.ArgumentNotNull(options, "options");
            WinampM3UPlaylistFileAdapter = Enforce.NotNull(Enforce.ArgumentNotNull(context, "context").ResolveUnregistered<WinampM3UPlaylistFileAdapter>());
        }

        protected void EnsureBackupPath(string suffix)
        {
            if (BackupPath == null)
                BackupPath = Path.Combine(Path.Combine(Options.PlaylistsPath, "Backup" + suffix),
                    DateTime.Now.ToString("O").Replace(':', '-'));
            if (!Directory.Exists(BackupPath))
                Directory.CreateDirectory(BackupPath);
        }

    }
}