using Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.Autofac.Modules;
using Oleg_ivo.Base.WPF.Dialogs;
using Oleg_ivo.MeloManager.Dialogs;
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
                var mediaDataContext = context.ResolveUnregistered<MediaDbContext>(
                    new TypedParameter(typeof (string),
                    meloManagerOptions.ConnectionString));
                //mediaDataContext.Log = Console.Out;
                //mediaDataContext.ObjectTrackingEnabled = true;
                return mediaDataContext;
            }).SingleInstance();
            builder.Register(context => context.Resolve<MediaDbContext>()).As<IMediaCache>().As<IMediaRepository>();

            builder.RegisterType<WinampControl>().SingleInstance();
            builder.RegisterType<WinampFileAdapterService>().SingleInstance();
            builder.RegisterType<WinampFilesMonitor>().SingleInstance();

            //MVVM registration:
            builder.RegisterType<MainViewModel>();
            builder.RegisterType<MainView>();

            builder.RegisterType<MediaListViewModel>();
            builder.RegisterType<MediaList>();

            builder.RegisterType<MediaTreeViewModel>();
            builder.RegisterType<MediaTree>();

            builder.RegisterType<TrackingViewModel>();

            //MVVM Dialogs
            builder.RegisterType<ModalDialogService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterGeneric(typeof(DialogViewModel<>));

            builder.RegisterType<SimpleStringDialogViewModel>();
            builder.RegisterType<SimpleStringDialog>().AsImplementedInterfaces();
        }
    }
}