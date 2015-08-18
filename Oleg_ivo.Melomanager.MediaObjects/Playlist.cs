using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Плейлист
    /// </summary>
    [DebuggerDisplay("Плейлист [{Name}]")]
    public class Playlist : MediaContainer, IEnumerable<MediaFile>
    {
        private string originalFileName;

        /// <summary>
        /// Создаёт плейлист и инициализирует внутренне поле <see cref="originalFileName"/>, а также на основе него добавляет связанный элемент в коллекцию <see cref="MediaContainer.Files"/>
        /// </summary>
        /// <param name="originalFileName"></param>
        /// <param name="mediaCache"></param>
        public Playlist(string originalFileName, IMediaCache mediaCache)
        {
            this.originalFileName = originalFileName;
            if (System.IO.File.Exists(originalFileName) && !Files.Any())
            {
                var file = mediaCache.GetOrAddCachedFile(originalFileName);
                Files.Add(file);
            }
        }

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
        public string GetOriginalFileName(IMediaCache mediaCache)
        {
            return originalFileName ??
                   (originalFileName =
                       Files.Select(file => file.FullFileName)
                           .FirstOrDefault(filename => mediaCache.GetOrAddFileExists(filename)));
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

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<MediaFile> GetEnumerator()
        {
            return ChildContainers.OfType<MediaFile>().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ChildContainers.OfType<MediaFile>().GetEnumerator();
        }
    }
}