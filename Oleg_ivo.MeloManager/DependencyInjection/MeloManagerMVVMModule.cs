using Autofac;
using Oleg_ivo.Base.WPF.Dialogs;
using Oleg_ivo.MeloManager.Dialogs;
using Oleg_ivo.MeloManager.Dialogs.ParentsChildsEdit;
using Oleg_ivo.MeloManager.Dialogs.SettingsEdit;
using Oleg_ivo.MeloManager.View;
using Oleg_ivo.MeloManager.ViewModel;
using Oleg_ivo.MeloManager.Winamp.Tracking;

namespace Oleg_ivo.MeloManager.DependencyInjection
{
    public class MeloManagerMVVMModule : Module
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

            builder.RegisterType<ParentsChildsEditDialogViewModel>();
            builder.RegisterType<ParentsChildsEditDialog>().AsImplementedInterfaces();

            builder.RegisterType<DiffResultDialogViewModel>();
            builder.RegisterType<DiffResultDialog>().AsImplementedInterfaces();

            builder.RegisterType<SettingsEditDialogViewModel>();
            builder.RegisterType<SettingsEditDialog>().AsImplementedInterfaces();
        }
    }
}