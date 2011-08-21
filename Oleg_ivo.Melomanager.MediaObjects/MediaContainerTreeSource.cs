using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// 
    /// </summary>
    public class MediaContainerTreeSource : BindingList<MediaContainerTreeWrapper>
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
            item.ChildsChanged += item_ChildsChanged;

            //����������� ���������� �������� ���������
            foreach (var child in item.UnderlyingItem.Childs)
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

            //����������� �������� �������� ���������
            foreach (var child in underlyingItem.Childs.ToList())
            {
                var childWrapper = FindItem(item, child);
                Remove(childWrapper);
                //����� �������� �� ������ � �������, �� � � ������ �����-�����������
                //RemoveRelation(underlyingItem, child);

                //�������� ��������� �����-����������, ���� � ���� ��� ������ ���������
                //RemoveIfNoParent(child);
            }

            MediaContainerTreeWrapper parentWrapper = item.Parent;
            if (parentWrapper != null)
            {
                parentWrapper.ChildsChanged -= item_ChildsChanged;
                RemoveRelation(parentWrapper.UnderlyingItem, underlyingItem);
                parentWrapper.ChildsChanged += item_ChildsChanged;
            }
            item.ChildsChanged -= item_ChildsChanged;

            //�������� ������������� �����-����������, ���� � ���� ��� ������ ���������
            RemoveIfNoParent(underlyingItem);

            //������� �������� �� ������ � �����, �.�. ������� ������� ������ ��������� ����� ������
            base.Remove(item);
        }

        private void RemoveRelation(MediaContainer parent, MediaContainer child)
        {
            var parentRelation = parent.GetChildRelation(child);
            var childRelation = child.GetParentRelation(parent);
            parent.RemoveChild(child);
            child.RemoveParent(parent);
                
            //�������� �����, �.�. ��� ���� ���� �� �������
            parent.ChildMediaContainers.Remove(parentRelation);
            child.ParentMediaContainers.Remove(childRelation);
            MediaDataContext.MediaContainersParentChilds.DeleteOnSubmit(parentRelation);
            MediaDataContext.MediaContainersParentChilds.DeleteOnSubmit(childRelation);
        }

        private void RemoveIfNoParent(MediaContainer mediaContainer)
        {
            if (!mediaContainer.ParentMediaContainers.Any())
            {
                MediaDataContext.MediaContainers.DeleteOnSubmit(mediaContainer);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        public void AddCategory(Category category)
        {
            var item = category.CreateWrapper(GetSourceId);
            Add(item);
        }

        void item_ChildsChanged(object sender, MediaListChangedEventArgs e)
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
    /// ����� ��� ��������� ������ ��� ���������. ������ ������ ��������� ����� �� ����� ������������� ��������
    /// </summary>
    internal static class CategoryWrapperExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="GetSourceId"></param>
        /// <returns></returns>
        public static MediaContainerTreeWrapper CreateWrapper(this Category category, MediaContainerTreeWrapper.getMyTreeSourceIdDelegate GetSourceId)
        {
            return new MediaContainerTreeWrapper(GetSourceId, category, null);
        }
    }
}