using System.Windows;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.MeloManager.ViewModel;

namespace Oleg_ivo.MeloManager.View
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

        [Dependency(Required = true)]
        public MainViewModel ViewModel
        {
            get { return (MainViewModel) DataContext; }
            set { DataContext = value; }
        }

        /* TODO:
        private void bbiInit_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            ViewModel.InitDataSource();
        }

        private void bbiLoad_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            ViewModel.LoadFromDb();
        }

        private void bbiSave_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            ViewModel.SaveAndLoad();
        }
        */

        private void MainView_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.InitDataSource();//LoadFromDb();
        }
    }
}
