using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.MeloManager.DependencyInjection;

namespace Oleg_ivo.MeloManager.Repairers
{
    public abstract class ProcessorBase
    {
        protected static readonly string[] MusicFilesSearchPatterns = { "*.mp3", "*.wma", "*.ogg", "*.flac", "*.aac" };
        protected MeloManagerOptions Options;

        protected ProcessorBase(MeloManagerOptions options, IComponentContext context)
        {
            Options = Enforce.ArgumentNotNull(options, "options");
        }

        protected List<string> GetAllFiles()
        {
            string musicFilesSource = Options.MusicFilesSource;
            var files =
                musicFilesSource.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                    .SelectMany(
                        path =>
                            MusicFilesSearchPatterns.SelectMany(
                                searchPattern => Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories)))
                    .ToList();
            return files;
        }
    }
}