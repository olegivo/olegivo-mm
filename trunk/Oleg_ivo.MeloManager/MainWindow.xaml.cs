using System;
using System.Data.Linq;
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

            Playlist playlist1 = new Playlist { Name = "Плейлист1" };
            Playlist playlist2 = new Playlist { Name = "Плейлист2" };
            Playlist playlist3 = new Playlist { Name = "Плейлист3" };
            Playlist playlist4 = new Playlist { Name = "Плейлист4" };

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
            if (mediaTree1.tree.DataSource != MediaDataContext.MediaContainers)
            {
                mediaTree1.InitDataSource(MediaDataContext.MediaContainers);
            }
        }
    }
}
