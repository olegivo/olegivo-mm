using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    //[DebuggerDisplay("{FullFileName}")]
    partial class File : IEquatable<File>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override string ToString()
        {
            return FullFileName;
        }

        private FileInfo fileInfo;
        private List<string> fullFileNameElements;

        public FileInfo FileInfo
        {
            get
            {
                if (fileInfo == null)
                    CreateFileInfoAndElements();
                return fileInfo;
            }
        }

        public bool Equals(File other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_FullFileName, other._FullFileName);
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
            return (_FullFileName != null ? _FullFileName.GetHashCode() : 0);
        }

        public static bool operator ==(File left, File right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(File left, File right)
        {
            return !Equals(left, right);
        }

        partial void OnFullFileNameChanged()
        {
            CreateFileInfoAndElements();
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

        public File Repair(IEnumerable<string> foundFiles)
        {
            //создание списка найденных файлов:
            var repairedFiles =
                (foundFiles.Where(file => file.ToLower().Contains(Filename.ToLower()))
                            .Select(GetFile)).ToList();

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

        private static readonly Dictionary<string, File> filesCache = new Dictionary<string, File>();

        public static File GetFile(string fullFilename)
        {
            if (filesCache.ContainsKey(fullFilename))
                return filesCache[fullFilename];

            var fileName = System.IO.Path.GetFileName(fullFilename);
            var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fullFilename);
            var extension = System.IO.Path.GetExtension(fullFilename);
            var drive = System.IO.Path.GetPathRoot(fullFilename);
            var path = System.IO.Path.GetDirectoryName(fullFilename);
            var file = new File
            {
                FullFileName = fullFilename,
                Drive = drive,
                Path = path,
                Filename = fileName,
                FileNameWithoutExtension = fileNameWithoutExtension,
                Extention = extension
            };
            filesCache.Add(fullFilename, file);
            return file;

        }
    }
}