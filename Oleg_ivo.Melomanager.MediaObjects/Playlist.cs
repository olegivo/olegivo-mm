using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Плейлист
    /// </summary>
    [DebuggerDisplay("Плейлист [{Name}]")]
    public class Playlist : MediaContainer
    {
        private string originalFileName;
        private readonly IMediaCache mediaCache;

        /// <summary>
        /// Создаёт плейлист и инициализирует <see cref="OriginalFileName"/>, а также на основе него добавляет связанный элемент в коллекцию <see cref="MediaContainer.Files"/>
        /// </summary>
        /// <param name="originalFileName"></param>
        /// <param name="mediaCache"></param>
        public Playlist(string originalFileName, IMediaCache mediaCache)
        {
            this.originalFileName = originalFileName;
            this.mediaCache = mediaCache;
            if (System.IO.File.Exists(originalFileName) && !Files.Any())
            {
                var file = MediaCache.GetOrAddCachedFile(originalFileName);
                Files.Add(file);
            }
        }

        [Obsolete("Должен быть protected")]
        public Playlist()
        {
        }

        /// <summary>
        /// Родительские категории
        /// </summary>
        public IEnumerable<Category> ParentCategories
        {
            get { return ParentContainers != null ? ParentContainers.OfType<Category>() : null; }
        }

        /// <summary>
        /// Дочерние файлы
        /// </summary>
        public IEnumerable<MediaFile> MediaFiles
        {
            get { return ChildContainers != null ? ChildContainers.OfType<MediaFile>() : null; }
        }

        /// <summary>
        /// Файл-источник плейлиста (в случае отсутствия вычисляется как первый из существующих файлов данного медиа-контейнера)
        /// </summary>
        public string OriginalFileName
        {
            get
            {
                return originalFileName ??
                       (originalFileName =
                           Files.Select(file => file.FullFileName)
                               .FirstOrDefault(System.IO.File.Exists/*TODO: кеширование признака "существует"? (например, в MediaCache)*/));
            }
        }

        public IMediaCache MediaCache
        {
            get { return mediaCache; }
        }

        /// <summary>
        /// Добавить дочерний медиа-файл
        /// </summary>
        /// <param name="child"></param>
        public void AddChildMediaFile(MediaFile child)
        {
            AddChild(child);
        }
        /// <summary>
        /// Удалить дочерний медиа-файл
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChildMediaFile(MediaFile child)
        {
            RemoveChild(child);
        }

        /// <summary>
        /// Добавить родительскую категорию
        /// </summary>
        /// <param name="parent"></param>
        public void AddParentCategory(Category parent)
        {
            AddParent(parent);
        }

        /// <summary>
        /// Удалить родительскую категорию
        /// </summary>
        /// <param name="parent"></param>
        public void RemoveParentCategory(Category parent)
        {
            RemoveParent(parent);
        }

    }
}