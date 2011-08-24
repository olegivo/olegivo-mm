using System.ComponentModel;
using System.Data.Linq;
using System.Windows.Controls;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.MediaList
{
    /// <summary>
    /// Interaction logic for MediaList.xaml
    /// </summary>
    public partial class MediaList : UserControl, INotifyPropertyChanged
    {
        private EntitySet<MediaContainersParentChild> _DataSource;

        public MediaList()
        {
            InitializeComponent();
        }

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
        public EntitySet<MediaContainersParentChild> DataSource
        {
            get { return _DataSource; }
            set
            {
                if (_DataSource == value) return;
                _DataSource = value;
                OnPropertyChanged("DataSource");
            }
        }
    }
}
