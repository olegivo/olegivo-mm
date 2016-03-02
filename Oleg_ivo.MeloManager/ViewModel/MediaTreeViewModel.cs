using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Autofac;
using GalaSoft.MvvmLight.Command;
using NLog;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.WPF.Dialogs;
using Oleg_ivo.Base.WPF.Extensions;
using Oleg_ivo.Base.WPF.ViewModels;
using Oleg_ivo.MeloManager.Dialogs;
using Oleg_ivo.MeloManager.Dialogs.ParentsChildsEdit;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff;
using Oleg_ivo.MeloManager.Winamp;
using Reactive.Bindings;

namespace Oleg_ivo.MeloManager.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class MediaTreeViewModel : ViewModelBase
    {
        private readonly IComponentContext context;

        #region Fields
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        private ObservableCollection<MediaContainerTreeWrapper> items;
        private ObservableCollection<MediaContainer> childListDataSource;
        private ObservableCollection<MediaContainer> parentListDataSource;

        #endregion

        #region Properties
        public IMediaRepository MediaRepository { get; set; }

        [Dependency(Required = true)]
        public IModalDialogService ModalDialogService { get; set; }

        public ObservableCollection<MediaContainerTreeWrapper> Items
        {
            get { return items; }
            set
            {
                if (items == value) return;

                items = value;
                RaisePropertyChanged("Items");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MediaContainerTreeWrapper CurrentItem
        {
            get { return CurrentWrapper.Value; }
            set
            {
                if (CurrentItem == value) return;
                //TODO: история переходов
                CurrentWrapper.Value = value;
                RaisePropertyChanged("CurrentItem");
                RaisePropertyChanged("CurrentTreeMediaContainer");
                ChildListDataSource = new ObservableCollection<MediaContainer>(value.UnderlyingItem.ChildContainers);
                ParentListDataSource = new ObservableCollection<MediaContainer>(value.UnderlyingItem.ParentContainers);
            }
        }

        /// <summary>
        /// Текущий медиа-контейнер
        /// </summary>
        public MediaContainer CurrentTreeMediaContainer
        {
            get { return CurrentContainer.Value; }
        }

        /// <summary>
        /// Источник данных для списка родителей текущего медиа-контейнера
        /// </summary>
        public ObservableCollection<MediaContainer> ParentListDataSource
        {
            get { return parentListDataSource; }
            set
            {
                if (parentListDataSource == value) return;
                parentListDataSource = value;
                RaisePropertyChanged("ParentListDataSource");
                OnParentListDataSourceChanged();
            }
        }

        public string NameFilter
        {
            get { return nameFilter; }
            set
            {
                if (nameFilter == value) return;
                nameFilter = value;
                Predicate<object> filter;
                if (string.IsNullOrEmpty(NameFilter))
                    filter = null;
                else
                {
                    var lowerInvariant = NameFilter.ToLowerInvariant();
                    filter = RecursivePredicate(wrapper => wrapper.Name.ToLowerInvariant().Contains(lowerInvariant));
                }

                var view = CollectionViewSource.GetDefaultView(Items);
                view.Filter = filter;
                foreach (MediaContainerTreeWrapper wrapper in view)
                    wrapper.Filter = filter;
                RaisePropertyChanged("NameFilter");
            }
        }

        private Predicate<object> RecursivePredicate(Predicate<MediaContainerTreeWrapper> predicate)
        {
            return o =>
            {
                var wrapper = (MediaContainerTreeWrapper) o;
                var result = predicate(wrapper) 
                    || wrapper.ChildItems.Any(child => RecursivePredicate(predicate)(child));
                return result;
            };
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
        public ObservableCollection<MediaContainer> ChildListDataSource
        {
            get { return childListDataSource; }
            set
            {
                if (childListDataSource == value) return;
                childListDataSource = value;
                RaisePropertyChanged("ChildListDataSource");
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

        public ICommand CommandDeleteItem { get; private set; }
        public ICommand CommandEditParents { get; private set; }
        public ICommand CommandEditChildren { get; private set; }
        public ReactiveCommand CommandDeleteCurrent { get; private set; }
        public ReactiveCommand CommandAddCategoryToCurrent { get; private set; }
        public ReactiveCommand CommandAddPlaylistToCurrent { get; private set; }
        public ReactiveCommand CommandAddMediaFileToCurrent { get; private set; }

        private void InitCommands()
        {
            CommandDeleteItem = new RelayCommand<MediaContainerTreeWrapper>(DeleteItem);
            CommandEditParents = new RelayCommand<MediaContainerTreeWrapper>(EditParents);
            CommandEditChildren = new RelayCommand<MediaContainerTreeWrapper>(EditChildren);
            
            CommandDeleteCurrent = new ReactiveCommand(CurrentContainer.Select(c => c != null), false).AddHandler(() => DeleteItem(CurrentWrapper.Value));

            CommandAddCategoryToCurrent = new ReactiveCommand(CurrentContainer.Select(c => c == null || c is Category)).AddHandler(() => AddCategory(CurrentWrapper.Value));
            CommandAddPlaylistToCurrent = new ReactiveCommand(CurrentContainer.Select(c => c is Category), false).AddHandler(() => AddPlaylist(CurrentWrapper.Value));
            CommandAddMediaFileToCurrent = new ReactiveCommand(CurrentContainer.Select(c => c is Category || c is Playlist), false).AddHandler(() => AddMediaFile(CurrentWrapper.Value));
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
                    mediaContainers.Select(mc => new MediaContainerTreeWrapper(mc, null, context, winampControl)));
        }

        public void AddMediaContainer(MediaContainer mediaContainer, MediaContainerTreeWrapper parent)
        {
            var wrapper = new MediaContainerTreeWrapper(mediaContainer, parent, context, winampControl);
            if (parent == null)
                Items.Add(wrapper);
            else
                parent.ChildItems.Add(wrapper);
            
            if (mediaContainer.Id == 0)
                MediaRepository.MediaContainers.Add(mediaContainer);
            CurrentItem = wrapper;
        }

        public event EventHandler<DeletingEventArgs<List<MediaContainerTreeWrapper>>> Deleting;

        private void DeleteItem(MediaContainerTreeWrapper wrapper)
        {
            DeleteItem(wrapper, false);
        }

        public void DeleteItem(MediaContainerTreeWrapper wrapper, bool silent)
        {
            if (!silent && Deleting != null)
            {
                var eventArgs = new DeletingEventArgs<List<MediaContainerTreeWrapper>>(new List<MediaContainerTreeWrapper> {wrapper});
                Deleting(this, eventArgs);
                if (eventArgs.Cancel)
                    return;
            }

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
            foundWrappers.First().DeleteWithChildren(MediaRepository);
            //удаление из родительскитх обёрток
            foreach (var foundWrapper in foundWrappers)
            {
                if (foundWrapper.Parent == null)
                    Items.Remove(foundWrapper);
                else
                    foundWrapper.Parent.ChildItems.Remove(foundWrapper);
            }
        }

        private void AddCategory(MediaContainerTreeWrapper parent)
        {
            ModalDialogService.CreateAndShowDialog<SimpleStringDialogViewModel>(
                modalWindow =>
                {
                    modalWindow.ViewModel.Caption = "Добавление категории";
                    modalWindow.ViewModel.ContentViewModel.Description = "Введите название";
                    modalWindow.ViewModel.ContentViewModel.Value = "Новая категория";
                },
                (model, dialogResult) =>
                {
                    if (dialogResult.HasValue && dialogResult.Value)
                    {
                        var category = new Category { Name = model.ContentViewModel.Value };
                        AddMediaContainer(category, parent);
                    }
                });
        }

        private void AddPlaylist(MediaContainerTreeWrapper parent)
        {
            ModalDialogService.CreateAndShowDialog<SimpleStringDialogViewModel>(
                modalWindow =>
                {
                    modalWindow.ViewModel.Caption = "Добавление плейлиста";
                    modalWindow.ViewModel.ContentViewModel.Description = "Введите название";
                    modalWindow.ViewModel.ContentViewModel.Value = "Новый плейлист";
                },
                (model, dialogResult) =>
                {
                    if (dialogResult.HasValue && dialogResult.Value)
                    {
                        var playlist = new Playlist { Name = model.ContentViewModel.Value };
                        AddMediaContainer(playlist, parent);
                    }
                });
        }

        private void AddMediaFile(MediaContainerTreeWrapper parent)
        {
            ModalDialogService.CreateAndShowDialog<SimpleStringDialogViewModel>(
                modalWindow =>
                {
                    modalWindow.ViewModel.Caption = "Добавление медиа-файла";
                    modalWindow.ViewModel.ContentViewModel.Description = "Введите название";
                    modalWindow.ViewModel.ContentViewModel.Value = "Новый меди-файл";
                },
                (model, dialogResult) =>
                {
                    if (dialogResult.HasValue && dialogResult.Value)
                    {
                        var playlist = new MediaFile { Name = model.ContentViewModel.Value };
                        AddMediaContainer(playlist, parent);
                    }
                });
        }

        private void EditParents(MediaContainerTreeWrapper item)
        {
            var underlyingItem = item.UnderlyingItem;
            var parentContainers = underlyingItem.ParentContainers;
            var mediaContainers = MediaRepository.MediaContainers
                .AsEnumerable()
                .Where(
                    mc =>
                        mc != underlyingItem &&
                        (mc is Category || (underlyingItem is MediaFile && mc is Playlist)))
                .ToList();
            if(!mediaContainers.Any()) return;

            ModalDialogService.CreateAndShowDialog<ParentsChildsEditDialogViewModel>(
                modalWindow =>
                {
                    modalWindow.Width = 400;
                    modalWindow.Height = 800;
                    modalWindow.ViewModel.Caption = "Редактирование родительских элементов";

                    modalWindow.ViewModel.ContentViewModel.Items =
                        new ObservableCollection<SelectableItem<MediaContainer>>(
                            mediaContainers.Select(
                                mc => new SelectableItem<MediaContainer>(mc, parentContainers.Contains(mc))));
                },
                (model, dialogResult) =>
                {
                    if (dialogResult.HasValue && dialogResult.Value)
                    {
                        var newParents = model.ContentViewModel.SelectedItems.ToList();
                        var diffAction = GetUpdateElementsDiffAction(underlyingItem, false, parentContainers, newParents);
                        if (diffAction.DiffType != DiffType.None &&
                            ShowDiff(diffAction.ChildDiffActions.Cast<ItemDiffAction<MediaContainer>>()))
                        {
                            diffAction.Apply();
                        }
                    }
                });
        }

        private void EditChildren(MediaContainerTreeWrapper item)
        {
            var underlyingItem = item.UnderlyingItem;
            if(underlyingItem is MediaFile) return;
            var childContainers = underlyingItem.ChildContainers;
            var mediaContainers = MediaRepository.MediaContainers
                .AsEnumerable()
                .Where(
                    mc =>
                        mc != underlyingItem &&
                        (underlyingItem is Category || (underlyingItem is Playlist && mc is MediaFile)))
                .ToList();
            if(!mediaContainers.Any()) return;

            ModalDialogService.CreateAndShowDialog<ParentsChildsEditDialogViewModel>(
                modalWindow =>
                {
                    modalWindow.Width = 400;
                    modalWindow.Height = 800;
                    modalWindow.ViewModel.Caption = "Редактирование дочерних элементов";

                    modalWindow.ViewModel.ContentViewModel.Items =
                        new ObservableCollection<SelectableItem<MediaContainer>>(
                            mediaContainers.Select(
                                mc => new SelectableItem<MediaContainer>(mc, childContainers.Contains(mc))));
                },
                (model, dialogResult) =>
                {
                    if (dialogResult.HasValue && dialogResult.Value)
                    {
                        var newChildren = model.ContentViewModel.SelectedItems.ToList();
                        var diffAction = GetUpdateElementsDiffAction(underlyingItem, true, childContainers, newChildren);
                        if (diffAction.DiffType != DiffType.None &&
                            ShowDiff(diffAction.ChildDiffActions.Cast<ItemDiffAction<MediaContainer>>()))
                        {
                            diffAction.Apply();
                        }
                    }
                });
        }

        private bool ShowDiff(IEnumerable<ItemDiffAction<MediaContainer>> diffActions)
        {
            var dialogResult = ModalDialogService.CreateAndShowDialog<DiffResultDialogViewModel>(
                modalWindow =>
                {
                    modalWindow.Width = 400;
                    modalWindow.Height = 400;
                    modalWindow.ViewModel.Caption = "Результирующая разница";
                    modalWindow.ViewModel.ContentViewModel.Items =
                        diffActions.Where(action => action.DiffType != DiffType.None)
                            .Cast<IDiffAction>()
                            .OrderBy(action => action.DiffType)
                            .ThenBy(action => action.ToString())
                            .ToList();
                });
            return dialogResult.HasValue && dialogResult.Value;
        }

        private ContainerDiffAction<MediaContainer> GetUpdateElementsDiffAction(MediaContainer target, bool isChild, IEnumerable<MediaContainer> oldVersion, IEnumerable<MediaContainer> newVersion)
        {
            Func<MediaContainer, IDiffAction> onAdd = addedItem => new DiffAction<MediaContainer, MediaContainer>(() => target,
                () => addedItem,
                (mediaContainer, item) =>
                {
                    (isChild ? mediaContainer.ChildContainers : mediaContainer.ParentContainers).Add(item);
                    log.Debug("Добавлен: {0}", item);
                }, DiffType.Added);

            Func<MediaContainer, IDiffAction> onDelete = deletedItem => new DiffAction<MediaContainer, MediaContainer>(() => target,
                () => deletedItem,
                (mediaContainer, item) =>
                {
                    if (isChild)
                    {
                        MediaRepository.RemoveRelation(target, item);
                        MediaRepository.RemoveIfOrphan(item);
                    }
                    else
                    {
                        MediaRepository.RemoveRelation(item, target);
                    }
                    log.Debug("Удалён: {0}", item);
                }, DiffType.Deleted);
            Func<MediaContainer, MediaContainer, IDiffAction> onEquals = (item1, item2) =>
                new DiffAction<MediaContainer, MediaContainer>(() => target,
                    () => item1,
                    (mediaContainer, item) =>
                    {
                        log.Trace("Без изменения: {0}", item);
                    }, DiffType.None);

            //compare old and new version
            var collectionDiff = DiffCreator.CreateCollectionDiff(oldVersion, newVersion, onAdd, onDelete, onEquals);

            return new ContainerDiffAction<MediaContainer>(() => target, collectionDiff, DiffType.Modified)
            {
                PreAction = () => log.Debug("Обновление элементов {0}", target),
                PostAction = () => log.Debug("Обновление элементов {0} завершено", target),
            };
        }

        #endregion

        #region переработать!
        private MediaContainerTreeSource treeDataSource;
        private string nameFilter;
        private readonly WinampControl winampControl;

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
                RaisePropertyChanged("TreeDataSource");
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

        public MediaTreeViewModel(IComponentContext context, WinampControl winampControl)
        {
            this.context = Enforce.ArgumentNotNull(context, "context");
            this.winampControl = Enforce.ArgumentNotNull(winampControl, "winampControl");
            items = new ObservableCollection<MediaContainerTreeWrapper>();
            items.CollectionChanged += items_CollectionChanged;
            CurrentWrapper = new ReactiveProperty<MediaContainerTreeWrapper>();
            CurrentContainer = CurrentWrapper.Select(wrapper => wrapper != null ? wrapper.UnderlyingItem : null).ToReactiveProperty();
            Disposer.Add(new CompositeDisposable(
                CurrentWrapper.Subscribe(w=>log.Debug("Текущая обёртка: {0}", w)),
                CurrentContainer.Subscribe(c => log.Debug("Текущий контейнер: {0}", c))
                ));
            InitCommands();
        }

        public ReactiveProperty<MediaContainer> CurrentContainer { get; private set; }

        public ReactiveProperty<MediaContainerTreeWrapper> CurrentWrapper { get; private set; }

        void items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Items");
        }

    }
}