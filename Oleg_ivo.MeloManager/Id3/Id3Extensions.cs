using System;
using System.Text;
using Oleg_ivo.MeloManager.MediaObjects;
using TagLib;
using TagLib.Id3v2;
using File = TagLib.File;

namespace Oleg_ivo.MeloManager.Id3
{
    public static class Id3Extensions
    {
        public static Guid ReadMMId(this File file)
        {
            var t = (TagLib.Id3v2.Tag) file.GetTag(TagTypes.Id3v2);
            var p = PrivateFrame.Get(t, MediaFile.MMId, false); // This is important. Note that the third parameter is false.
            var data = Encoding.Unicode.GetString(p.PrivateData.Data);
            return Guid.Parse(data);
        }

        public static void WriteMMId(this File file, Guid value)
        {
            var t = (TagLib.Id3v2.Tag)file.GetTag(TagTypes.Id3v2); // You can add a true parameter to the GetTag function if the file doesn't already have a tag.
            var p = PrivateFrame.Get(t, MediaFile.MMId, true);
            p.PrivateData = Encoding.Unicode.GetBytes(value.ToString());
        }
    }
}
