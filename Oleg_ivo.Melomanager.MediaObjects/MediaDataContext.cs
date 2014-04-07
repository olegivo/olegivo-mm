namespace Oleg_ivo.MeloManager.MediaObjects
{
    partial class MediaFile
    {

    }

    partial class MediaDataContext
    {


        public Category CreateCategory()
        {
            var category = new Category();
            MediaContainers.InsertOnSubmit(category);
            return category;
        }

        public Playlist CreatePlaylist()
        {
            var playlist = new Playlist();
            MediaContainers.InsertOnSubmit(playlist);
            return playlist;
        }

        public MediaFile CreateMediaFile()
        {
            var mediaFile = new MediaFile();
            MediaContainers.InsertOnSubmit(mediaFile);
            return mediaFile;
        }
    }
}
