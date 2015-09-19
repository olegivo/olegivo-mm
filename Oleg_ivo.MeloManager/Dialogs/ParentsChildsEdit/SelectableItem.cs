using System;
using System.Diagnostics;
using System.Windows.Input;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.WPF.ViewModels;
using Reactive.Bindings;

namespace Oleg_ivo.MeloManager.Dialogs.ParentsChildsEdit
{
    [DebuggerDisplay(@"{IsSelected ? ""+"" : ""-""} {Item}")]
    public class SelectableItem<TItem> : ViewModelBase where TItem : class
    {
        private bool isSelected;

        public SelectableItem(TItem item, bool isSelected = false)
        {
            Item = Enforce.ArgumentNotNull(item, "item");
            this.isSelected = isSelected;
            var commandSwitchIsSelected = new ReactiveCommand<SelectableItem<TItem>>();
            CommandSwitchIsSelected = commandSwitchIsSelected;
            Disposer.Add(commandSwitchIsSelected);
            Disposer.Add(commandSwitchIsSelected.Subscribe(si => si.IsSelected = !si.IsSelected));
        }

        public ICommand CommandSwitchIsSelected { get; private set; }

        public TItem Item { get; private set; }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if(isSelected == value) return;
                isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }
    }
}