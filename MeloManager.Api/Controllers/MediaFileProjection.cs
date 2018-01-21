namespace MeloManager.Api.Controllers
{
    internal class MediaFileProjection : MediaContainerProjection
    {
        public string Album { get; set; }
        public string Artist { get; set; }
        public int? Length { get; set; }
        public string Title { get; set; }
        public int? Track { get; set; }
        public int? TrackCount { get; set; }
        public int? Year { get; set; }
        public object File { get; set; }
    }
}