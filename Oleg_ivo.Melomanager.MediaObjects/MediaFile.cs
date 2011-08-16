using System;
using System.Linq;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Медиа-файл
    /// </summary>
    partial class MediaFile
    {
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
