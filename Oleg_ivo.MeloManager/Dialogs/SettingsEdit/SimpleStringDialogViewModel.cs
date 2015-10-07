using System.Collections.Generic;
using System.Linq;
using NLog;
using Oleg_ivo.Base.WPF.ViewModels;
using Oleg_ivo.MeloManager.DependencyInjection;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.Dialogs.SettingsEdit
{
    public class SettingsEditDialogViewModel : ViewModelBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public SettingsEditDialogViewModel(IMediaRepository dbContext)
        {
            RootCategories = dbContext.Categories.Where(category => !category.ParentContainers.Any()).ToList();
        }

        public bool AutoImportPlaylistsOnStart { get; set; }

        public bool DisableMonitorFilesChanges { get; set; }

        public bool DisableWinampBinding { get; set; }

        public long WinampImportCategoryId { get; set; }

        public IList<Category> RootCategories { get; private set; }

        public void ReadFromOptions(MeloManagerOptions options)
        {
            AutoImportPlaylistsOnStart = options.AutoImportPlaylistsOnStart;
            DisableMonitorFilesChanges = options.DisableMonitorFilesChanges;
            DisableWinampBinding = options.DisableWinampBinding;
            WinampImportCategoryId = options.WinampImportCategoryId;
        }

        public void WriteToOptions(MeloManagerOptions options)
        {
            options.AutoImportPlaylistsOnStart = AutoImportPlaylistsOnStart;
            options.DisableMonitorFilesChanges = DisableMonitorFilesChanges;
            options.DisableWinampBinding = DisableWinampBinding;
            options.WinampImportCategoryId = WinampImportCategoryId;
        }
    }
}
