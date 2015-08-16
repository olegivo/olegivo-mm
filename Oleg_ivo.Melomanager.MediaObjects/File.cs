using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.IO;
using System.Linq;
using NLog;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    //[DebuggerDisplay("{FullFileName}")]
    public class File : IEquatable<File>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public File()
        {
            MediaContainers = new HashSet<MediaContainer>();
        }

        public override string ToString()
        {
            return FullFileName;
        }

        private FileInfo fileInfo;
        private List<string> fullFileNameElements;
        private string fullFileName;

        public FileInfo FileInfo
        {
            get
            {
                if (fileInfo == null)
                    CreateFileInfoAndElements();
                return fileInfo;
            }
        }

        public virtual ICollection<MediaContainer> MediaContainers { get; set; }
        public long Id { get; set; }

        public string Drive { get; set; }

        public string Path { get; set; }

        public string Filename { get; set; }

        public string Extention { get; set; }

        public string FullFileName
        {
            get { return fullFileName; }
            set
            {
                if(fullFileName == value) return;
                fullFileName = value;
                CreateFileInfoAndElements();
            }
        }

        public string FileNameWithoutExtension { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }

        public bool Equals(File other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(fullFileName, other.fullFileName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((File) obj);
        }

        public override int GetHashCode()
        {
            return (fullFileName != null ? fullFileName.GetHashCode() : 0);
        }

        public static bool operator ==(File left, File right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(File left, File right)
        {
            return !Equals(left, right);
        }

        private void CreateFileInfoAndElements()
        {
            fileInfo = new FileInfo(FullFileName);
            fullFileNameElements = FullFileName.Split(new[] {'\\'}, StringSplitOptions.RemoveEmptyEntries).Reverse().ToList();
        }

        private class FullFileNameElementsComparer : IComparer<List<string>>
        {
            private readonly List<string> original;

            public FullFileNameElementsComparer(List<string> original)
            {
                this.original = original;
            }

            public int Compare(List<string> x, List<string> y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return 1;
                if (y == null) return -1;
                return GetIntersectsParts(x).CompareTo(GetIntersectsParts(y));
            }

            private int GetIntersectsParts(List<string> items)
            {
                var minLength = new[] { original.Count, items.Count }.Min();
                var i = 0;
                for (; i < minLength; i++)
                    if (!String.Equals(items[i],original[i], StringComparison.InvariantCultureIgnoreCase)) 
                        break;
                return i;
            }
        }

        public File Repair(IEnumerable<string> foundFiles, IMediaCache mediaCache)
        {
            //создание списка найденных файлов:
            var filename = Filename.ToLower();
            var repairedFiles =
                (foundFiles.Where(file => file.ToLower().Contains(filename))
                            .Select(mediaCache.GetOrAddCachedFile)).ToList();

            File repairedFile;
            switch (repairedFiles.Count)
            {
                case 0:
                    return null;
                case 1:
                    repairedFile = repairedFiles.Single();
                    log.Trace("При починке файла {0} найдено единственное соответствие {1}", FullFileName, repairedFile.FullFileName);
                    return repairedFile;
                default:
                    var comparer = new FullFileNameElementsComparer(this.fullFileNameElements);
                    var list = repairedFiles.OrderBy(f => f.fullFileNameElements, comparer).ToList();
                    repairedFile = list.First();
                    log.Trace("При починке файла {0} найдено несколько соответствий ({1}) но будет использовано только одно: {2}", FullFileName, repairedFiles.Count, repairedFile.FullFileName);
                    return repairedFile;
            }
        }
    }
}