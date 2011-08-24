using System;
using System.Data.Linq;
using System.Linq;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.ViewModel;

namespace Oleg_ivo.MeloManager
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        /// <summary>
        /// 
        /// </summary>
        public MainView()
        {
            InitializeComponent();
        }

        MainViewModel vm
        {
            get { return DataContext as MainViewModel; }
        }

        private static MediaDataContext MediaDataContext
        {
            get { return DataProvider.DataContext; }
        }



        private void InitDataSource()
        {
            
            LoadFromDb();

            Category category1 = MediaDataContext.CreateCategory();
            category1.Name = "Категория1";
            Category category2 = MediaDataContext.CreateCategory();
            category2.Name = "Категория2";

            MediaFile mediaFile1 = new MediaFile {Name = "File1"};
            MediaFile mediaFile2 = new MediaFile {Name = "File2"};
            MediaFile mediaFile3 = new MediaFile {Name = "File3"};
            MediaFile mediaFile4 = new MediaFile {Name = "File4"};
            MediaFile mediaFile5 = new MediaFile {Name = "File5"};
            MediaFile mediaFile6 = new MediaFile {Name = "File6"};
            MediaFile mediaFile7 = new MediaFile {Name = "File7"};
            MediaFile mediaFile8 = new MediaFile {Name = "File8"};

            Playlist playlist1 = new Playlist { Name = "Плейлист1" };
            Playlist playlist2 = new Playlist { Name = "Плейлист2" };
            Playlist playlist3 = new Playlist { Name = "Плейлист3" };
            Playlist playlist4 = new Playlist { Name = "Плейлист4" };

            playlist1.AddChildMediaFile(mediaFile1);
            playlist1.AddChildMediaFile(mediaFile2);
            playlist2.AddChildMediaFile(mediaFile3);
            playlist2.AddChildMediaFile(mediaFile4);
            playlist3.AddChildMediaFile(mediaFile5);
            playlist3.AddChildMediaFile(mediaFile6);
            playlist4.AddChildMediaFile(mediaFile7);
            playlist4.AddChildMediaFile(mediaFile8);

            category1.AddChild(playlist1);
            //category1.RemoveChild(playlist1);
            category1.AddChild(playlist2);
            category2.AddChild(playlist2);
            category2.AddChild(playlist3);
            category2.AddChild(playlist4);

            SaveAndLoad();

        }

        private void SaveAndLoad()
        {
            try
            {
                //ChangeSet changeSet = MediaDataContext.GetChangeSet();
                MediaDataContext.SubmitChanges();
                LoadFromDb();
            }
            catch (Exception ex)
            {
                throw new Exception("", ex);
            }
        }

        private void LoadFromDb()
        {
            MediaDataContext.Refresh(RefreshMode.OverwriteCurrentValues);
            MediaContainerTreeSource treeSource = new MediaContainerTreeSource();
            treeSource.MediaDataContext = DataProvider.DataContext;
            foreach (var category in MediaDataContext.MediaContainers
                                        .AsEnumerable()
                                            .OfType<Category>()
                                                .Where(c => c != null && c.ParentCategory == null))
            {
                treeSource.AddCategory(category);
            }

            vm.TreeDataSource = treeSource;
            //if (mediaTree1.tree.ItemsSource != MediaDataContext.MediaContainers)
            //{
            //    mediaTree1.InitDataSource(MediaDataContext.MediaContainers);
            //}
        }

        private void bbiInit_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            InitDataSource();
        }

        private void bbiLoad_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            LoadFromDb();
        }

        private void bbiSave_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            SaveAndLoad();
        }

        private void mediaTree1_FocusedRowChanged(object sender, DevExpress.Xpf.Grid.FocusedRowChangedEventArgs e)
        {
            MediaContainerTreeWrapper mediaContainerTreeWrapper = e.NewRow as MediaContainerTreeWrapper;
            if (mediaContainerTreeWrapper != null)
            {
                MediaContainer mediaContainer = mediaContainerTreeWrapper.UnderlyingItem;
                vm.CurrentTreeMediaContainer = mediaContainer;
                if(mediaContainer!=null)
                {
                    mediaListChilds.DataSource = mediaContainer.ChildMediaContainers;
                    mediaListParents.DataSource = mediaContainer.ParentMediaContainers;
                }
            }

            //e.NewRow
        }
    }
}
