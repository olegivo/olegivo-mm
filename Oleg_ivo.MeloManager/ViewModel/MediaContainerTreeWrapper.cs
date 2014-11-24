using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Autofac;
using GalaSoft.MvvmLight;
using Microsoft.Expression.Interactivity.Core;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.PlaylistFileAdapters;
using Oleg_ivo.MeloManager.Winamp;

namespace Oleg_ivo.MeloManager.ViewModel
{
    /// <summary>
    /// ������ ��� <see cref="MediaContainer"/>
    /// </summary>
    [DebuggerDisplay("Wrapper: {UnderlyingItem}; Parent: {Parent!=null ? Parent.UnderlyingItem : null}")]
    public class MediaContainerTreeWrapper : ViewModelBase
    {
        private readonly WinampControl winampControl;
        private readonly IComponentContext context;
        private readonly Func<MediaContainerTreeWrapper, long> _getMySourceIdDelegateId;
        private Predicate<object> filter;
        private ICommand commandPlay;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="underlyingItem"></param>
        /// <param name="parent"></param>
        /// <param name="context"></param>
        /// <param name="winampControl"></param>
        public MediaContainerTreeWrapper(MediaContainer underlyingItem, MediaContainerTreeWrapper parent, IComponentContext context, WinampControl winampControl)//TODO: ������� ����� ������������ � �����-�������
        {
            this.winampControl = winampControl;
            this.context = Enforce.ArgumentNotNull(context, "context");
            UnderlyingItem = Enforce.ArgumentNotNull(underlyingItem, "underlyingItem");
            UnderlyingItem.ChildrenChanged += UnderlyingItem_ChildrenChanged;
            Parent = parent;

            var mediaContainerTreeWrappers = UnderlyingItem.Children.Select(mc => new MediaContainerTreeWrapper(mc, this, context, winampControl));
            ChildItems = new ObservableCollection<MediaContainerTreeWrapper>(mediaContainerTreeWrappers);
            ChildItems.CollectionChanged += ChildItems_CollectionChanged;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("{0}", UnderlyingItem!=null ? UnderlyingItem.ToString() : "������ ����������");
        }

        public void DeleteWithChildren(MediaDataContext dataContext)
        {
            if (Parent != null)
            {
                //������ ����� ����� parent � ������� ���������
                var parentContainer = Parent.UnderlyingItem;
                var relation =
                    parentContainer.ChildMediaContainers.Single(rel => rel.ChildMediaContainer == UnderlyingItem);

                if (relation.Id > 0)
                    dataContext.MediaContainersParentChilds.DeleteOnSubmit(relation);
                UnderlyingItem.ParentMediaContainers.Remove(relation);
                parentContainer.ChildMediaContainers.Remove(relation);
            }

            //� ������ ��������� ������� ��� ������� ���������� �� ����
            if (!UnderlyingItem.Parents.Any())
            {
                if (UnderlyingItem.Id > 0) 
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
            UnderlyingItem.ChildrenChanged += UnderlyingItem_ChildrenChanged;
            Parent = parent;

            var mediaContainerTreeWrappers =
                UnderlyingItem.Children.Select(
                    mc => new MediaContainerTreeWrapper(sourceIdDelegate, mc, this));
            ChildItems = new ObservableCollection<MediaContainerTreeWrapper>(mediaContainerTreeWrappers);
            ChildItems.CollectionChanged += ChildItems_CollectionChanged;
        }

        void ChildItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

        public IEnumerable<MediaContainerTreeWrapper> ParentsRecursive
        {
            get
            {
                var currentParent = Parent;
                while (currentParent!=null)
                {
                    yield return currentParent;
                    currentParent = currentParent.Parent;
                }
            }
        }

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
                if (filter == value) return;
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
        public MediaContainerTreeWrapper FindChild(MediaContainer target, MediaContainerTreeWrapper parent = null)
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
                            ?? childWrapper.FindChild(target, parent);
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
            if (child == null)
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

        void UnderlyingItem_ChildrenChanged(object sender, MediaListChangedEventArgs e)
        {
            if (ChildrenChanged != null)
                ChildrenChanged(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        internal event EventHandler<MediaListChangedEventArgs> ChildrenChanged;

        /// <summary>
        /// ����� ��� ��������� ������ �� ����������� ��������
        /// </summary>
        internal class MediaContainerTreeWrapperByUnderlyingItemComparer : IEqualityComparer<MediaContainerTreeWrapper>
        {
            public bool Equals(MediaContainerTreeWrapper x, MediaContainerTreeWrapper y)
            {
                if (x == y) return true;
                return GetHashCode(x)==GetHashCode(y);
            }

            public int GetHashCode(MediaContainerTreeWrapper obj)
            {
                return obj!=null && obj.UnderlyingItem != null ? obj.UnderlyingItem.GetHashCode() : 0;
            }
        }

        public ICommand CommandPlay
        {
            get { return commandPlay ?? (commandPlay = new ActionCommand(Play)); }
        }

        private void Play()
        {
            var mediaFiles = FindChildrenOfType<MediaFile>().Select(wrapper => (MediaFile)wrapper.UnderlyingItem);
            var adapter = context.ResolveUnregistered<WinampM3UPlaylistFileAdapter>();
            var filename = @"playlist.m3u";
            //Environment.CurrentDirectory
            adapter.MediaFilesToFile(filename, mediaFiles);
            winampControl.LoadPlaylist(filename);
            //Process.Start(filename);
        }

        public IEnumerable<MediaContainerTreeWrapper> FindChildrenOfType<T>() where T: MediaContainer
        {
            if(UnderlyingItem is T)  yield return this;
            foreach (var childItem in ChildItems)
                foreach (var subChildItem in childItem.FindChildrenOfType<T>())
                    yield return subChildItem;
        }
    }
}