using Microsoft.AspNetCore.Mvc.Rendering;

namespace DBTask.Controllers
{
    public class LetterModel
    {
        public string Region { get; set; }
        public SelectList RegionsList { get; set; }

        public string Rayon { get; set; }
        public SelectList RayonsList { get; set; }

        public string City { get; set; }
        public SelectList CitiesList { get; set; }

        public string Street { get; set; }
        public SelectList StreetsList { get; set; }

        public string House { get; set; }
        public string Flat { get; set; }

        public string User { get; set; }
        public SelectList UsersList { get; set; }

        public string Code1 { get; set; }
        public string Code2 { get; set; }
        public string Code3 { get; set; }
    }
}