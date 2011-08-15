using System.Windows.Controls;

namespace Oleg_ivo.MeloManager
{
    /// <summary>
    /// Interaction logic for MediaTree.xaml
    /// </summary>
    public partial class MediaTree : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public MediaTree()
        {
            InitializeComponent();

            InitDatasource();
        }

        private void InitDatasource()
        {
            var ds = new[]
                         {
                             new {Name = "Плейлисты", ID = 0, ParentID = 0},
                                new {Name = "Плейлист1", ID = 1, ParentID = 0}, 
                                    new {Name = "Файл11", ID = 11, ParentID = 1}, 
                                    new {Name = "Файл12", ID = 12, ParentID = 1},
                                new {Name = "Плейлист2", ID = 2, ParentID = 0}, 
                                    new {Name = "Файл21", ID = 21, ParentID = 2}, 
                                    new {Name = "Файл22", ID = 22, ParentID = 2}
                         };

            //tree.Columns.AddField("Name");
            tree.DataSource = ds;
            tree.ExpandAll();}
    }
}
