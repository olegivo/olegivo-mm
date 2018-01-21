using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NLog;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.MeloManager.DependencyInjection;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.MediaObjects.Extensions;
using Oleg_ivo.MeloManager.Repairers;
using Oleg_ivo.Tools.Utils;
using File = System.IO.File;

namespace Oleg_ivo.MeloManager.Id3
{
    public class Id3Updater : ProcessorBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly MediaDbContext mediaRepository;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:System.Object"/>.
        /// </summary>
        public Id3Updater(MeloManagerOptions options, IComponentContext context, MediaDbContext mediaRepository) : base(options, context)
        {
            this.mediaRepository = mediaRepository;
        }

        public void Run()
        {
            mediaRepository.RefreshCache();

            using (new ElapsedAction(elapsed => log.Debug("Obtaining ID3 info completed in {0}", elapsed)))
            {
                mediaRepository.Playlists.SelectMany(p => p.ChildContainers)
                    .Distinct()
                    .AsEnumerable()
                    .Cast<MediaFile>()
                    .Select(mf => new { mf, file = mf.Files.Select(f => f.FullFileName).SingleOrDefault(File.Exists) })
                    .Where(item => item.file != null)
                    .Select(item => new { item.mf, file = TagLib.File.Create(item.file) })
                    .Apply(item => Update(item.file, item.mf))
                    .ToList();
            }

            using (new ElapsedAction(elapsed => log.Debug("Saving completed in {0}", elapsed)))
                mediaRepository.SaveChanges();
            //mediaRepository.SubmitChangesWithLog();
        }

        private static int? NullIfEmpty(int value)
        {
            return value > 0 ? value : (int?) null;
        }

        private static string NullIfEmpty(string value)
        {
            return !string.IsNullOrWhiteSpace(value) ? value: null;
        }

        private void Update(TagLib.File file, MediaFile mediaFile)
        {
            if (file.PossiblyCorrupt)
            {
                log.Error("File {0} posible corrupt: \n{1}", file.Name, file.CorruptionReasons.JoinToString("\n"));
                return;
            }

            var updated = false;

            var album = NullIfEmpty(file.Tag.Album);
            if (mediaFile.Album != album)
            {
                mediaFile.Album = album;
                updated = true;
            }

            var artist = NullIfEmpty(file.Tag.FirstAlbumArtist) ?? NullIfEmpty(file.Tag.FirstArtist);
            if (mediaFile.Artist != artist)
            {
                mediaFile.Artist = artist;
                updated = true;
            }

            var totalSeconds = NullIfEmpty((int) Math.Round(file.Properties.Duration.TotalSeconds));
            if (mediaFile.Length != totalSeconds && totalSeconds > 0)
            {
                mediaFile.Length = totalSeconds;
                updated = true;
            }

            var title = NullIfEmpty(file.Tag.Title);
            if (mediaFile.Title != title)
            {
                mediaFile.Title = title;
                updated = true;
            }

            var track = NullIfEmpty((int) file.Tag.Track);
            if (mediaFile.Track != track)
            {
                mediaFile.Track = track;
                updated = true;
            }

            var trackCount = NullIfEmpty((int) file.Tag.TrackCount);
            if (mediaFile.TrackCount != trackCount)
            {
                mediaFile.TrackCount = trackCount;
                updated = true;
            }

            var year = NullIfEmpty((int) file.Tag.Year);
            if (mediaFile.Year != year)
            {
                mediaFile.Year = year;
                updated = true;
            }

            if (updated)
            {
                log.Debug("{0} updated", mediaFile);
            }
        }
    }
}