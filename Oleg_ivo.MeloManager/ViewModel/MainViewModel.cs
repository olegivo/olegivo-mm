using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Autofac;
using NLog;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.Base.WPF.Dialogs;
using Oleg_ivo.Base.WPF.Extensions;
using Oleg_ivo.Base.WPF.ViewModels;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.MediaObjects.Extensions;
using Oleg_ivo.MeloManager.Prism;
using Oleg_ivo.MeloManager.Winamp;
using Oleg_ivo.MeloManager.Winamp.Tracking;
using Oleg_ivo.Tools.Utils;
using Reactive.Bindings;

namespace Oleg_ivo.MeloManager.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IComponentContext context;
        private readonly WinampControl winampControl;
        private readonly WinampFilesMonitor winampFilesMonitor;
        private readonly MeloManagerOptions options;
        private readonly Window mainWindow;

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

                    InitCommands();
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
        public MediaDbContext DbContext
        {
            get { return MediaTree.DbContext; }
            set { MediaTree.DbContext = value; }
        }

        [Dependency(Required = true)]
        public TrackingViewModel Tracking { get; set; }

        [Dependency]
        public IModalDialogService ModalDialogService { get; set; }

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
            CanWorkWithDataContext = new ReactiveProperty<bool>();
            CommandLoadFromDb = new ReactiveCommand(CanWorkWithDataContext).AddHandler(() => LoadFromDb());
            CommandSaveAndLoad = new ReactiveCommand(CanWorkWithDataContext).AddHandler(SaveAndLoad);
            CommandImportWinampPlaylists = new ReactiveCommand(CanWorkWithDataContext).AddHandler(() => ImportWinampPlaylists());

            CommandTreeAddCategory = MediaTree.CommandAddCategoryToCurrent.AddHandler(TreeAddCategory);
            CommandTreeAddPlaylist = MediaTree.CommandAddPlaylistToCurrent.AddHandler(TreeAddPlaylist);
            CommandTreeAddMediaFile = MediaTree.CommandAddMediaFileToCurrent.AddHandler(TreeAddMediaFile);
            CommandTreeDeleteCurrent = MediaTree.CommandDeleteCurrent.AddHandler(TreeDeleteCurrent);

            CommandTrayDoubleClick = new ReactiveCommand().AddHandler(SwitchHideShow);

            CommandInitDataSource = new ReactiveCommand(CanWorkWithDataContext).AddHandler(InitDataSource);
            CommandTest = new ReactiveCommand().AddHandler(Test);

            Disposer.Add(CanWorkWithDataContext);
            Disposer.Add(CanWorkWithDataContext.Subscribe(value => log.Debug("CanWorkWithDataContext = {0}", value)));
        }

        public ReactiveProperty<bool> CanWorkWithDataContext { get; set; }
        
        private void SwitchHideShow()
        {
            if (mainWindow.WindowState == WindowState.Minimized)
            {
                mainWindow.WindowState = WindowState.Maximized;
                mainWindow.Show();
                mainWindow.Activate();
            }
            if (!mainWindow.IsActive)
            {
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

        public ICommand CommandTreeDeleteCurrent { get; private set; }
        
        public ICommand CommandTreeAddCategory { get; private set; }

        public ICommand CommandTreeAddPlaylist { get; private set; }

        public ICommand CommandTreeAddMediaFile { get; private set; }

        public ICommand CommandImportWinampPlaylists { get; private set; }

        public ICommand CommandInitDataSource { get; private set; }

        public ICommand CommandTrayDoubleClick { get; private set; }

        #endregion

        #region Methods

        private Task LoadFromDb()
        {
            //Clear();
            log.Info(StatusText = "Загрузка из БД в процессе...");
            CanWorkWithDataContext.Value = false;
            MediaTree.Items = new ObservableCollection<MediaContainerTreeWrapper>();
            var task = Task.Factory.StartNew(
                () =>
                {
                    //DataContext.ActionWithLog(dataContext => dataContext.RefreshCache());
                    MediaTree.InitSource(
                        DbContext.Categories.Where(category => category.IsRoot));
                    log.Info(StatusText = "Загрузка из БД завершена");
                    CanWorkWithDataContext.Value = true;
                });
            /*foreach (var mc in categories)
            {
                MediaTree.AddMediaContainer(mc, null);
            }*/
            log.Info(StatusText = "Загрузка из БД в процессе...");
            return task;
        }

        private void SaveAndLoad()
        {
            Save();
            LoadFromDb();
        }

        private void Save()
        {
            log.Info(StatusText = "Сохранение в процесе...");
            CanWorkWithDataContext.Value = false;
            //var changeSet = DataContext.GetChangeSet();
            DbContext.SubmitChangesWithLog(log.Debug);
            //DataContext.SubmitChanges();
            log.Info(StatusText = "Сохранение завершено");
            CanWorkWithDataContext.Value = true;
        }

        private void TreeDeleteCurrent()
        {
            StatusText = "Удаление текущего элемента";
        }

        private void TreeAddCategory()
        {
            StatusText = "Добавить категорию";
        }

        private void TreeAddPlaylist()
        {
            StatusText = "Добавить плейлист";
        }

        private void TreeAddMediaFile()
        {
            StatusText = "Добавить медиа-файл";
        }

        /// <summary>
        /// Импорт плейлистов
        /// </summary>
        /// <param name="onlyChanged">Только изменённые с последнего импорта</param>
        private void ImportWinampPlaylists(bool onlyChanged = false)
        {
            Task.Factory.StartNew(Save)
                .ContinueWith(task =>
                {
                    CanWorkWithDataContext.Value = false;
                    StatusText = "Импорт плейлистов из Winamp в процессе...";
                    var root = MediaTree.Items.FirstOrDefault(item => item.UnderlyingItem.Id == options.WinampImportCategoryId)
                               ?? (MediaTree.CurrentItem != null
                                   ? MediaTree.CurrentItem.ParentsRecursive.LastOrDefault()
                                   : null)
                               ?? MediaTree.Items.FirstOrDefault();
                    var createNew = root == null;
                    if (!createNew && root.UnderlyingItem.Id != options.WinampImportCategoryId)
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
                                return false;
                        }
                    }
                    var winampCategory = createNew
                        ? new Category {Name = "Плейлисты Winamp", IsRoot = true}
                        : (Category) root.UnderlyingItem;

                    var imported = winampFilesMonitor.RunImport(winampCategory, onlyChanged);

                    if (options.WinampImportCategoryId != winampCategory.Id)
                        options.WinampImportCategoryId = winampCategory.Id;
                    return imported;
                })
                .ContinueWith(task =>
                {
                    CanWorkWithDataContext.Value = true;
                    if(task.Result) LoadFromDb();
                })
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


            MediaTree.AddMediaContainer(c1, null);
            MediaTree.AddMediaContainer(c2, null);
            c1.AddChild(c2);
            MediaTree.AddMediaContainer(c3, null);
        }

        private void Test()
        {
            //TestMethods.TestDialogService(ModalDialogService);
            //TestMethods.TestWinampTrackingWindow(context);
            //winampControl.LoadPlaylist(@"f:\Subversion\MM\Oleg_ivo.MeloManager\bin\Debug\playlist.m3u");
            //TestMethods.TestInsertMediaFileWithFile(DataContext);
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
        public MainViewModel(IComponentContext context, WinampControl winampControl, WinampFilesMonitor winampFilesMonitor, MeloManagerOptions options)//TODO: сделать для классов Winamp фасад
        {
            this.context = Enforce.ArgumentNotNull(context, "context");
            this.winampControl = Enforce.ArgumentNotNull(winampControl, "winampControl");
            this.winampFilesMonitor = Enforce.ArgumentNotNull(winampFilesMonitor, "winampFilesMonitor");
            this.options = Enforce.ArgumentNotNull(options, "options");

            Disposer.Add(winampControl);
            Disposer.Add(winampFilesMonitor);

            //InitializeComponents();
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real"
                mainWindow = Application.Current.MainWindow;
            }
        }

        private Task RunServices()
        {
            var task
                = Task.Factory.StartNew(
                    () =>
                    {
                        log.Info(StatusText = "Запуск служб в процессе...");
                        
                        log.Info("WinampBinding: {0}", !options.DisableWinampBinding ? "on" : "off");
                        if (!options.DisableWinampBinding) winampControl.LaunchBind();
                        
                        log.Info("AutoImportPlaylistsOnStart: {0}", options.AutoImportPlaylistsOnStart ? "on" : "off");
                        if (options.AutoImportPlaylistsOnStart) ImportWinampPlaylists(true);

                        log.Info("DisableMonitorFilesChanges: {0}", !options.DisableMonitorFilesChanges ? "on" : "off");
                        if (!options.DisableMonitorFilesChanges) winampFilesMonitor.MonitorFilesChanges();
                    })
                    .ContinueWith(t => log.Info(StatusText = "Запуск служб завершён"));
            return task;
        }

        public void Init()
        {
            log.Info(StatusText = "Инициализация в процессе...");
            Task.Factory.StartNew(() => { })
                .ContinueWith(task => DbContext.RefreshCache())
                .ContinueWith(task => LoadFromDb())
                .ContinueWith(task => RunServices())
                .ContinueWith(task => log.Info(StatusText = "Инициализация завершена"));
        }

    }
}