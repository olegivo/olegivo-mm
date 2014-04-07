using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    [DebuggerDisplay("{FullFileName}")]
    partial class File
    {
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
                    if (!string.Equals(items[i],original[i], StringComparison.InvariantCultureIgnoreCase)) 
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

            switch (repairedFiles.Count)
            {
                case 0:
                    return null;
                case 1:
                    return repairedFiles.Single();
                default:
                    var comparer = new FullFileNameElementsComparer(this.fullFileNameElements);
                    var list = repairedFiles.OrderBy(f => f.fullFileNameElements, comparer).ToList();
                    return list.First();
            }
        }

        public static File GetFile(string fullFilename)
        {
            var fileName = System.IO.Path.GetFileName(fullFilename);
            var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fullFilename);
            var extension = System.IO.Path.GetExtension(fullFilename);
            var drive = System.IO.Path.GetPathRoot(fullFilename);
            var path = System.IO.Path.GetDirectoryName(fullFilename);
            return new File
            {
                FullFileName = fullFilename,
                Drive = drive,
                Path = path,
                Filename = fileName,
                FileNameWithoutExtension = fileNameWithoutExtension,
                Extention = extension
            };

        }
    }
}