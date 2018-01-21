using System;

namespace MeloManager.Api.Controllers
{
    public class MediaContainerProjection
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public Guid RowGuid { get; set; }
        public string Name { get; set; }
        public bool IsRoot { get; set; }
        public DateTime? DateUpdate { get; set; }
        public int ChildContainersCount { get; set; }
        public int ParentContainersCount { get; set; }
        public int FilesCount { get; set; }
    }
}