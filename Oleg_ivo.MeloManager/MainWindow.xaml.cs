using System;
using System.Data.Linq;
using System.Linq;
using System.Windows;
using DevExpress.Xpf.NavBar;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// 
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ExplorerBarView_Click(object sender, RoutedEventArgs e)
        {
            NavBarGroup group = navBarControl1.View.GetNavBarGroup(e);
            NavBarItem item = navBarControl1.View.GetNavBarItem(e);
            if (group != null || item != null)
            {
                //MessageBox.Show("Click - " + (item != null
                //                                  ? "Item: " + item.Content
                //                                  : "Group: " +
                //                                    group.Header));
                if (item == nbiFillTestData)
                {
                    InitDataSource();
                }
                else if(item==nbiLoadFromDb)
                {
                    LoadFromDb();
                }
                else if (item == nbiSave)
                {
                    SaveAndLoad();
                }
            }
        }

        private static MediaDataContext MediaDataContext
        {
            get { return DataProvider.DataContext; }
        }



        private void InitDataSource()
        {
            
            LoadFromDb();

            //tree.DataSource = MediaDataContext.MediaContainers.GetNewBindingList();

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

            //MediaDataContext.MediaContainers.InsertAllOnSubmit(new MediaContainer[] {category});
            SaveAndLoad();


            //var ds = new[]
            //             {
            //                 new {Name = "Плейлисты", ID = 0, ParentID = 0},
            //                    new {Name = "Плейлист1", ID = 1, ParentID = 0}, 
            //                        new {Name = "Файл11", ID = 11, ParentID = 1}, 
            //                        new {Name = "Файл12", ID = 12, ParentID = 1},
            //                    new {Name = "Плейлист2", ID = 2, ParentID = 0}, 
            //                        new {Name = "Файл21", ID = 21, ParentID = 2}, 
            //                        new {Name = "Файл22", ID = 22, ParentID = 2}
            //             };

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


            mediaTree1.DataSource = treeSource;
            //if (mediaTree1.tree.ItemsSource != MediaDataContext.MediaContainers)
            //{
            //    mediaTree1.InitDataSource(MediaDataContext.MediaContainers);
            //}
        }
    }
}
