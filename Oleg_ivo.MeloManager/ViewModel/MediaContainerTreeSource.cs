using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class MediaContainerTreeSource : ObservableCollection<MediaContainerTreeWrapper>
    {
        private long identity = 0;
        private Hashtable ids = new Hashtable();

        internal long GetId(long originalId)
        {
            return (long) (ids[originalId] ?? 0);
        }

        /// <summary>
        /// 
        /// </summary>
        public MediaDataContext MediaDataContext { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public new void Add(MediaContainerTreeWrapper item)
        {
            base.Add(item);
            item.ChildrenChanged += item_ChildrenChanged;

            //рекурсивное добавление дочерних элементов
            foreach (var child in item.UnderlyingItem.Children)
            {
                Add(new MediaContainerTreeWrapper(GetSourceId, child, item));
            }

        }

        private long GetSourceId(MediaContainerTreeWrapper key)
        {
            var id = ids[key];
            if (id==null)
            {
                ids.Add(key, ++identity);
                id = identity;
            }

            return (long) id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public new void Remove(MediaContainerTreeWrapper item)
        {
            MediaContainer underlyingItem = item.UnderlyingItem;

            //рекурсивное удаление дочерних элементов
            foreach (var child in underlyingItem.Children.ToList())
            {
                var childWrapper = FindItem(item, child);
                Remove(childWrapper);
                //вызов удаления не только в обёртках, но и в дереве медиа-контейнеров
                //RemoveRelation(underlyingItem, child);

                //удаление дочернего медиа-контейнера, если у него нет больше родителей
                //RemoveIfNoParent(child);
            }

            MediaContainerTreeWrapper parentWrapper = item.Parent;
            if (parentWrapper != null)
            {
                parentWrapper.ChildrenChanged -= item_ChildrenChanged;
                RemoveRelation(parentWrapper.UnderlyingItem, underlyingItem);
                parentWrapper.ChildrenChanged += item_ChildrenChanged;
            }
            item.ChildrenChanged -= item_ChildrenChanged;

            //удаление родительского медиа-контейнера, если у него нет больше родителей
            RemoveIfNoParent(underlyingItem);

            //базовое уделение из списка в конце, т.к. верхний элемент должен удаляться после нижних
            base.Remove(item);
        }

        private void RemoveRelation(MediaContainer parent, MediaContainer child)
        {
            MediaDataContext.RemoveRelation(parent, child);
        }

        private void RemoveIfNoParent(MediaContainer mediaContainer)
        {
            if (!mediaContainer.ParentMediaContainers.Any())
            {
                MediaDataContext.MediaContainers.DeleteOnSubmit(mediaContainer);
            }
        }

        /// <summary>
        /// Добавить категорию
        /// </summary>
        /// <param name="category"></param>
        public void AddCategory(Category category)
        {
            var item = new MediaContainerTreeWrapper(GetSourceId, category, null);
            Add(item);
        }

        void item_ChildrenChanged(object sender, MediaListChangedEventArgs e)
        {
            MediaContainerTreeWrapper parent = sender as MediaContainerTreeWrapper;
            MediaContainerTreeWrapper child;
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    child = new MediaContainerTreeWrapper(GetSourceId, e.MediaContainer, parent);
                    Add(child);
                    break;
                case ListChangedType.ItemDeleted:
                    child = FindItem(parent, e.MediaContainer);
                    Remove(child);
                    break;
            }
        }

        private MediaContainerTreeWrapper FindItem(MediaContainerTreeWrapper parent, MediaContainer underlyingItem)
        {
            return
                this.Where(item => item.Parent == parent && item.UnderlyingItem == underlyingItem)
                    .SingleOrDefault();
        }
    }

    /// <summary>
    /// Класс для поддержки обёртки для категорий. Только обёртки категории могут не иметь родительского элемента
    /// </summary>
    internal static class CategoryWrapperExtension
    {
    }
}