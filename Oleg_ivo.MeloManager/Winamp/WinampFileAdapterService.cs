using System.Collections.Generic;
using Autofac;
using NLog;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.PlaylistFileAdapters;
using Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff;

namespace Oleg_ivo.MeloManager.Winamp
{
    public class WinampFileAdapterService : IFileAdapterService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly PlaylistImporter<WinampM3UPlaylistFileAdapter> importer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WinampFileAdapterService(IComponentContext context)
        {
            importer = context.ResolveUnregistered<PlaylistImporter<WinampM3UPlaylistFileAdapter>>();
        }

        public WinampM3UPlaylistFileAdapter Adapter { get { return importer.Adapter; } }

        

        public void RequestImport(string importFilename)
        {
            throw new System.NotImplementedException();
        }

        public void RequestExport(Playlist playlist, string exportFilename)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Playlist> Import(IEnumerable<string> playlistFilenames, Category winampCategory)
        {
            return importer.Import(playlistFilenames, winampCategory);
        }

        public void Import(string filename)
        {
            var diffAction = importer.GetImportDiffAction(filename);
            if (diffAction.DiffType == DiffType.None)
                log.Debug("При попытке импорта файла плейлиста {0} не обнаружилось различий", filename);
            else
                diffAction.Apply();
        }

        public void Save()
        {
            importer.DbContext.SaveChanges();
        }
    }
}