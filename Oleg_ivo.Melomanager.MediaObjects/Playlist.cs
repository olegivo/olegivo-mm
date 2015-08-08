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
        private readonly IMediaCache mediaCache;

        /// <summary>
        /// Создаёт плейлист и инициализирует <see cref="OriginalFileName"/>, а также на основе него добавляет связанный элемент в коллекцию <see cref="MediaContainer.MediaContainerFiles"/>
        /// </summary>
        /// <param name="originalFileName"></param>
        /// <param name="mediaCache"></param>
        public Playlist(string originalFileName, IMediaCache mediaCache)
        {
            this.originalFileName = originalFileName;
            this.mediaCache = mediaCache;
            if (System.IO.File.Exists(originalFileName) && !MediaContainerFiles.Any())
            {
                var file = MediaCache.GetOrAddCachedFile(originalFileName);
                MediaContainerFiles.Add(new MediaContainerFile { File = file });
            }
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