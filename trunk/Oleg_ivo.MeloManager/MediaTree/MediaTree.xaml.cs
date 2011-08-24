using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.Grid;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager
{
    /// <summary>
    /// Interaction logic for MediaTree.xaml
    /// </summary>
    public partial class MediaTree : UserControl, INotifyPropertyChanged
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
            foreach (var category in data.OfType<Category>().Where(c => c != null && c.ParentCategory == null))
            {
                treeSource.AddCategory(category);
            }

            tree.ItemsSource = treeSource;
            //tree.Columns.AddField("Name");
            //tree.DataSource = ds;
            //tree.ExpandAll();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Remove()
        {
            MediaContainerTreeWrapper item = treeListView1.FocusedRow as MediaContainerTreeWrapper;
            var treeSource = tree.ItemsSource as MediaContainerTreeSource;
            if (item != null)
                if (treeSource != null)
                    treeSource.Remove(item);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool NodeFocused
        {
            get
            {
                return treeListView1.FocusedNode != null;
            }
        }

        /*
                private void tree_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
                {
                    btnRemove.IsEnabled = treeListView1.FocusedRow != null;
                }
        */

        ///// <summary>
        ///// 
        ///// </summary>
        //public object DataSource
        //{
        //    get { return tree.DataSource; }
        //    set { tree.DataSource = value; }
        //}
        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public event FocusedRowChangedEventHandler FocusedRowChanged
        {
            add { treeListView1.FocusedRowChanged += value; }
            remove { treeListView1.FocusedRowChanged -= value; }
        }
    }
}
