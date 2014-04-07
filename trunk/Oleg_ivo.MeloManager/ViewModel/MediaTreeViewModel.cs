using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class MediaTreeViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<MediaContainerTreeWrapper> items;
        private MediaContainerTreeWrapper currentItem;
        private MediaContainer currentTreeMediaContainer;
        private IQueryable<MediaContainer> childListDataSource;
        private IQueryable<MediaContainer> parentListDataSource;
        private ICommand commandTreeAddCategory;
        #endregion

        #region Properties
        public ObservableCollection<MediaContainerTreeWrapper> Items
        {
            get { return items; }
            set
            {
                if (items == value) return;

                items = value;
                RaisePropertyChanged(() => Items);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MediaContainerTreeWrapper CurrentItem
        {
            get { return currentItem; }
            set
            {
                if (currentItem == value) return;
                currentItem = value;
                RaisePropertyChanged(() => CurrentItem);
                CurrentTreeMediaContainer = CurrentItem != null ? CurrentItem.UnderlyingItem : null;
            }
        }

        /// <summary>
        /// Текущий медиа-контейнер
        /// </summary>
        public MediaContainer CurrentTreeMediaContainer
        {
            get
            {
                return currentTreeMediaContainer;
            }
            set
            {
                if (currentTreeMediaContainer == value) return;

                currentTreeMediaContainer = value;
                ChildListDataSource = CurrentTreeMediaContainer.Childs;
                ParentListDataSource = CurrentTreeMediaContainer.Parents;
                RaisePropertyChanged(() => CurrentTreeMediaContainer);
            }
        }
        
        /// <summary>
        /// Источник данных для списка родителей текущего медиа-контейнера
        /// </summary>
        public IQueryable<MediaContainer> ParentListDataSource
        {
            get { return parentListDataSource; }
            set
            {
                if (parentListDataSource == value) return;
                parentListDataSource = value;
                RaisePropertyChanged(() => ParentListDataSource);
                OnParentListDataSourceChanged();
            }
        }

        private void OnParentListDataSourceChanged()
        {
            if (ParentListDataSourceChanged != null)
                ParentListDataSourceChanged(this, EventArgs.Empty);
        }

        public event EventHandler ParentListDataSourceChanged;//TODO: 2 binding?

        /// <summary>
        /// Источник данных для списка детей текущего медиа-контейнера
        /// </summary>
        public IQueryable<MediaContainer> ChildListDataSource
        {
            get { return childListDataSource; }
            set
            {
                if (childListDataSource == value) return;
                childListDataSource = value;
                RaisePropertyChanged(() => ChildListDataSource);
                OnChildListDataSourceChanged();
            }
        }

        private void OnChildListDataSourceChanged()
        {
            if (ChildListDataSourceChanged != null)
                ChildListDataSourceChanged(this, EventArgs.Empty);
        }

        public event EventHandler ChildListDataSourceChanged;//TODO: 2 binding?

        #endregion

        #region Commands
        public ICommand CommandTreeAddCategory
        {
            get
            { //TODO
                return commandTreeAddCategory ??
                       (commandTreeAddCategory = new RelayCommand<object>(TreeAddCategory));
            }
        }

        
        #endregion

        #region Methods
        public void Clear()
        {
            if (Items != null)
                Items.Clear();
        }

        public void AddCategory(Category category, MediaContainerTreeWrapper parent)
        {
            Items.Add(new MediaContainerTreeWrapper(category, parent));
        }

        private void TreeAddCategory(object mediaTree)
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region переработать!
        private MediaContainerTreeSource treeDataSource;

        /// <summary>
        /// Источник данных для дерева
        /// </summary>
        public MediaContainerTreeSource TreeDataSource
        {
            get { return treeDataSource; }
            set
            {
                if (treeDataSource == value) return;
                treeDataSource = value;
                RaisePropertyChanged(() => TreeDataSource);
            }
        }

        #endregion

        /*
        public override void Cleanup()
        {
            // Clean up if needed

            base.Cleanup();
        }
        */

        public MediaTreeViewModel()
        {
            items = new ObservableCollection<MediaContainerTreeWrapper>();
            items.CollectionChanged += items_CollectionChanged;
        }

        void items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(() => Items);
        }

    }
}