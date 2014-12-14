using NLog;
using Oleg_ivo.Base.WPF.Dialogs;

namespace Oleg_ivo.MeloManager.Dialogs
{
    public class SimpleStringDialogViewModel : DialogViewModelBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private string value;
        private string description;

        public string Value
        {
            get { return value; }
            set
            {
                if(this.value == value) return;
                this.value = value;
                RaisePropertyChanged(() => Value);
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                if (description == value) return;
                description = value;
                RaisePropertyChanged(() => Description);
            }
        }
    }
}
