using System;
using Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.Autofac.Modules;
using Oleg_ivo.MeloManager.MediaObjects;
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

            //Services
            builder.Register(context =>
            {
                var meloManagerOptions = context.Resolve<MeloManagerOptions>();
                var mediaDataContext = context.ResolveUnregistered<MediaDataContext>(
                    new TypedParameter(typeof (string),
                    meloManagerOptions.ConnectionString));
                //mediaDataContext.Log = Console.Out;
                //mediaDataContext.ObjectTrackingEnabled = true;
                return mediaDataContext;
            });

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