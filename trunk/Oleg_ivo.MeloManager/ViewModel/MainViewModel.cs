using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Autofac;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NLog;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.PlaylistFileAdapters;
using Oleg_ivo.MeloManager.Winamp;

namespace Oleg_ivo.MeloManager.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly WinampControl winampControl;

        #region Fields
        private static Logger log = LogManager.GetCurrentClassLogger();

        private readonly IComponentContext context;

        private MediaTreeViewModel mediaTree;
        private MediaListViewModel parents;
        private MediaListViewModel children;

        private ICommand commandLoadFromDb;
        private ICommand commandSaveAndLoad;
        private ICommand commandTreeAddCategory;
        private ICommand commandImportWinampPlaylists;
        private ICommand commandInitDataSource;
        private ICommand commandTest;

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
        void Parents_RowDoubleClick(object sender, System.EventArgs e)
        {
            GoToParent(Parents.SelectedItem);
        }

        void Children_RowDoubleClick(object sender, System.EventArgs e)
        {
            GoToChild(Children.SelectedItem);
        }

        void MediaTree_ParentListDataSourceChanged(object sender, System.EventArgs e)
        {
            Parents.ListDataSource = MediaTree.ParentListDataSource;//TODO: binding (multybinding or data trigger?)
        }

        void MediaTree_ChildListDataSourceChanged(object sender, System.EventArgs e)
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
        public ICommand CommandLoadFromDb
        {
            get
            {
                return commandLoadFromDb ??
                       (commandLoadFromDb = new RelayCommand(LoadFromDb));
            }
        }

        public ICommand CommandSaveAndLoad
        {
            get
            {
                return commandSaveAndLoad ??
                       (commandSaveAndLoad = new RelayCommand(SaveAndLoad));
            }
        }

        public ICommand CommandTreeAddCategory
        {
            get
            {
                return commandTreeAddCategory ??
                       (commandTreeAddCategory = new RelayCommand(TreeAddCategory));
            }
        }

        public ICommand CommandImportWinampPlaylists
        {
            get
            {
                return commandImportWinampPlaylists ??
                       (commandImportWinampPlaylists = new RelayCommand(ImportWinampPlaylists));
            }
        }

        public ICommand CommandInitDataSource
        {
            get
            {
                return commandInitDataSource ??
                       (commandInitDataSource = new RelayCommand(InitDataSource));
            }
        }

        public ICommand CommandTest
        {
            get
            {
                return commandTest ??
                       (commandTest = new RelayCommand(Test));
            }
        }

        #endregion

        #region Methods

        public void LoadFromDb()
        {
            //Clear();
            log.Info(StatusText = "Загрузка из БД");
            MediaTree.Items = new ObservableCollection<MediaContainerTreeWrapper>();
            var categories = DataContext.MediaContainers
                .Where(mc => mc is Category && mc.IsRoot)
                .Cast<Category>();
            MediaTree.InitSource(categories);
            /*foreach (var mc in categories)
            {
                MediaTree.AddCategory(mc, null);
            }*/
            log.Info(StatusText = "Загрузка из БД завершена");
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
            StatusText = "Импорт плейлистов из Winamp";
            var adapter = context.ResolveUnregistered<WinampM3UPlaylistFileAdapter>();

            var winampCategory = new Category { Name = "Плейлисты Winamp", IsRoot = true };
            winampCategory.AddChildren(adapter.GetPlaylists());//TODO:обновление плейлистов

            MediaTree.AddCategory(winampCategory, null);
            DataContext.MediaContainers.InsertOnSubmit(winampCategory);
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
            winampControl.LoadPlaylist(@"f:\Subversion\MM\Oleg_ivo.MeloManager\bin\Debug\playlist.m3u");
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
        public MainViewModel(IComponentContext context, WinampControl winampControl)
        {
            this.winampControl = winampControl;
            this.context = Enforce.ArgumentNotNull(context, "context");
            winampControl.LaunchBind();
            //InitializeComponents();
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }
    }
}