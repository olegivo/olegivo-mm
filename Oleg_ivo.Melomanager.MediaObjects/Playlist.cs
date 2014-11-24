using System.Diagnostics;
using System.Linq;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Плейлист
    /// </summary>
    [DebuggerDisplay("Плейлист [{Name}]")]
    partial class Playlist
    {
        private string originalFileName;

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
            get { return Children != null ? Children.Cast<MediaFile>() : null; }
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
                           MediaContainerFiles.Select(mcf => mcf.File.FullFileName)
                               .FirstOrDefault(System.IO.File.Exists));
            }
            set
            {
                if(originalFileName == value) return;
                originalFileName = value;
                if (System.IO.File.Exists(OriginalFileName) && !MediaContainerFiles.Any())
                {
                    var file = MediaCache.GetOrAddCachedFile(OriginalFileName);
                    MediaContainerFiles.Add(new MediaContainerFile { File = file });
                }
            }
        }

        public IMediaCache MediaCache { get; set; }

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