using System.Linq;
using GalaSoft.MvvmLight;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private MediaContainer _currentTreeMediaContainer;

        public string Welcome
        {
            get
            {
                return "Welcome to MVVM Light";
            }
        }

        private MediaContainerTreeSource _treeDataSource;
        private MediaContainerTreeWrapper _currentItem;
        private IQueryable<MediaContainer> _ChildListDataSource;
        private IQueryable<MediaContainer> _ParentListDataSource;

        /// <summary>
        /// 
        /// </summary>
        public MediaContainerTreeWrapper CurrentItem
        {
            get { return _currentItem; }
            set
            {
                if (_currentItem == value) return;
                _currentItem = value;
                RaisePropertyChanged("CurrentItem");
            }
        }

        /// <summary>
        /// Источник данных для дерева
        /// </summary>
        public MediaContainerTreeSource TreeDataSource
        {
            get { return _treeDataSource; }
            set
            {
                if (_treeDataSource == value) return;
                _treeDataSource = value;
                RaisePropertyChanged("TreeDataSource");
            }
        }

        /// <summary>
        /// Источник данных для списка родителей текущего медиа-контейнера
        /// </summary>
        public IQueryable<MediaContainer> ParentListDataSource
        {
            get { return _ParentListDataSource; }
            set
            {
                if (_ParentListDataSource == value) return;
                _ParentListDataSource = value;
                RaisePropertyChanged("ParentListDataSource");
            }
        }

        /// <summary>
        /// Источник данных для списка детей текущего медиа-контейнера
        /// </summary>
        public IQueryable<MediaContainer> ChildListDataSource
        {
            get { return _ChildListDataSource; }
            set
            {
                if (_ChildListDataSource == value) return;
                _ChildListDataSource = value;
                RaisePropertyChanged("ChildListDataSource");
            }
        }

        /// <summary>
        /// Текущий медиа-контейнер
        /// </summary>
        public MediaContainer CurrentTreeMediaContainer
        {
            get
            {
                return _currentTreeMediaContainer;
            }
            set
            {
                if(_currentTreeMediaContainer==value) return;

                _currentTreeMediaContainer = value;
                ChildListDataSource = CurrentTreeMediaContainer.Childs;
                ParentListDataSource = CurrentTreeMediaContainer.Parents;
                RaisePropertyChanged("CurrentTreeMediaContainer");
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            /*
                        if (IsInDesignMode)
                        {
                            // Code runs in Blend --> create design time data.
                        }
                        else
                        {
                            // Code runs "for real"
                        }
            */
        }

        /*
                public override void Cleanup()
                {
                    // Clean up if needed

                    base.Cleanup();
                }
        */
    }
}