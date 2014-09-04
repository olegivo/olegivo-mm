using Oleg_ivo.Base.Autofac.DependencyInjection;

namespace Oleg_ivo.MeloManager.Winamp.Tracking
{
    /// <summary>
    /// Interaction logic for WinampTrackingView.xaml
    /// </summary>
    public partial class WinampTrackingView
    {
        public WinampTrackingView()
        {
            InitializeComponent();
        }

        [Dependency(Required = true)]
        public TrackingViewModel ViewModel
        {
            get { return (TrackingViewModel)DataContext; }
            set { DataContext = value; }
        }

    }
}
