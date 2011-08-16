using System.Linq;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Плейлист
    /// </summary>
    partial class Playlist
    {
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
            get { return Parents != null ? Parents.Cast<MediaFile>() : null; }
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