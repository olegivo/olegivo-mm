using System.Collections.Generic;
using Oleg_ivo.Base.WPF.ViewModels;
using Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff;

namespace Oleg_ivo.MeloManager.Dialogs.ParentsChildsEdit
{
    public class DiffResultDialogViewModel : ViewModelBase
    {
        public IList<IDiffAction> Items { get; set; }
    }
}