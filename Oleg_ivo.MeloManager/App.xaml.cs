using System.Windows;
using Oleg_ivo.MeloManager.DependencyInjection;

namespace Oleg_ivo.MeloManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Windows.Application"/> class.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">More than one instance of the <see cref="T:System.Windows.Application"/> class is created per <see cref="T:System.AppDomain"/>.</exception>
        public App()
        {
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var bootstrapper = new MeloManagerBootstrapper(e.Args);
            bootstrapper.Run();
        }

    }
}
