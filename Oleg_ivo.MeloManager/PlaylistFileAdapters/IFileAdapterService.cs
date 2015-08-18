using System.Collections.Generic;
using System.Threading.Tasks;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    public interface IFileAdapterService
    {
        void RequestImport(string importFilename);
        Task<bool> RequestImport(IEnumerable<string> playlistFilenames, Category winampCategory);
        void RequestExport(Playlist playlist, string exportFilename);
        IList<string> GetPlaylistFiles(bool onlyChanged);
        void RefreshPlaylistFilesContainer();
    }
}