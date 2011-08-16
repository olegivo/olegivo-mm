using System.Linq;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Категория
    /// </summary>
    partial class Category
    {
        /// <summary>
        /// Родительская категория
        /// </summary>
        public Category ParentCategory
        {
            get { return Parents != null ? Parents.Cast<Category>().FirstOrDefault() : null; }
            set
            {
                if (ParentMediaContainers.Count > 0) ParentMediaContainers.Clear();//у категории не может быть несколько родителей
                AddParent(value);
            }
        }

        /// <summary>
        /// Дочерние элементы
        /// </summary>
        public new IQueryable<MediaContainer> Childs
        {
            get { return Childs; }
        }

        /// <summary>
        /// Добавить дочерний элемент
        /// </summary>
        /// <param name="child"></param>
        public new void AddChild(MediaContainer child)
        {
            base.AddChild(child);
        }

        /// <summary>
        /// Удалить дочерний элемент
        /// </summary>
        /// <param name="child"></param>
        public new void RemoveChild(MediaContainer child)
        {
            base.RemoveChild(child);
        }
    }
}