using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.ViewModel;

namespace Oleg_ivo.MeloManager
{
    public interface IMediaContainerSourceUI
    {
        /// <summary>
        /// �������� ������
        /// </summary>
        MediaContainerTreeSource DataSource { get; set; }

        /// <summary>
        /// 
        /// </summary>
        MediaContainerTreeWrapper CurrentItem { get; set; }
    }
}