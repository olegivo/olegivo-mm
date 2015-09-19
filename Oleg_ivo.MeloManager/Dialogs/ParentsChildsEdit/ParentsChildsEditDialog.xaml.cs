using System;
using System.Reactive.Disposables;
using System.Windows;

namespace Oleg_ivo.MeloManager.Dialogs.ParentsChildsEdit
{
    /// <summary>
    /// Interaction logic for ParentsChildsEditDialog.xaml
    /// </summary>
    public partial class ParentsChildsEditDialog : IDisposable
    {
        private readonly CompositeDisposable disposer = new CompositeDisposable();
        public ParentsChildsEditDialog()
        {
            InitializeComponent();
            DataContextChanged += ParentsChildsEditDialog_DataContextChanged;
        }

        void ParentsChildsEditDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //disposer.Add(
            //    ViewModel.Filter.Throttle(TimeSpan.FromSeconds(0.5))
            //        .ObserveOnUIDispatcher().Subscribe(filter => CollectionViewSource.GetDefaultView(DataGrid.ItemsSource).Refresh()));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            disposer.Dispose();
        }
    }
}
