using System.Windows;
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

        /*public static readonly DependencyProperty IntValueProperty =
        DependencyProperty.Register("MediaTreeViewModel", typeof(int), typeof(MediaTreeViewModel), new UIPropertyMetadata(1, OnIntValuePropertyChanged));
        private static void OnIntValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaTreeViewModel newPropertyValue = (MediaTreeViewModel)e.NewValue;
            MediaTree instance = (MediaTree)d;
            // Perform callback action.
        }*/

        public MediaTreeViewModel ViewModel
        {
            get { return (MediaTreeViewModel)DataContext; }
            /*set
            {
                if (DataContext == value) return;
                if (ViewModel != null)
                    ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                DataContext = value;
                if (ViewModel != null)
                    ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }*/
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
            //var treeViewItem = radTreeView.GetItemByPath(path);
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
                treeSource.AddMediaContainer(category);
            }

            tree.ItemsSource = treeSource;
            //tree.Columns.AddField("Name");
            //tree.DataSource = ds;
            //tree.ExpandAll();
        }
        */

        private void MediaTree_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NameFilter")
            {
                if (string.IsNullOrEmpty(ViewModel.NameFilter))
                {
                    tree.CollapseAll();
                }
                else
                {
                    tree.ExpandAll();
                }
            }
        }

    }
}
