using System.Collections.ObjectModel;
using System.Linq;
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

namespace Oleg_ivo.MeloManager.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        #region Fields
        private static Logger log = LogManager.GetCurrentClassLogger();

        private readonly IComponentContext context;

        private MediaTreeViewModel mediaTree;
        private MediaListViewModel parents;
        private MediaListViewModel childs;

        private ICommand commandLoadFromDb;
        private ICommand commandSaveAndLoad;
        private ICommand commandTreeAddCategory;
        private ICommand commandImportWinampPlaylists;
        private ICommand commandInitDataSource;

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
                }
                mediaTree = value;
                if (MediaTree != null)
                {
                    MediaTree.ParentListDataSourceChanged += MediaTree_ParentListDataSourceChanged;
                    MediaTree.ChildListDataSourceChanged += MediaTree_ChildListDataSourceChanged;
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
                    Parents.RowDoubleClick -= Parents_RowDoubleClick;
                parents = value;
                if (Parents != null)
                    Parents.RowDoubleClick += Parents_RowDoubleClick;
                RaisePropertyChanged(() => Parents);
            }
        }

        [Dependency(Required = true)]
        public MediaListViewModel Childs
        {
            get { return childs; }
            set
            {
                if (childs == value) return;
                if (Childs != null)
                    Childs.RowDoubleClick -= Childs_RowDoubleClick;
                childs = value;
                if (Childs != null)
                    Childs.RowDoubleClick += Childs_RowDoubleClick;
                RaisePropertyChanged(() => Childs);
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

        void Parents_RowDoubleClick(object sender, System.EventArgs e)
        {
            var treeWrapper = MediaTree.Items
                .Select(item => item.FindChild(Parents.SelectedItem))
                .ExcludeNull()
                .FirstOrDefault();
            MediaTree.CurrentItem = treeWrapper;
        }

        void Childs_RowDoubleClick(object sender, System.EventArgs e)
        {
            var treeWrapper = mediaTree.CurrentItem.FindChild(Childs.SelectedItem);
            MediaTree.CurrentItem = treeWrapper;
        }

        void MediaTree_ParentListDataSourceChanged(object sender, System.EventArgs e)
        {
            Parents.ListDataSource = MediaTree.ParentListDataSource;
        }

        void MediaTree_ChildListDataSourceChanged(object sender, System.EventArgs e)
        {
            Childs.ListDataSource = MediaTree.ChildListDataSource;
        }

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

            var winampCategory = new Category {Name = "Плейлисты Winamp", IsRoot = true};
            winampCategory.AddChildren(adapter.GetPlaylists());

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

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IComponentContext context)
        {
            this.context = Enforce.ArgumentNotNull(context, "context");
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