using System.Collections.Generic;

namespace ProMag_Steam_Games
{
    public class SteamApp
    {
        public int AppId { get; set; }
        public string? Name { get; set; }
        public string? Genres { get; set; }
        public string? ReleaseYear { get; set; }
        public string? HeaderImage { get; set; }
    }

    public class Package
    {
        public string? Name { get; set; }
        public List<int> AppIds { get; set; } = [];
    }

    public class PackageResponse
    {
        public List<Package> Packages { get; set; } = [];
    }
}