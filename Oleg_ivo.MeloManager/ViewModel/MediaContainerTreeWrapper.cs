using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.ViewModel
{
    /// <summary>
    /// ������ ��� <see cref="MediaContainer"/>
    /// </summary>
    [DebuggerDisplay("Wrapper: {UnderlyingItem}; Parent: {Parent!=null ? Parent.UnderlyingItem : null}")]
    public class MediaContainerTreeWrapper : ViewModelBase
    {
        private readonly Func<MediaContainerTreeWrapper, long> _getMySourceIdDelegateId;
        private Predicate<object> filter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="underlyingItem"></param>
        /// <param name="parent"></param>
        public MediaContainerTreeWrapper(MediaContainer underlyingItem, MediaContainerTreeWrapper parent)
        {
            //������ �� ����� ���� ������
            UnderlyingItem = Enforce.ArgumentNotNull(underlyingItem, "underlyingItem");
            UnderlyingItem.ChildsChanged += UnderlyingItem_ChildsChanged;
            Parent = parent;

            var mediaContainerTreeWrappers = UnderlyingItem.Childs.Select(mc => new MediaContainerTreeWrapper(mc, this));
            ChildItems = new ObservableCollection<MediaContainerTreeWrapper>(mediaContainerTreeWrappers);
            ChildItems.CollectionChanged += ChildItems_CollectionChanged;
        }

        public void DeleteWithChildren(MediaDataContext dataContext)
        {
            if (Parent != null)
            {
                //������ ����� ����� parent � ������� ���������
                var parentContainer = Parent.UnderlyingItem;
                var relation =
                    parentContainer.ChildMediaContainers.Single(rel => rel.ChildMediaContainer == UnderlyingItem);

                dataContext.MediaContainersParentChilds.DeleteOnSubmit(relation);
                UnderlyingItem.ParentMediaContainers.Remove(relation);
                parentContainer.ChildMediaContainers.Remove(relation);
            }

            //� ������ ��������� ������� ��� ������� ���������� �� ����
            if (!UnderlyingItem.Parents.Any())
            {
                dataContext.MediaContainers.DeleteOnSubmit(UnderlyingItem);

                //����������� �������� �������� ���������
                foreach (var childItem in ChildItems.ToList())
                {
                    childItem.DeleteWithChildren(dataContext);
                    ChildItems.Remove(childItem);
                }
            }
        }

        //public IEnumerable<MediaContainerTreeWrapper> GetAllChilds(){}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceIdDelegate"></param>
        /// <param name="underlyingItem"></param>
        /// <param name="parent"></param>
        [Obsolete]
        public MediaContainerTreeWrapper(Func<MediaContainerTreeWrapper, long> sourceIdDelegate, MediaContainer underlyingItem, MediaContainerTreeWrapper parent)
        {
            _getMySourceIdDelegateId = Enforce.ArgumentNotNull(sourceIdDelegate, "sourceIdDelegate");
            //������ �� ����� ���� ������
            UnderlyingItem = Enforce.ArgumentNotNull(underlyingItem, "underlyingItem");
            UnderlyingItem.ChildsChanged += UnderlyingItem_ChildsChanged;
            Parent = parent;

            var mediaContainerTreeWrappers =
                UnderlyingItem.Childs.Select(
                    mc => new MediaContainerTreeWrapper(sourceIdDelegate, mc, this));
            ChildItems = new ObservableCollection<MediaContainerTreeWrapper>(mediaContainerTreeWrappers);
            ChildItems.CollectionChanged += ChildItems_CollectionChanged;
        }

        void ChildItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public MediaContainer UnderlyingItem { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<MediaContainerTreeWrapper> ChildItems { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public long Id
        {
            get { return _getMySourceIdDelegateId(this); }
        }
        /// <summary>
        /// 
        /// </summary>
        public long OriginalId
        {
            get { return UnderlyingItem.Id; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return UnderlyingItem.Name; }
        }

        /// <summary>
        /// 
        /// </summary>
        public MediaContainerTreeWrapper Parent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long ParentId
        {
            get
            {
                return Parent != null ? Parent.Id : 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public BitmapImage Image
        {
            get { return ImageResourceFactory.GetImage(UnderlyingItem.GetType()); }
        }

        public Predicate<object> Filter
        {
            get { return filter; }
            set
            {
                if(filter == value) return;
                filter = value;
                ICollectionView view = CollectionViewSource.GetDefaultView(ChildItems);
                view.Filter = filter;
                foreach (MediaContainerTreeWrapper wrapper in view)
                    wrapper.Filter = filter;
                RaisePropertyChanged(() => Filter);
            }
        }

        #endregion

        /// <summary>
        /// ����� ������ ��� �������� �����-����������:
        /// target ������ ���� ����� UnderlyingItem,
        /// parent (���� �������) ������ ���� ����� ������������ ������-������ �������� ����������
        /// </summary>
        /// <param name="target">������� �����-���������</param>
        /// <param name="parent">������������ ������ ������� ������. ���� null, �� �����������</param>
        /// <returns></returns>
        public MediaContainerTreeWrapper FindChild(MediaContainer target, MediaContainerTreeWrapper parent=null)
        {
            MediaContainerTreeWrapper result = null;

            var parentIsMe = this == parent;

            if (ChildItems != null)
            {
                if (parentIsMe)
                {
                    result = ChildItems.FirstOrDefault(child => child.UnderlyingItem == target);
                }
                else
                {
                    foreach (var childWrapper in ChildItems)
                    {
                        result = 
                            ChildItems.FirstOrDefault(child => child.UnderlyingItem == target) 
                            ??                                  childWrapper.FindChild(target, parent);
                        if (result != null)
                            break;
                    }
                }
            }


            return result;
        }

        /// <summary>
        /// ���������� ����� �����, � ������� (<see cref="UnderlyingItem"/> == <see cref="target"/>), 
        /// ��� ���� � ������������� �������� (<see cref="UnderlyingItem"/> == <see cref="parent"/>)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parent">������������ ����������� �������� ��� ������������� ����������</param>
        /// <returns></returns>
        public List<MediaContainerTreeWrapper> FindChildren(MediaContainer target, MediaContainer parent)
        {
            var result = new List<MediaContainerTreeWrapper>();

            var parentIsMe = parent == UnderlyingItem;
            foreach (var childWrapper in ChildItems)
            {
                if (parentIsMe && childWrapper.UnderlyingItem == target)
                    result.Add(childWrapper);
                result.AddRange(childWrapper.FindChildren(target, parent));
            }

            return result;
        }

        /// <summary>
        /// ����� ������ ��� �������� �����-����������:
        /// target ������ ���� ����� UnderlyingItem,
        /// parent (���� �������) ������ ���� ����� ������������ ������-������ �������� ����������
        /// child (���� �������) ������ ����������� � ��������� ��������-������ �������� ����������
        /// </summary>
        /// <param name="target">������� �����-���������</param>
        /// <param name="child">�������� ������ ������� ������</param>
        /// <returns></returns>
        public MediaContainerTreeWrapper FindParent(MediaContainer target, MediaContainerTreeWrapper child)
        {
            if(child==null)
                throw new ArgumentNullException("child");

            MediaContainerTreeWrapper result = null;
            var childIsMe = this == child;

            if (Parent != null)
            {
                result = childIsMe && target == Parent.UnderlyingItem
                    ? Parent
                    : Parent.FindParent(target, child);
            }

            return result;
        }

        void UnderlyingItem_ChildsChanged(object sender, MediaListChangedEventArgs e)
        {
            if (ChildsChanged != null)
                ChildsChanged(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        internal event EventHandler<MediaListChangedEventArgs> ChildsChanged;
    }
}