namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    internal class ExtInfo
    {
        private readonly string title;
        internal readonly string filename;

        public ExtInfo(string title, string filename)
        {
            this.title = title;
            this.filename = filename;
        }
    }
}