using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// Обёртка для <see cref="MediaContainer"/>
    /// </summary>
    public class MediaContainerTreeWrapper : INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public delegate long getMyTreeSourceIdDelegate(MediaContainerTreeWrapper key);

        private readonly getMyTreeSourceIdDelegate _getMySourceIdDelegateId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceIdDelegate"></param>
        /// <param name="underlyingItem"></param>
        /// <param name="parent"></param>
        public MediaContainerTreeWrapper(getMyTreeSourceIdDelegate sourceIdDelegate, MediaContainer underlyingItem, MediaContainerTreeWrapper parent)
        {
            if (sourceIdDelegate == null) throw new ArgumentNullException("sourceIdDelegate");
            _getMySourceIdDelegateId = sourceIdDelegate;
            if (underlyingItem == null) throw new ArgumentNullException("underlyingItem", "Обёртка не может быть пустой");
            UnderlyingItem = underlyingItem;
            UnderlyingItem.ChildsChanged += UnderlyingItem_ChildsChanged;
            Parent = parent;
        }

        /// <summary>
        /// 
        /// </summary>
        public MediaContainer UnderlyingItem { get; private set; }

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

        void UnderlyingItem_ChildsChanged(object sender, MediaListChangedEventArgs e)
        {
            if (ChildsChanged != null)
                ChildsChanged(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        internal event EventHandler<MediaListChangedEventArgs> ChildsChanged;

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

        /// <summary>
        /// 
        /// </summary>
        public BitmapImage Image
        {
            get { return ImageResourceFactory.GetImage(UnderlyingItem.GetType()); }
        }
    }
}