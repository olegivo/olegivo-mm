using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.MeloManager.ViewModel;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Oleg_ivo.MeloManager.View
{
    /// <summary>
    /// Interaction logic for MediaList.xaml
    /// </summary>
    public partial class MediaList
    {
        public MediaList()
        {
            InitializeComponent();
        }

        [Dependency(Required = true)]
        public MediaListViewModel ViewModel
        {
            get { return (MediaListViewModel) DataContext; }
            set { DataContext = value; }
        }

        private void Grid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null || grid.SelectedItems == null || grid.SelectedItems.Count != 1) 
                return;
            
            var element = e.MouseDevice.DirectlyOver as FrameworkElement;
            if (element != null)
            {
                var cell = element.ParentOfType<DataGridCell>();
                if (cell != null)
                {
                    ViewModel.OnCellDoubleClick();
                }

                var row = element.ParentOfType<DataGridRow>();
                if (row != null)
                {
                    ViewModel.OnRowDoubleClick();
                }

            }
            /*FrameworkElement originalSender = e.OriginalSource as FrameworkElement;
            if (originalSender != null)
            {
                var cell = originalSender.ParentOfType<GridViewCell>();
                if (cell != null)
                {
                    ViewModel.OnCellDoubleClick();
                }

                var row = originalSender.ParentOfType<GridViewRow>();
                if (row != null)
                {
                    ViewModel.OnRowDoubleClick();
                }
            }*/
        }
    }
}
