using Autofac;
using GalaSoft.MvvmLight;
using NLog;
using Oleg_ivo.Base.Autofac.DependencyInjection;

namespace Oleg_ivo.MeloManager.Dialogs
{
    public class SimpleStringDialogViewModel : ViewModelBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private string value;
        private string description;
        private string caption;

        [Dependency]
        public IComponentContext Context { get; set; }

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

        public string Caption
        {
            get { return caption; }
            set
            {
                if (caption == value) return;
                caption = value;
                RaisePropertyChanged(() => Caption);
            }
        }
    }
}
