using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NLog;
using Oleg_ivo.Base.WPF.ViewModels;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.Search;
using Reactive.Bindings;
using ReactiveUI;

namespace Oleg_ivo.MeloManager.Dialogs.ParentsChildsEdit
{
    public class ParentsChildsEditDialogViewModel : ViewModelBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private ObservableCollection<SelectableItem<MediaContainer>> value;

        public ParentsChildsEditDialogViewModel()
        {
            Disposer.Add(Filter = new ReactiveProperty<string>());
            Disposer.Add(SelectedFilter = new ReactiveProperty<string>());
            Disposer.Add(AvailableFilter = new ReactiveProperty<string>());
        }

        public ObservableCollection<SelectableItem<MediaContainer>> Items
        {
            get { return value; }
            set
            {
                if(this.value == value) return;
                this.value = value;

                Func<SelectableItem<MediaContainer>, string, bool> filterName = (item, filter) =>
                    (string.IsNullOrWhiteSpace(filter) ||
                     item.Item.Name.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >=
                     0);

                var selectableItems = new ReactiveList<SelectableItem<MediaContainer>>(value) {ChangeTrackingEnabled = true};
                //Disposer.Add(selectableItems.ItemChanged.Subscribe(item => log.Debug("{0} changed", item.PropertyName)));
                SelectedSearchViewModel = new SelectableSearchViewModel<MediaContainer>(
                    selectableItems, 
                    item => item.IsSelected, filterName, SelectedFilter);
                AvailableSearchViewModel = new SelectableSearchViewModel<MediaContainer>(
                    selectableItems,
                    item => !item.IsSelected, filterName, AvailableFilter);
            }
        }

        public SelectableSearchViewModel<MediaContainer> SelectedSearchViewModel { get; set; }
        public SelectableSearchViewModel<MediaContainer> AvailableSearchViewModel { get; set; }

        public IEnumerable<MediaContainer> SelectedItems
        {
            get { return SelectedSearchViewModel.InnerSearchViewModel.SearchResults.Value.Select(item => item.Item); }
        }

        public ReactiveProperty<string> Filter { get; private set; }
        public ReactiveProperty<string> SelectedFilter { get; private set; }
        public ReactiveProperty<string> AvailableFilter { get; private set; }
    }
}