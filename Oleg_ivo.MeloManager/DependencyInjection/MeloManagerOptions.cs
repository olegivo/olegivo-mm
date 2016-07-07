using System;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Win32;
using NLog;
using Oleg_ivo.MeloManager.Properties;
using Oleg_ivo.Tools.Utils;
using Reactive.Bindings;

namespace Oleg_ivo.MeloManager.DependencyInjection
{
    public class MeloManagerOptions : IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private string mp3TagRenamePreviewFileName;

        private readonly CompositeDisposable disposer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public MeloManagerOptions()
        {
            disposer = new CompositeDisposable(
                DisableMonitorFilesChangesProperty = new ReactiveProperty<bool>(),
                DisableMonitorFilesChangesProperty.DistinctUntilChanged().Subscribe(b => log.Info("MonitorFilesChanges: {0}", !b ? "on" : "off")),
                DisableWinampBindingProperty = new ReactiveProperty<bool>(),
                DisableWinampBindingProperty.DistinctUntilChanged().Subscribe(b => log.Info("WinampBinding: {0}", !b ? "on" : "off")));
        }

        public ReactiveProperty<bool> DisableMonitorFilesChangesProperty { get; private set; }

        public bool DisableMonitorFilesChanges
        {
            get { return DisableMonitorFilesChangesProperty.Value; }
            set { DisableMonitorFilesChangesProperty.Value = value; }
        }

        public ReactiveProperty<bool> DisableWinampBindingProperty { get; private set; }

        public bool DisableWinampBinding
        {
            get { return DisableWinampBindingProperty.Value; }
            set { DisableWinampBindingProperty.Value = value; }
        }

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
        public string MusicFilesSource
        {
            get { return Settings.Default.MusicFilesSource; }
            set { Settings.Default.MusicFilesSource = value; }
        }

        /// <summary>
        /// Режим автоматической починки. Для данного режима следует указывать также параметры <see cref="PlaylistsPath"/> и <see cref="MusicFilesSource"/>
        /// </summary>
        public bool RepairMode { get; set; }

        /// <summary>
        /// Режим переименования
        /// </summary>
        public bool Mp3TagRenameMode { get; set; }

        /// <summary>
        /// Режим сканирования медиа-файлов и обновления IDV3-тегов
        /// </summary>
        public bool Id3Mode { get; set; }

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
            set
            {
                if(Settings.Default.AutoImportPlaylistsOnStart == value) return;
                Settings.Default.AutoImportPlaylistsOnStart = value;
                log.Info("AutoImportPlaylistsOnStart: {0}", value ? "on" : "off");
            }
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            disposer.Dispose();
        }
    }
}