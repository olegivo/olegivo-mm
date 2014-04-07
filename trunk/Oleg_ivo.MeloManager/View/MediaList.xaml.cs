using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.MeloManager.ViewModel;

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
    }
}
