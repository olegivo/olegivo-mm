using System;
using System.Collections.Specialized;
using Microsoft.Win32;
using Oleg_ivo.MeloManager.Properties;
using Oleg_ivo.Tools.Utils;

namespace Oleg_ivo.MeloManager.Prism
{
    public class MeloManagerOptions
    {
        private string mp3TagRenamePreviewFileName;

        public bool DisableMonitorFilesChanges { get; set; }
        public bool DisableWinampBinding { get; set; }

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
        public string Mp3TagRenamePreviewFileName
        {
            get
            {
                if (mp3TagRenamePreviewFileName == null)
                {
                    var mp3TagWithVersion =
                        Registry.GetValue(
                            @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Mp3tag",
                            "DisplayName", null) as string;
                    mp3TagRenamePreviewFileName =
                        Utils.FileUtils.UnwrapEnvironmentBasedPath(
                            string.Format(@"%userprofile%\AppData\Local\Temp\{0}\preview.txt", mp3TagWithVersion));
                }
                return mp3TagRenamePreviewFileName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ConnectionString { get { return Settings.Default.ConnectionString; } }

        public DateTime LastPlaylistsImportDate
        {
            get { return Settings.Default.LastPlaylistsImportDate; }
            set { Settings.Default.LastPlaylistsImportDate = value; }
        }

        public bool AutoImportPlaylistsOnStart
        {
            get { return Settings.Default.AutoImportPlaylistsOnStart; }
            set { Settings.Default.AutoImportPlaylistsOnStart = value; }
        }

        public long WinampImportCategoryId
        {
            get { return Settings.Default.WinampImportCategoryId; }
            set { Settings.Default.WinampImportCategoryId = value; }
        }

        public StringCollection Users
        {
            get { return Settings.Default.Users; }
        }

        public void Save()
        {
            Settings.Default.Save();
        }
    }
}