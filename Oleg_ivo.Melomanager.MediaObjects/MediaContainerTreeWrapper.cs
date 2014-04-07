using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media.Imaging;
using Oleg_ivo.Base.Autofac;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Обёртка для <see cref="MediaContainer"/>
    /// </summary>
    public class MediaContainerTreeWrapper : INotifyPropertyChanged
    {
        private readonly Func<MediaContainerTreeWrapper, long> _getMySourceIdDelegateId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="underlyingItem"></param>
        /// <param name="parent"></param>
        public MediaContainerTreeWrapper(MediaContainer underlyingItem, MediaContainerTreeWrapper parent)
        {
            //Обёртка не может быть пустой
            UnderlyingItem = Enforce.ArgumentNotNull(underlyingItem, "underlyingItem");
            UnderlyingItem.ChildsChanged += UnderlyingItem_ChildsChanged;
            Parent = parent;

            var mediaContainerTreeWrappers = UnderlyingItem.Childs.Select(mc => new MediaContainerTreeWrapper(mc, this));
            ChildItems = new ObservableCollection<MediaContainerTreeWrapper>(mediaContainerTreeWrappers);
            ChildItems.CollectionChanged += ChildItems_CollectionChanged;
        }

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
            //Обёртка не может быть пустой
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
        internal MediaContainerTreeWrapper Parent { get; set; }

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

        #endregion

        void UnderlyingItem_ChildsChanged(object sender, MediaListChangedEventArgs e)
        {
            if (ChildsChanged != null)
                ChildsChanged(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        internal event EventHandler<MediaListChangedEventArgs> ChildsChanged;

        #region INPC
        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }

        #endregion
    }
}