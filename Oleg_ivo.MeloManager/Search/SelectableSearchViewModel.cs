using System;
using Oleg_ivo.MeloManager.Dialogs.ParentsChildsEdit;
using Reactive.Bindings;
using ReactiveUI;

namespace Oleg_ivo.MeloManager.Search
{
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
}