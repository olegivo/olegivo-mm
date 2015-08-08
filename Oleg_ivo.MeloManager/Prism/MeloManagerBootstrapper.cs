using System;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using NLog;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.Autofac.Modules;
using Oleg_ivo.MeloManager.Repairers;

namespace Oleg_ivo.MeloManager.Prism
{
    public class MeloManagerBootstrapper : AutofacBootstrapperBase<MainWindow,MeloManagerOptions,MeloManagerPrismModule,MeloManagerAutofacModule>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private MeloManagerOptions options;

        public MeloManagerBootstrapper(string[] args) : base(args)
        {
        }

        protected override void InitializeShell()
        {
            options = Container.Resolve<MeloManagerOptions>();
            if (!ProcessArgs())
            {
                Application.Current.Shutdown();
                return;
            }
            base.InitializeShell();

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            Application.Current.Exit += Current_Exit;
        }

        void Current_Exit(object sender, ExitEventArgs e)
        {
            log.Debug("—охранение настроек");
            options.Save();
        }

        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            
        }

        private bool ProcessArgs()
        {
            UpdateUsers();

            try
            {
                if (options.Mp3TagRenameMode)
                {
                    var mp3TagRenameRepairer = Container.ResolveUnregistered<Mp3TagRenameRepairer>();
                    mp3TagRenameRepairer.Repair();
                }
                if (options.RepairMode)
                {
                    var autoRepairer = Container.ResolveUnregistered<AutoRepairer>();
                    //autoRepairer.Repair(@"C:\Users\oleg\AppData\Roaming\Winamp\Plugins\ml\playlists\", @"D:\Music");
                    autoRepairer.Repair();
                }
                if (options.RepairMode || options.Mp3TagRenameMode)
                {
                    log.Debug("¬ командной строке заданы альтернативного режимы работы приложение, поэтому главное окно запущено не будет");
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            log.Debug("¬ командной строке не заданы альтернативного режимы работы приложение, поэтому будет запущено главное окно");
            return true;
        }

        private void UpdateUsers()
        {
            log.Debug("ќбновление списка пользователей");
            var userName = Environment.UserName.ToLower();
            if (!options.Users.Contains(userName))
            {
                options.Users.Add(userName);
                log.Debug("“екущий пользователь добавлен в список пользователей");
                options.Save();
            }
        }
    }
}