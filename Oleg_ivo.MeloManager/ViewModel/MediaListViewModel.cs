using System.Linq;
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
        private IQueryable<MediaContainer> listDataSource;
        private MediaContainer selectedItem;

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

        /// <summary>
        /// Источник данных для списка родителей текущего медиа-контейнера
        /// </summary>
        public IQueryable<MediaContainer> ListDataSource
        {
            get { return listDataSource; }
            set
            {
                if (listDataSource == value) return;
                listDataSource = value;
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
    }
}