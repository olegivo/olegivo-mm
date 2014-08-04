using System;
using System.Windows;
using Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.Autofac.Modules;
using Oleg_ivo.MeloManager.Repairers;

namespace Oleg_ivo.MeloManager.Prism
{
    public class MeloManagerBootstrapper : AutofacBootstrapperBase<MainWindow,MeloManagerOptions,MeloManagerPrismModule,MeloManagerAutofacModule>
    {
        public MeloManagerBootstrapper(string[] args) : base(args)
        {
        }

        protected override void InitializeShell()
        {
            if (!ProcessArgs())
            {
                Application.Current.Shutdown();
                return;
            }
            base.InitializeShell();

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            
        }

        private bool ProcessArgs()
        {
            try
            {
                var options = Container.Resolve<MeloManagerOptions>();
                if (options.Mp3TagRenameMode)
                {
                    var mp3TagRenameRepairer = Container.ResolveUnregistered<Mp3TagRenameRepairer>();
                    //mp3TagRenameRepairer.Repair(@"C:\Users\oleg\AppData\Roaming\Winamp\Plugins\ml\playlists\", @"C:\Users\oleg\AppData\Local\Temp\Mp3tag v2.58\preview.txt");
                    mp3TagRenameRepairer.Repair();
                }
                if (options.RepairMode)
                {
                    var autoRepairer = Container.ResolveUnregistered<AutoRepairer>();
                    //autoRepairer.Repair(@"C:\Users\oleg\AppData\Roaming\Winamp\Plugins\ml\playlists\", @"D:\Music");
                    autoRepairer.Repair();
                }
                if (options.RepairMode || options.Mp3TagRenameMode)
                    return false;
            }
            catch (Exception ex)
            {
                throw;
            }

            return true;
        }
    }
}