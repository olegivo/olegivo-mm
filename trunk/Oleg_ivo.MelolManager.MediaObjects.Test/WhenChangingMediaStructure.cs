using System.Linq;
using NUnit.Framework;

namespace Oleg_ivo.MeloManager.MediaObjects.Test
{
    /// <summary>
    /// При изменении структуры медиа-данных (добавление/удаление в коллекцию дочерних/родительских элементов)
    /// </summary>
    [TestFixture]
    public class WhenChangingMediaStructure
    {
        /// <summary>
        /// 
        /// </summary>
        [TestFixtureSetUp]
        public void ClassInit()
        {
            // Arrange, Act
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Test()
        {

        }
    }

    /// <summary>
    /// Категория
    /// </summary>
    internal static class CategoryExtensions
    {

        /// <summary>
        /// Может содержаться максимум в одной категории
        /// </summary>
        /// <param name="category"></param>
        internal static void CanContainedInOneCategory(this Category category)
        {
            if(category.ParentCategory == null) category.ParentCategory = new Category();
            Category parent2 = new Category();
            
        }

        /// <summary>
        /// Может содержать категории, плейлисты и файлы
        /// </summary>
        /// <param name="category"></param>
        internal static void CanContainsCategoriesPlaylistsAndFiles(this Category category)
        {
            
        }

    }

    /// <summary>
    /// Плейлист
    /// </summary>
    internal static class PlaylistExtensions
    {

        /// <summary>
        /// Может содержаться только в категориях
        /// </summary>
        /// <param name="category"></param>
        internal static void CanContainedOnlyInCategories(this Playlist category)
        {
            
        }

        /// <summary>
        /// Может содержать файлы
        /// </summary>
        /// <param name="category"></param>
        internal static void CanContainsFiles(this Playlist category)
        {
            
        }

    }

    /// <summary>
    /// Файл
    /// </summary>
    internal static class FileExtensions
    {

        /// <summary>
        /// Может содержаться только в плейлистах или категориях
        /// </summary>
        /// <param name="category"></param>
        internal static void CanContainedOnlyInCategoriesOrPlaylists(this MediaFile category)
        {
            
        }

        /// <summary>
        /// Может содержать поля
        /// </summary>
        /// <param name="category"></param>
        internal static void CanContainsFields(this MediaFile category)
        {
            
        }

    }
}
