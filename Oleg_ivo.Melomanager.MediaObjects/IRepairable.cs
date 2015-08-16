using System.Collections.Generic;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    public interface IRepairable
    {
        /// <summary>
        /// Починка медиа-контейнера (рекурсивно)
        /// </summary>
        /// <param name="foundFiles"></param>
        /// <param name="optionRepairOnlyBadFiles"></param>
        /// <param name="mediaCache"></param>
        void BatchRepair(IEnumerable<string> foundFiles, bool optionRepairOnlyBadFiles, IMediaCache mediaCache);

        bool IsRepaired { get; set; }
    }
}