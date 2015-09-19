using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Oleg_ivo.Base.WPF.ViewModels;
using Oleg_ivo.MeloManager.Dialogs.ParentsChildsEdit;
using Reactive.Bindings;
using ReactiveUI;

namespace Oleg_ivo.MeloManager.Search
{
    public class SearchViewModel<TItem> : ViewModelBase where TItem : class, INotifyPropertyChanged
    {
        protected readonly ReactiveList<TItem> SourceItems;
        private readonly Func<TItem, string, bool> searchPredicate;
        private readonly ReactiveProperty<IList<TItem>> externalSource;
        public ReactiveProperty<IList<TItem>> SearchResults { get; private set; }
        public ReactiveProperty<string> QueryText { get; private set; }

        public CollectionViewSource ResultsViewSource { get; private set; }

        public SearchViewModel(ReactiveList<TItem> sourceItems, Func<TItem, string, bool> searchPredicate,
            ReactiveProperty<string> queryText = null, ReactiveProperty<IList<TItem>> externalSource = null)
        {
            SourceItems = sourceItems;
            this.searchPredicate = searchPredicate;
            this.externalSource = externalSource;
            ResultsViewSource = new CollectionViewSource();
            if (queryText != null) 
                QueryText = queryText;
            else 
                Disposer.Add(QueryText = new ReactiveProperty<string>());
            
            Disposer.Add(SearchResults = new ReactiveProperty<IList<TItem>>());
            var combineLatest = this.externalSource != null
                ? QueryText.CombineLatest(this.externalSource.StartWith((IList<TItem>) null), (query, item) => query)
                : QueryText.CombineLatest(SourceItems.ItemChanged.StartWith((IObservedChange<TItem, object>) null),
                    (query, item) => query).Throttle(query => Observable.Timer(GetThrottleTime(query)));
            Disposer.Add(combineLatest
                .Select(query => Observable.FromAsync(() => QueryAsync(query, this.externalSource!=null ? this.externalSource.Value : SourceItems)))
                .Concat()
                .ObserveOnDispatcher()
                .Subscribe(SetSearchResults));
        }

        private TimeSpan GetThrottleTime(string query)
        {
            if (string.IsNullOrEmpty(query)) return TimeSpan.Zero;
            //if (query.Length < 3) return TimeSpan.FromSeconds(1.5);
            return TimeSpan.FromSeconds(0.4);
        }

        private bool isWait;
        private TItem currentItem;

        public bool IsWait
        {
            get { return isWait; }
            set
            {
                if (isWait == value) return;
                isWait = value;
                RaisePropertyChanged("IsWait");
            }
        }

        public TItem CurrentItem
        {
            get { return currentItem; }
            set
            {
                if (Equals(currentItem, value)) return;
                currentItem = value;
                RaisePropertyChanged("CurrentItem");
                // log.Trace("CurrentItem changed: {0}, selected action: {1}", currentItem != null ? currentItem.Entity : "null", currentItem != null ? currentItem.SelectedAction : null);
            }
        }

        protected Task<IList<TItem>> QueryAsync(string query, IEnumerable<TItem> source)
        {
            return Task.Factory.StartNew(() =>
            {
                IsWait = true;
                IList<TItem> items = source!=null ? source.Where(item => searchPredicate(item, query)).ToList() : null;
                return items;
            });
        }

        protected void SetSearchResults(IList<TItem> searchResults)
        {
            IsWait = false;
            ResultsViewSource.Source = SearchResults.Value = searchResults;
        }

    }

    public class SelectableSearchViewModel<TItem> : SearchViewModel<SelectableItem<TItem>> where TItem : class //TODO: вложенность вместо наследования
    {
        public SearchViewModel<SelectableItem<TItem>> InnerSearchViewModel { get; private set; }

        public SelectableSearchViewModel(ReactiveList<SelectableItem<TItem>> sourceItems,
            Func<SelectableItem<TItem>, bool> selectablePredicate,
            Func<SelectableItem<TItem>, string, bool> searchPredicate, ReactiveProperty<string> queryText = null)
            : base(sourceItems, (item, _) => selectablePredicate == null || selectablePredicate(item), queryText)
        {
            Disposer.Add(
                InnerSearchViewModel =
                    new SearchViewModel<SelectableItem<TItem>>(sourceItems, searchPredicate,
                        externalSource: SearchResults, queryText: queryText));
        }

    }
/*
    public class SelectableSearchViewModel2<TItem> : SearchViewModel<SelectableItem<TItem>> where TItem : class //TODO: вложенность вместо наследования
    {
        private readonly Func<SelectableItem<TItem>, bool> selectablePredicate;

        public SelectableSearchViewModel2(ReactiveList<SelectableItem<TItem>> sourceItems,
            Func<SelectableItem<TItem>, string, bool> searchPredicate, ReactiveProperty<string> queryText = null,
            Func<SelectableItem<TItem>, bool> selectablePredicate = null)
            : base(sourceItems, searchPredicate, queryText)
        {
            this.selectablePredicate = selectablePredicate;
        }

        private Task<List<SelectableItem<TItem>>> SelectableQueryAsync(IEnumerable<SelectableItem<TItem>> source)
        {
            return Task.Factory.StartNew(() =>
            {
                IsWait = true;
                List<SelectableItem<TItem>> items = source.Where(item => selectablePredicate(item)).ToList();
                return items;
            });
        }


        protected override IDisposable SubscribeFilters(IObservable<string> filter)
        {
            if (selectablePredicate == null) return base.SubscribeFilters(filter);
            return filter
                .Select(
                    query =>
                        Observable.FromAsync(
                            () =>
                            {
                                var selectableQueryTask = SelectableQueryAsync(SourceItems);
                                return 
                                    selectableQueryTask
                                    .ContinueWith(task => QueryAsync(query, task.Result))
                                    .ContinueWith(task => new {SelectedResult = selectableQueryTask.Result, FinalResult = task.Unwrap().Result});
                            }))
                .Concat()
                .ObserveOnDispatcher()
                .Subscribe(obj =>
                {
                    SetSelecteableResults(obj.SelectedResult);
                    SetSearchResults(obj.FinalResult);
                });
        }

        private void SetSelecteableResults(List<SelectableItem<TItem>> selectableItems)
        {
            SelectableResults = selectableItems;
            RaisePropertyChanged(() => SelectableResults);
        }

        public List<SelectableItem<TItem>> SelectableResults { get; set; }
    }
*/
}
