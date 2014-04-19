using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NLog;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class MediaTreeViewModel : ViewModelBase
    {
        #region Fields
        private static Logger log = LogManager.GetCurrentClassLogger();
        
        private ObservableCollection<MediaContainerTreeWrapper> items;
        private MediaContainerTreeWrapper currentItem;
        private MediaContainer currentTreeMediaContainer;
        private IQueryable<MediaContainer> childListDataSource;
        private IQueryable<MediaContainer> parentListDataSource;

        private ICommand commandTreeAddCategory;
        private ICommand commandDeleteItem;

        #endregion

        #region Properties
        public MediaDataContext DataContext { get; set; }

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
                //TODO: история переходов
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
            private set
            {
                if (currentTreeMediaContainer == value) return;

                currentTreeMediaContainer = value;
                ChildListDataSource = CurrentTreeMediaContainer!=null ? CurrentTreeMediaContainer.Childs : null;
                ParentListDataSource = CurrentTreeMediaContainer!=null ? CurrentTreeMediaContainer.Parents : null;
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
            {
                return commandTreeAddCategory ??
                       (commandTreeAddCategory = new RelayCommand<object>(TreeAddCategory));
            }
        }

        public ICommand CommandDeleteItem
        {
            get
            {
                return commandDeleteItem ??
                       (commandDeleteItem = new RelayCommand<MediaContainerTreeWrapper>(DeleteItem));
            }
        }

        #endregion

        #region Methods

        public class TreeWalker
        {

        }
        public void Clear()
        {
            //TODO:удаление различных элементов?
            /*foreach (var wrapper in Items)
            {
                
            }*/
            if (Items != null)
                Items.Clear();
        }

        public void InitSource(IEnumerable<Category> mediaContainers)
        {
            Items =
                new ObservableCollection<MediaContainerTreeWrapper>(
                    mediaContainers.Select(mc => new MediaContainerTreeWrapper(mc, null)));
        }

        public void AddCategory(Category category, MediaContainerTreeWrapper parent)
        {
            Items.Add(new MediaContainerTreeWrapper(category, parent));
            if (category.Id == 0)
                DataContext.MediaContainers.InsertOnSubmit(category);
        }

        public void DeleteItem(MediaContainerTreeWrapper wrapper)
        {
            var foundWrappers = new List<MediaContainerTreeWrapper>();
            if (wrapper.Parent != null)
            {
                var parentContainer = wrapper.Parent.UnderlyingItem;
                foundWrappers.AddRange(
                    Items.SelectMany(treeWrapper => treeWrapper.FindChildren(wrapper.UnderlyingItem, parentContainer)));
            }
            else
                foundWrappers.Add(wrapper);
            
            if (!foundWrappers.Any()) return;
            
            //удаление из базы
            foundWrappers.First().DeleteWithChildren(DataContext);
            //удаление из родительскитх обёрток
            foreach (var foundWrapper in foundWrappers)
            {
                if (foundWrapper.Parent == null)
                    Items.Remove(foundWrapper);
                else
                    foundWrapper.Parent.ChildItems.Remove(foundWrapper);
            }
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