using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using Oleg_ivo.Base.WPF.ViewModels;
using Reactive.Bindings;

namespace Oleg_ivo.MeloManager.Dialogs.SettingsEdit
{
    public class DirectoryViewModel : ViewModelBase
    {
        private string path;
        private readonly ReactiveCommand<string> commandChangeDirectory;

        public string Path
        {
            get { return path; }
            set
            {
                if(path == value) return;
                path = value;
                RaisePropertyChanged("Path");
            }
        }

        public DirectoryViewModel()
        {
            Disposer.Add(commandChangeDirectory = new ReactiveCommand<string>());
            Disposer.Add(commandChangeDirectory.Subscribe(_ => ChangeDirectory()));
        }

        public ICommand CommandChangeDirectory
        {
            get { return commandChangeDirectory; }
        }

        public bool ChangeDirectory()
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = "Выбор папки",
                IsFolderPicker = true,
                InitialDirectory = Path,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = Path,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            var result = dlg.ShowDialog(Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive)) == CommonFileDialogResult.Ok;
            if (result)
            {
                Path = dlg.FileName;
            }

            return result;
        }

    }
}