using System.Windows;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.WPF.Dialogs;

namespace Oleg_ivo.MeloManager.Dialogs
{
    /// <summary>
    /// Interaction logic for SimpleStringDialog.xaml
    /// </summary>
    public partial class SimpleStringDialog : IModalWindow<SimpleStringDialogViewModel>
    {
        public SimpleStringDialog()
        {
            InitializeComponent();
        }

        [Dependency(Required = true)]
        public SimpleStringDialogViewModel ViewModel
        {
            get { return (SimpleStringDialogViewModel) DataContext; }
            set { DataContext = value; }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

    }
}
