using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Категория
    /// </summary>
    [DebuggerDisplay("Категория [{Name}]")]
    public class Category : MediaContainer
    {
        /// <summary>
        /// Родительская категория
        /// </summary>
        public Category ParentCategory
        {
            get { return ParentContainers != null ? ParentContainers.OfType<Category>().FirstOrDefault() : null; }
            set
            {
                if (ParentContainers.Any()) ParentContainers.Clear();//у категории не может быть несколько родителей // TODO: на данный момент
                AddParent(value);
            }
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
        /// Добавить дочерний элемент
        /// </summary>
        /// <param name="children"></param>
        public void AddChildren(IEnumerable<MediaContainer> children)
        {
            foreach (var child in children)
            {
                base.AddChild(child);
            }
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