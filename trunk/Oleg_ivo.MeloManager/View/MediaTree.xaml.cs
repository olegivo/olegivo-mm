using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.MeloManager.ViewModel;
using Telerik.Windows.Controls;

namespace Oleg_ivo.MeloManager.View
{
    /// <summary>
    /// Interaction logic for MediaTree.xaml
    /// </summary>
    public partial class MediaTree
    {
        /// <summary>
        /// 
        /// </summary>
        public MediaTree()
        {
            InitializeComponent();
        }

        [Dependency(Required = true)]
        public MediaTreeViewModel ViewModel
        {
            get { return (MediaTreeViewModel)DataContext; }
            set { DataContext = value; }
        }

        private void RadTreeView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var radTreeView = (RadTreeView)sender;
            var currentItem = ViewModel.CurrentItem;
            string path = string.Empty;
            while (currentItem != null)
            {
                if (path != string.Empty)
                    path = radTreeView.PathSeparator + path;
                path = currentItem.Name + path;
                currentItem = currentItem.Parent;
            }
            var treeViewItem = radTreeView.GetItemByPath(path);
            radTreeView.BringPathIntoView(path);
        }


        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void InitDataSource(IEnumerable<MediaContainer> data)
        {
            MediaContainerTreeSource treeSource = new MediaContainerTreeSource();
            treeSource.MediaDataContext = DataProvider.DataContext;
            foreach (var category in data.OfType<Category>().Where(c => c != null && c.ParentCategory == null))
            {
                treeSource.AddCategory(category);
            }

            tree.ItemsSource = treeSource;
            //tree.Columns.AddField("Name");
            //tree.DataSource = ds;
            //tree.ExpandAll();
        }
        */

    }
}
