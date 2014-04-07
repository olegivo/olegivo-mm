using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private ICommand commandTreeAddCategory;
        private MediaTreeViewModel mediaTree;
        private MediaListViewModel parents;
        private MediaListViewModel childs;

        #endregion
        
        [Dependency(Required = true)]
        public MediaTreeViewModel MediaTree
        {
            get { return mediaTree; }
            set
            {
                if (mediaTree == value) return;
                if (MediaTree != null)
                {
                    MediaTree.ParentListDataSourceChanged -= MediaTree_ParentListDataSourceChanged;
                    MediaTree.ChildListDataSourceChanged -= MediaTree_ChildListDataSourceChanged;
                }
                mediaTree = value;
                if (MediaTree != null)
                {
                    MediaTree.ParentListDataSourceChanged += MediaTree_ParentListDataSourceChanged;
                    MediaTree.ChildListDataSourceChanged += MediaTree_ChildListDataSourceChanged;
                }
                RaisePropertyChanged(() => MediaTree);
            }
        }

        void MediaTree_ParentListDataSourceChanged(object sender, System.EventArgs e)
        {
            Parents.ListDataSource = MediaTree.ParentListDataSource;
        }

        void MediaTree_ChildListDataSourceChanged(object sender, System.EventArgs e)
        {
            Childs.ListDataSource = MediaTree.ChildListDataSource;
        }

        [Dependency(Required = true)]
        public MediaListViewModel Parents
        {
            get { return parents; }
            set
            {
                if(parents == value) return;
                parents = value;
                RaisePropertyChanged(() => Parents);
            }
        }

        [Dependency(Required = true)]
        public MediaListViewModel Childs
        {
            get { return childs; }
            set
            {
                if(childs == value) return;
                childs = value;
                RaisePropertyChanged(() => Childs);
            }
        }

        public ICommand CommandTreeAddCategory
        {
            get
            { //TODO
                return commandTreeAddCategory ??
                       (commandTreeAddCategory = new RelayCommand<object>(TreeAddCategory));
            }
        }

        private void TreeAddCategory(object mediaTree)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }

        public void InitDataSource()
        {
            var f1 = new MediaFile { Name = "Файл 1" };
            var f2 = new MediaFile { Name = "Файл 2" };
            var f3 = new MediaFile { Name = "Файл 3" };
            var f4 = new MediaFile { Name = "Файл 4" };
            var f5 = new MediaFile { Name = "Файл 4" };

            var p1 = new Playlist { Name = "Плейлист 1" };
            p1.AddChildMediaFile(f1);
            p1.AddChildMediaFile(f2);
            var p2 = new Playlist { Name = "Плейлист 2" };
            p2.AddChildMediaFile(f2);
            p2.AddChildMediaFile(f3);
            var p3 = new Playlist { Name = "Плейлист 3" };
            p3.AddChildMediaFile(f3);
            p3.AddChildMediaFile(f4);
            p3.AddChildMediaFile(f5);

            var c1 = new Category { Name = "Категория 1" };
            c1.AddChild(p1);
            var c2 = new Category { Name = "Категория 2" };
            c2.AddChild(p2);
            var c3 = new Category { Name = "Категория 3" };
            c3.AddChild(p2);
            c3.AddChild(p3);

            c1.AddChild(c2);

            MediaTree.AddCategory(c1, null);
            MediaTree.AddCategory(c3, null);
        }

        public void LoadFromDb()
        {
            //throw new System.NotImplementedException();
        }

        public void SaveAndLoad()
        {
            throw new System.NotImplementedException();
        }
    }
}