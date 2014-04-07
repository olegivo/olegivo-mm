using System.Linq;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Плейлист
    /// </summary>
    partial class Playlist
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
            return string.Format("Плейлист [{0}]", Name);
        }

        /// <summary>
        /// Родительские категории
        /// </summary>
        public IQueryable<Category> ParentCategories
        {
            get { return Parents != null ? Parents.Cast<Category>() : null; }
        }

        /// <summary>
        /// Дочерние файлы
        /// </summary>
        public IQueryable<MediaFile> MediaFiles
        {
            get { return Childs != null ? Childs.Cast<MediaFile>() : null; }
        }

        /// <summary>
        /// Файл-источник плейлиста
        /// </summary>
        public string OriginalFileName { get; set; }

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