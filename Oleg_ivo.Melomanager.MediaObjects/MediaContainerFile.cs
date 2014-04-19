namespace Oleg_ivo.MeloManager.MediaObjects
{
    //[DebuggerDisplay("MediaContainer:{MediaContainer}; File:{File.FileName}")]
    partial class MediaContainerFile
    {
        public override string ToString()
        {
            return string.Format("MediaContainer:{0}; File:{1}", MediaContainer, File);
        }
    }
}