using Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.Autofac.Modules;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.View;
using Oleg_ivo.MeloManager.ViewModel;
using Oleg_ivo.MeloManager.Winamp;
using Oleg_ivo.MeloManager.Winamp.Tracking;

namespace Oleg_ivo.MeloManager.Prism
{
    public class MeloManagerAutofacModule : BaseAutofacModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

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
            }).SingleInstance();
            builder.Register(context => context.Resolve<MediaDataContext>()).As<IMediaCache>();

            builder.RegisterType<WinampControl>().SingleInstance();
            builder.RegisterType<WinampFilesMonitor>().SingleInstance();

            //MVVM registration:
            builder.RegisterType<MainViewModel>();
            builder.RegisterType<MainView>();

            builder.RegisterType<MediaListViewModel>();
            builder.RegisterType<MediaList>();

            builder.RegisterType<MediaTreeViewModel>();
            builder.RegisterType<MediaTree>();

            builder.RegisterType<TrackingViewModel>();
        }
    }
}