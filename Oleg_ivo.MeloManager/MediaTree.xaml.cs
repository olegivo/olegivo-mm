using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager
{
    /// <summary>
    /// Interaction logic for MediaTree.xaml
    /// </summary>
    public partial class MediaTree : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public MediaTree()
        {
            InitializeComponent();

            //InitDataSource();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void InitDataSource(IEnumerable<MediaContainer> data)
        {
            MediaContainerTreeSource treeSource = new MediaContainerTreeSource();
            treeSource.MediaDataContext = DataProvider.DataContext;
            foreach (var category in data.OfType<Category>().Where(c => c!=null && c.ParentCategory == null))
            {
                treeSource.AddCategory(category);
            }

            tree.DataSource = treeSource;
            //tree.Columns.AddField("Name");
            //tree.DataSource = ds;
            //tree.ExpandAll();
        }

        private void btnRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MediaContainerTreeWrapper item = tree.GetDataRecordByNode(tree.FocusedNode) as MediaContainerTreeWrapper;
            var treeSource = tree.DataSource as MediaContainerTreeSource;
            if (item!=null && treeSource!=null)
            {
                treeSource.Remove(item);
            }
            //tree.Selection
        }

        /// <summary>
        /// 
        /// </summary>
        public bool NodeFocused
        {
            get
            {
                return tree.FocusedNode != null;
            }
        }

        private void tree_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            btnRemove.IsEnabled = tree.FocusedNode != null;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //public object DataSource
        //{
        //    get { return tree.DataSource; }
        //    set { tree.DataSource = value; }
        //}
    }
}
