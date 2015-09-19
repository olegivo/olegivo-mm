using Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.Autofac.Modules;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.PlaylistFileAdapters;
using Oleg_ivo.MeloManager.Winamp;

namespace Oleg_ivo.MeloManager.DependencyInjection
{
    public class MeloManagerAutofacModule : BaseAutofacModule
    {
        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        /// <param name="builder">The builder through which components can be
        ///             registered.</param>
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
            builder.RegisterType<WinampFileAdapterService>().SingleInstance().As<IFileAdapterService>();
            builder.RegisterType<WinampFilesMonitor>().SingleInstance();

            builder.RegisterModule<MeloManagerMVVMModule>();
        }
    }
}