using PhotoBooth.Models;

namespace PhotoBooth.Service
{
    public static class Settings
    {
        public static PhotoEvent CurrentEvent { get; set; }
        public static PhotoEvent LastEvent { get; set; }
        public static string BrandImage { get; set; }
    }
}