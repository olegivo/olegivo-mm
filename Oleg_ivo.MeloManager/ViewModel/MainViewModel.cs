using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Autofac;
using Codeplex.Reactive;
using GalaSoft.MvvmLight;
using NLog;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.MeloManager.Extensions;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.PlaylistFileAdapters;
using Oleg_ivo.MeloManager.Winamp;
using Oleg_ivo.MeloManager.Winamp.Tracking;

namespace Oleg_ivo.MeloManager.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class MainViewModel : ViewModelBase, IDisposable/*TODO: IDisposable реализовать в промежуточном базовом классе (+virtual)*/
    {
        #region Fields
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IComponentContext context;
        private readonly WinampControl winampControl;
        private readonly WinampFilesMonitor winampFilesMonitor;

        private MediaTreeViewModel mediaTree;
        private MediaListViewModel parents;
        private MediaListViewModel children;

        private string statusText;

        #endregion

        #region Properties
        [Dependency(Required = true)]
        public MediaTreeViewModel MediaTree
        {
            get { return mediaTree; }
            set
            {
                if (mediaTree == value) return;
                if (MediaTree != null)
                {
                    MediaTree.ParentListDataSourceChanged -= MediaTree_ParentListDataSourceChanged;
                    MediaTree.ChildListDataSourceChanged -= MediaTree_ChildListDataSourceChanged;
                    MediaTree.Deleting -= MediaTree_Deleting;
                }
                mediaTree = value;
                if (MediaTree != null)
                {
                    MediaTree.ParentListDataSourceChanged += MediaTree_ParentListDataSourceChanged;
                    MediaTree.ChildListDataSourceChanged += MediaTree_ChildListDataSourceChanged;
                    MediaTree.Deleting += MediaTree_Deleting;
                }
                RaisePropertyChanged(() => MediaTree);
            }
        }

        [Dependency(Required = true)]
        public MediaListViewModel Parents
        {
            get { return parents; }
            set
            {
                if (parents == value) return;
                if (Parents != null)
                {
                    Parents.RowDoubleClick -= Parents_RowDoubleClick;
                    Parents.Deleting -= Parents_Deleting;
                }
                parents = value;
                if (Parents != null)
                {
                    Parents.RowDoubleClick += Parents_RowDoubleClick;
                    Parents.Deleting += Parents_Deleting;
                }
                RaisePropertyChanged(() => Parents);
            }
        }

        [Dependency(Required = true)]
        public MediaListViewModel Children
        {
            get { return children; }
            set
            {
                if (children == value) return;
                if (Children != null)
                {
                    Children.RowDoubleClick -= Children_RowDoubleClick;
                    Children.Deleting -= Children_Deleting;
                }
                children = value;
                if (Children != null)
                {
                    Children.RowDoubleClick += Children_RowDoubleClick;
                    Children.Deleting += Children_Deleting;
                }
                RaisePropertyChanged(() => Children);
            }
        }

        [Dependency(Required = true)]
        public MediaDataContext DataContext
        {
            get { return MediaTree.DataContext; }
            set { MediaTree.DataContext = value; }
        }

        [Dependency(Required = true)]
        public TrackingViewModel Tracking { get; set; }


        /// <summary>
        /// Статусная строка
        /// </summary>
        public string StatusText
        {
            get { return statusText; }
            set
            {
                if (statusText == value) return;
                statusText = value;
                RaisePropertyChanged(() => StatusText);
            }
        }

        #endregion

        #region Event handlers
        void Parents_RowDoubleClick(object sender, EventArgs e)
        {
            GoToParent(Parents.SelectedItem);
        }

        void Children_RowDoubleClick(object sender, EventArgs e)
        {
            GoToChild(Children.SelectedItem);
        }

        void MediaTree_ParentListDataSourceChanged(object sender, EventArgs e)
        {
            Parents.ListDataSource = MediaTree.ParentListDataSource;//TODO: binding (multybinding or data trigger?)
        }

        void MediaTree_ChildListDataSourceChanged(object sender, EventArgs e)
        {
            Children.ListDataSource = MediaTree.ChildListDataSource;//TODO: binding (multybinding or data trigger?)
        }

        void Parents_Deleting(object sender, DeletingEventArgs<List<MediaContainer>> e)
        {
            e.Cancel = !DeleteParents(e.Deleting);
        }

        void Children_Deleting(object sender, DeletingEventArgs<List<MediaContainer>> e)
        {
            e.Cancel = !DeleteChildren(e.Deleting);
        }

        void MediaTree_Deleting(object sender, DeletingEventArgs<List<MediaContainerTreeWrapper>> e)
        {
            e.Cancel =
                MessageBox.Show(
                    string.Format("Будут удалены следующие элементы:\n{0}",
                        e.Deleting.JoinToString(",\n")),
                    "Удаление",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.No;
        }

        #endregion

        #region Commands

        private void InitCommands()
        {
            CommandTest = new ReactiveCommand().AddHandler(Test);
            CommandLoadFromDb = new ReactiveCommand().AddHandler(LoadFromDb);
            CommandSaveAndLoad = new ReactiveCommand().AddHandler(SaveAndLoad);
            CommandTreeAddCategory = new ReactiveCommand().AddHandler(TreeAddCategory);
            CommandImportWinampPlaylists = new ReactiveCommand().AddHandler(ImportWinampPlaylists);
            CommandInitDataSource = new ReactiveCommand().AddHandler(InitDataSource);
            CommandTrayDoubleClick = new ReactiveCommand().AddHandler(SwitchHideShow);
        }

        private void SwitchHideShow()
        {
            if (mainWindow.WindowState == WindowState.Minimized)
            {
                mainWindow.WindowState = WindowState.Maximized;
                mainWindow.Show();
                mainWindow.Activate();
            }
            else
            {
                mainWindow.WindowState = WindowState.Minimized;
            }
        }

        public ICommand CommandTest { get; private set; }
        
        public ICommand CommandLoadFromDb { get; private set; }

        public ICommand CommandSaveAndLoad { get; private set; }

        public ICommand CommandTreeAddCategory { get; private set; }

        public ICommand CommandImportWinampPlaylists { get; private set; }

        public ICommand CommandInitDataSource { get; private set; }

        public ICommand CommandTrayDoubleClick { get; private set; }

        #endregion

        #region Methods

        public void LoadFromDb()
        {
            //Clear();
            log.Info(StatusText = "Загрузка из БД");
            MediaTree.Items = new ObservableCollection<MediaContainerTreeWrapper>();
            Task.Factory.StartNew(
                () =>
                {
                    //DataContext.ActionWithLog(dataContext => dataContext.RefreshCache());
                    MediaTree.InitSource(
                        DataContext.MediaContainers.Where(mc => mc is Category && mc.IsRoot).Cast<Category>());
                    log.Info(StatusText = "Загрузка из БД завершена");
                });
            /*foreach (var mc in categories)
            {
                MediaTree.AddCategory(mc, null);
            }*/
            log.Info(StatusText = "Загрузка из БД в процессе...");
        }

        private void SaveAndLoad()
        {
            log.Info(StatusText = "Сохранение");
            //var changeSet = DataContext.GetChangeSet();
            DataContext.SubmitChanges();
            log.Info(StatusText = "Сохранение завершено");
            LoadFromDb();
        }

        private void TreeAddCategory()
        {
            StatusText = "Добавить категорию";//TODO: Добавить категорию
        }

        public void ImportWinampPlaylists()
        {
            Task.Factory.StartNew(() =>
            {
                log.Info(StatusText = "Сохранение в процессе...");
                //var changeSet = DataContext.GetChangeSet();
                DataContext.SubmitChanges();
            })
                .ContinueWith(task =>
                {
                    StatusText = "Импорт плейлистов из Winamp в процессе...";
                    var root = (MediaTree.CurrentItem != null
                        ? MediaTree.CurrentItem.ParentsRecursive.LastOrDefault()
                        : null) ??
                               MediaTree.Items.FirstOrDefault();
                    var createNew = root == null;
                    if (!createNew)
                    {
                        var messageBoxText =
                            string.Format(
                                "Импортировать в существующую категорию {0}?\nЕсли нет, будет создана новая категория",
                                root.Name);
                        var result = MessageBox.Show(messageBoxText, "Импорт плейлистов", MessageBoxButton.YesNoCancel);
                        switch (result)
                        {
                            case MessageBoxResult.No:
                                createNew = true;
                                break;
                            case MessageBoxResult.Cancel:
                                return;
                        }
                    }
                    var winampCategory = createNew
                        ? new Category {Name = "Плейлисты Winamp", IsRoot = true}
                        : root.UnderlyingItem as Category;
                    winampFilesMonitor.RunImportAll(winampCategory);
                })
                .ContinueWith(task => LoadFromDb())
                .ContinueWith(task => StatusText = "Импорт плейлистов из Winamp завершён");
        }

        private void InitDataSource()
        {
            StatusText = "Тестовая инициализация";
            var f1 = new MediaFile { Name = "Файл 1" };
            var f2 = new MediaFile { Name = "Файл 2" };
            var f3 = new MediaFile { Name = "Файл 3" };
            var f4 = new MediaFile { Name = "Файл 4" };
            var f5 = new MediaFile { Name = "Файл 5" };

            var p1 = new Playlist { Name = "Плейлист 1" };
            p1.AddChildMediaFile(f1);
            p1.AddChildMediaFile(f2);
            var p2 = new Playlist { Name = "Плейлист 2" };
            p2.AddChildMediaFile(f2);
            p2.AddChildMediaFile(f3);
            var p3 = new Playlist { Name = "Плейлист 3" };
            p3.AddChildMediaFile(f3);
            p3.AddChildMediaFile(f4);
            p3.AddChildMediaFile(f5);

            var c1 = new Category { Name = "Категория 1", IsRoot = true };
            c1.AddChild(p1);
            var c2 = new Category { Name = "Категория 2", IsRoot = true };
            c2.AddChild(p2);
            var c3 = new Category { Name = "Категория 3", IsRoot = true };
            c3.AddChild(p2);
            c3.AddChild(p3);


            MediaTree.AddCategory(c1, null);
            MediaTree.AddCategory(c2, null);
            c1.AddChild(c2);
            MediaTree.AddCategory(c3, null);
        }

        private void Test()
        {
            var winampTrackingWindow = context.ResolveUnregistered<WinampTrackingWindow>();
            winampTrackingWindow.ShowDialog();
            //winampControl.LoadPlaylist(@"f:\Subversion\MM\Oleg_ivo.MeloManager\bin\Debug\playlist.m3u");
            /*var mediaContainer = MediaTree.Items.First().UnderlyingItem;
            var playlistsPath = context.Resolve<MeloManagerOptions>().PlaylistsPath;
            var adapter = context.ResolveUnregistered<WinampM3UPlaylistFileAdapter>();
            foreach (
                var playlist in
                    mediaContainer.Children.Cast<Playlist>().ToList()
                        .Where(p => !p.MediaContainerFiles.Any()))
            {
                var originalFileName = System.IO.File.Exists(playlist.OriginalFileName)
                    ? playlist.OriginalFileName
                    : System.IO.Path.Combine(playlistsPath, adapter.Dic.FirstOrDefault(pair => pair.Value == playlist.Name).Key);
                playlist.MediaContainerFiles.Add(new MediaContainerFile { File = File.GetFile(originalFileName) });
            }*/

            /*var fullFilename = @"D:\Music\Disk\Music\9\Sixpence None the Richer - Kiss me.mp3";
            var fileName = System.IO.Path.GetFileName(fullFilename);
            var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fullFilename);
            var extension = System.IO.Path.GetExtension(fullFilename);
            var drive = System.IO.Path.GetPathRoot(fullFilename);
            var path = System.IO.Path.GetDirectoryName(fullFilename);
            var file = new File
            {
                FullFileName = fullFilename,
                Drive = drive,
                Path = path,
                Filename = fileName,
                FileNameWithoutExtension = fileNameWithoutExtension,
                Extention = extension
            };
            var mediaFile = new MediaFile { Name = "Test" };
            var mediaContainerFile = new MediaContainerFile { MediaContainer = mediaFile };
            file.MediaContainerFiles.Add(mediaContainerFile);

            DataContext.MediaContainerFiles.InsertOnSubmit(mediaContainerFile);
            DataContext.MediaContainers.InsertOnSubmit(mediaFile);
            DataContext.Files.InsertOnSubmit(file);

            DataContext.SubmitChangesWithLog();
            return;*/
        }

        private void GoToParent(MediaContainer parent)
        {
            var treeWrapper = MediaTree.Items
                .Select(item => item.UnderlyingItem == parent ? item : item.FindChild(parent))
                .ExcludeNull()
                .FirstOrDefault();
            if (treeWrapper != null)
            {
                MediaTree.NameFilter = null;
                MediaTree.CurrentItem = treeWrapper;
            }
            //TODO: GoHistory
        }

        private void GoToChild(MediaContainer child)
        {
            var treeWrapper = mediaTree.CurrentItem.FindChild(child);
            if (treeWrapper != null)
            {
                MediaTree.NameFilter = null;
                MediaTree.CurrentItem = treeWrapper;
            }
            //TODO: GoHistory
        }

        private bool DeleteChildren(List<MediaContainer> deletingItems)
        {
            var needDelete = MessageBox.Show(
                string.Format("Из {0} будут удалены:\n{1}", MediaTree.CurrentTreeMediaContainer,
                    deletingItems.JoinToString(",\n")),
                "Удаление",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;
            if (needDelete)
            {
                var list = deletingItems.Select(mc => MediaTree.CurrentItem.FindChild(mc, MediaTree.CurrentItem)).ToList();
                foreach (var wrapper in list)
                {
                    MediaTree.DeleteItem(wrapper, true);
                }
            }
            return needDelete;
        }

        private bool DeleteParents(List<MediaContainer> deletingItems)
        {
            var needDelete = MessageBox.Show(
                string.Format("{0} будет удален из:\n{1}", MediaTree.CurrentTreeMediaContainer,
                    deletingItems.JoinToString(",\n")),
                "Удаление",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;

            if (needDelete)
            {
                var comparer = new MediaContainerTreeWrapper.MediaContainerTreeWrapperByUnderlyingItemComparer();
                var list =
                    deletingItems.SelectMany(
                        mc => MediaTree.Items.SelectMany(w => w.FindChildren(MediaTree.CurrentItem.UnderlyingItem, mc)))
                        .Distinct(comparer)
                        .ToList();
                foreach (var wrapper in list)
                {
                    MediaTree.DeleteItem(wrapper, true);
                }

            }

            return needDelete;
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IComponentContext context, WinampControl winampControl, WinampFilesMonitor winampFilesMonitor)//TODO: сделать для классов Winamp фасад
        {
            this.context = Enforce.ArgumentNotNull(context, "context");
            this.winampControl = Enforce.ArgumentNotNull(winampControl, "winampControl");
            this.winampFilesMonitor = Enforce.ArgumentNotNull(winampFilesMonitor, "winampFilesMonitor");
            //InitializeComponents();
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real"
                mainWindow = Application.Current.MainWindow;
                RunServices();
                InitCommands();
            }
        }

        private void RunServices()
        {
            winampControl.LaunchBind();
            winampFilesMonitor.MonitorFilesChanges();
        }

        #region IDisposable
        private bool isDisposed;
        private readonly Window mainWindow;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed) return;

            winampControl.Dispose();
            winampFilesMonitor.Dispose();

            isDisposed = true;
        }

        #endregion
    }
}