using System;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using NLog;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.MeloManager.Id3;
using Oleg_ivo.MeloManager.Repairers;
using Oleg_ivo.Prism;

namespace Oleg_ivo.MeloManager.DependencyInjection
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
            log.Debug("Сохранение настроек");
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
                var alternativeMode = false;
                if (options.Mp3TagRenameMode)
                {
                    alternativeMode = true;
                    var mp3TagRenameRepairer = Container.ResolveUnregistered<Mp3TagRenameRepairer>();
                    mp3TagRenameRepairer.Repair();
                }
                if (options.RepairMode)
                {
                    alternativeMode = true;
                    var autoRepairer = Container.ResolveUnregistered<AutoRepairer>();
                    //autoRepairer.Repair(@"C:\Users\oleg\AppData\Roaming\Winamp\Plugins\ml\playlists\", @"D:\Music");
                    autoRepairer.Repair();
                }
                if (options.Id3Mode)
                {
                    alternativeMode = true;
                    var id3Updater = Container.ResolveUnregistered<Id3Updater>();
                    id3Updater.Run();
                }
                if (alternativeMode)
                {
                    log.Debug("В командной строке заданы альтернативные режимы работы приложения, поэтому главное окно запущено не будет");
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            log.Debug("В командной строке не заданы альтернативные режимы работы приложения, поэтому будет запущено главное окно");
            return true;
        }

        private void UpdateUsers()
        {
            log.Debug("Обновление списка пользователей");//TODO: список пользователей в секции ApplicationSettings нельзя сохранять, поэтому пока все пользователи забиты хардкодом.
            var userName = Environment.UserName.ToLower();
            if (!options.Users.Contains(userName))
            {
                options.Users.Add(userName);
                log.Debug("Текущий пользователь добавлен в список пользователей");
                options.Save();
            }
        }
    }
}