using Autofac;
using Oleg_ivo.Base.Autofac.Modules;
using Oleg_ivo.MeloManager.View;
using Oleg_ivo.MeloManager.ViewModel;

namespace Oleg_ivo.MeloManager.Prism
{
    public class MeloManagerAutofacModule : BaseAutofacModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            //Prism registration
            builder.RegisterType<MeloManagerPrismModule>();
            builder.RegisterType<MainWindow>();

            //MVVM registration:
            builder.RegisterType<MainViewModel>();
            builder.RegisterType<MainView>();

            builder.RegisterType<MediaListViewModel>();
            builder.RegisterType<MediaList>();

            builder.RegisterType<MediaTreeViewModel>();
            builder.RegisterType<MediaTree>();
        }
    }
}