using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NLog;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.Base.WPF.ViewModels;
using Oleg_ivo.MeloManager.DependencyInjection;
using Oleg_ivo.MeloManager.MediaObjects;
using Reactive.Bindings;

namespace Oleg_ivo.MeloManager.Dialogs.SettingsEdit
{
    public class SettingsEditDialogViewModel : ViewModelBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly ReactiveCommand commandAddDirectory;
        private readonly ReactiveCommand<DirectoryViewModel> commandRemoveDirectory;

        public SettingsEditDialogViewModel(IMediaRepository dbContext)
        {
            RootCategories = dbContext.Categories.Where(category => !category.ParentContainers.Any()).ToList();
            
            Disposer.Add(commandAddDirectory = new ReactiveCommand());
            Disposer.Add(commandAddDirectory.Subscribe(_ => AddDirectory()));
            Disposer.Add(commandRemoveDirectory = new ReactiveCommand<DirectoryViewModel>());
            Disposer.Add(commandRemoveDirectory.Subscribe(RemoveDirectory));
        }

        public ICommand CommandAddDirectory
        {
            get { return commandAddDirectory; }
        }

        public ICommand CommandRemoveDirectory
        {
            get { return commandRemoveDirectory; }
        }

        private void AddDirectory()
        {
            var viewModel = new DirectoryViewModel();
            if (viewModel.ChangeDirectory())
            {
                MusicFilesSource.Add(viewModel);
            }
        }

        private void RemoveDirectory(DirectoryViewModel item)
        {
            var text = string.Format("Удалить папку [{0}] из списка?", item.Path);
            if (MessageBox.Show(text, "Удаление источника файлов", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
                MessageBoxResult.Yes)
            {
                MusicFilesSource.Remove(item);
            }
        }

        public bool AutoImportPlaylistsOnStart { get; set; }

        public bool DisableMonitorFilesChanges { get; set; }

        public bool DisableWinampBinding { get; set; }

        public long WinampImportCategoryId { get; set; }

        public IList<Category> RootCategories { get; private set; }

        public void ReadFromOptions(MeloManagerOptions options)
        {
            MusicFilesSource =
                new ObservableCollection<DirectoryViewModel>(
                    options.MusicFilesSource.Split(';').Select(path => new DirectoryViewModel {Path = path}));
            AutoImportPlaylistsOnStart = options.AutoImportPlaylistsOnStart;
            DisableMonitorFilesChanges = options.DisableMonitorFilesChanges;
            DisableWinampBinding = options.DisableWinampBinding;
            WinampImportCategoryId = options.WinampImportCategoryId;
        }

        public ObservableCollection<DirectoryViewModel> MusicFilesSource { get; set; }

        public void WriteToOptions(MeloManagerOptions options)
        {
            options.MusicFilesSource = MusicFilesSource.Select(item=>item.Path).JoinToString(";");
            options.AutoImportPlaylistsOnStart = AutoImportPlaylistsOnStart;
            options.DisableMonitorFilesChanges = DisableMonitorFilesChanges;
            options.DisableWinampBinding = DisableWinampBinding;
            options.WinampImportCategoryId = WinampImportCategoryId;
        }
    }
}
