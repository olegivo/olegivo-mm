﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Медиа-файл
    /// </summary>
    partial class MediaFile
    {
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("Медиа-файл [{0}]", Name);
        }

        public override void BatchRepair(IEnumerable<string> foundFiles, bool optionRepairOnlyBadFiles)
        {
            //base.BatchRepair(foundFiles, optionRepairOnlyBadFiles);
            IsRepaired = false;


            var repairedFiles =
                (optionRepairOnlyBadFiles
                    ? MediaContainerFiles.Where(mcf => !mcf.File.FileInfo.Exists)
                    : MediaContainerFiles)
                    .Select(mcf => new {old=mcf, repairedFile=mcf.File.Repair(foundFiles)});

            foreach (var rf in repairedFiles.Where(rf=>rf.repairedFile != null).ToList())
            {
                MediaContainerFiles.Remove(rf.old);
                MediaContainerFiles.Add(new MediaContainerFile { File = rf.repairedFile });
            }

            IsRepaired = true;
/*
            //InnerElements.Clear();
            //AddRepairedFromHistory();
            
            var errors = new List<string>();
            //создание списка найденных файлов:
            foreach (string file in foundFiles)
                if (file.Contains(FileInfo.Name))
                {
                    RepairedFile repairedFile = new RepairedFile(file, tags != null ? tags.ToArray() : null);
                    InnerElements.Add(repairedFile);
                }

            if (InnerElements.Count > 0)
            {
                Repaired = true;
                if (InnerElements.Count > 1)
                {
                    MediaCollection mediaCollection = GetCorrectFiles(InnerElements, FileInfo.FullName);

                    if (mediaCollection.Count > 0)
                    {
                        InnerElements.Clear();
                        InnerElements.Add(mediaCollection);
                    }
                }

                if (InnerElements.Count == 1)
                {
                    RepairedFile repairedFile = InnerElements[0] as RepairedFile;
                    repairedFile.ReplaceToParent();
                }
            }
            if (errors.Count > 0)
            {
                //MessageBox.Show(string.Format("Имеются ошибки:{0}", errors.ToArray()), 
                //                "Проверьте настройки программы",
                //                MessageBoxButtons.OK,
                //                MessageBoxIcon.Exclamation);
            }
*/
        }

        /// <summary>
        /// Родительские элементы
        /// </summary>
        public new IQueryable<MediaContainer> ParentMediaContainers
        {
            get { return Parents != null ? Parents.Cast<MediaContainer>() : null; }
        }

        /// <summary>
        /// Добавить родительский элемент
        /// </summary>
        /// <param name="parent"></param>
        /// <exception cref="ArgumentException">Если аргумент - не категория или плейлист</exception>
        public void AddParentElement(MediaContainer parent)
        {
            if (!(parent is Category || parent is Playlist))
                throw new ArgumentException("ожидается категория или плейлист", "parent");
            AddParent(parent);
        }

        /// <summary>
        /// Удалить родительский элемент
        /// </summary>
        /// <param name="parent"></param>
        public void RemoveParentElement(MediaContainer parent)
        {
            RemoveParent(parent);
        }
    }
}