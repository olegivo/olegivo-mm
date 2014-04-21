using System;
using System.Collections.ObjectModel;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class MediaListViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<MediaContainer> listDataSource;
        private MediaContainer selectedItem;
        private string nameFilter;

        #endregion

        #region Properties
        public MediaContainer SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                RaisePropertyChanged(() => SelectedItem);
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
                    var lowerInvariant = (NameFilter ?? String.Empty).ToLowerInvariant();
                    filter = mc => ((MediaContainer) mc).Name.ToLowerInvariant().Contains(lowerInvariant);
                }

                var view = CollectionViewSource.GetDefaultView(ListDataSource);
                view.Filter = filter;
                RaisePropertyChanged(() => NameFilter);
            }
        }

        /// <summary>
        /// Источник данных для списка родителей текущего медиа-контейнера
        /// </summary>
        public ObservableCollection<MediaContainer> ListDataSource
        {
            get { return listDataSource; }
            set
            {
                if (listDataSource == value) return;
                listDataSource = value;
                NameFilter = null;
                RaisePropertyChanged(() => ListDataSource);
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

        public void OnCellDoubleClick()
        {
            
        }

        public event EventHandler RowDoubleClick;

        public void OnRowDoubleClick()
        {
            if(RowDoubleClick!=null) 
                RowDoubleClick(this, EventArgs.Empty);
        }
    }
}