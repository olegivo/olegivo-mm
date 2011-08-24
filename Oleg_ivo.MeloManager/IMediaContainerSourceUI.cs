using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager
{
    public interface IMediaContainerSourceUI
    {
        /// <summary>
        /// Источник данных
        /// </summary>
        MediaContainerTreeSource DataSource { get; set; }

        /// <summary>
        /// 
        /// </summary>
        MediaContainerTreeWrapper CurrentItem { get; set; }
    }
}