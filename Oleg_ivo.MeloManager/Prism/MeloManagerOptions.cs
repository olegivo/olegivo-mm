using System;
using Oleg_ivo.MeloManager.Properties;
using Oleg_ivo.Tools.Utils;

namespace Oleg_ivo.MeloManager.Prism
{
    public class MeloManagerOptions
    {
        /// <summary>
        /// Название конфигурации
        /// </summary>
        public string ConfigName { get; set; }

        /// <summary>
        /// Путь, по которому располагаются плейлисты, которые необходимо починить (без конечного "\")
        /// </summary>
        public string PlaylistsPath
        {
            get { return Utils.FileUtils.UnwrapEnvironmentBasedPath(Settings.Default.PlaylistsPath); }
        }

        /// <summary>
        /// Пути (через ";"), содержащие музыкальные файлы (без конечных "\")
        /// </summary>
        public string MusicFilesSource { get { return Settings.Default.MusicFilesSource; } }

        /// <summary>
        /// Режим автоматической починки. Для данного режима следует указывать также параметры <see cref="PlaylistsPath"/> и <see cref="MusicFilesSource"/>
        /// </summary>
        public bool RepairMode { get; set; }

        /// <summary>
        /// Режим переименования
        /// </summary>
        public bool Mp3TagRenameMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Mp3TagRenamePreviewFileName { get { return Utils.FileUtils.UnwrapEnvironmentBasedPath(Settings.Default.Mp3TagRenamePreviewFileName); } }

        /// <summary>
        /// 
        /// </summary>
        public string ConnectionString { get { return Settings.Default.ConnectionString; } }

        public DateTime LastPlaylistsImportDate
        {
            get { return Settings.Default.LastPlaylistsImportDate; }
            set
            {
                Settings.Default.LastPlaylistsImportDate = value;
                Settings.Default.Save();
            }
        }
    }
}