using System;
using System.IO;
using System.Windows;
using Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.WPF.Dialogs;
using Oleg_ivo.MeloManager.Dialogs;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.MediaObjects.Extensions;
using Oleg_ivo.MeloManager.Winamp.Tracking;
using File = Oleg_ivo.MeloManager.MediaObjects.File;

namespace Oleg_ivo.MeloManager.ViewModel
{
    public static class TestMethods
    {
        public static void TestInsertMediaFileWithFile(MediaDbContext dbContext)
        {
            var fullFilename = @"D:\Music\Disk\Music\9\Sixpence None the Richer - Kiss me.mp3";
            var fileName = Path.GetFileName(fullFilename);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFilename);
            var extension = Path.GetExtension(fullFilename);
            var drive = Path.GetPathRoot(fullFilename);
            var path = Path.GetDirectoryName(fullFilename);
            var file = new File
            {
                FullFileName = fullFilename,
                Drive = drive,
                Path = path,
                Filename = fileName,
                FileNameWithoutExtension = fileNameWithoutExtension,
                Extention = extension
            };
            var mediaFile = new MediaFile { Name = "Test" };
            file.MediaContainers.Add(mediaFile);

            dbContext.MediaContainers.Add(mediaFile);
            dbContext.Files.Add(file);

            dbContext.SubmitChangesWithLog(Console.WriteLine);
        }

        public static void TestDialogService(IModalDialogService modalDialogService)
        {
            modalDialogService.CreateAndShowDialog<SimpleStringDialogViewModel>(
                modalWindow =>
                {
                    //modalWindow.Title = "Ввод строкового значения";
                    modalWindow.ViewModel.Caption = "Ввод строкового значения";
                    modalWindow.ViewModel.ContentViewModel.Value = "test";
                    modalWindow.ViewModel.ContentViewModel.Description = "Введите значение";
                },
                (model, dialogResult) =>
                {
                    if (dialogResult.HasValue)
                    {
                        if (dialogResult.Value)
                            MessageBox.Show("Введено: " + model.ContentViewModel.Value);
                        else
                            MessageBox.Show("Не введено");
                    }
                    else
                        MessageBox.Show("Неизвестно");
                });
        }

        public static void TestWinampTrackingWindow(IComponentContext context)
        {
            var winampTrackingWindow = context.ResolveUnregistered<WinampTrackingWindow>();
            winampTrackingWindow.ShowDialog();
        }
    }
}