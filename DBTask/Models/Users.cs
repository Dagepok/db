namespace DBTask.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public UserType Type { get; set; }
        public string OblastCode { get; set; }
        public string Oblast { get; set; }
        public string RayonCode { get; set; }
        public string Rayon { get; set; }
        public string CityCode { get; set; }
        public string City { get; set; }
        public string StreetCode { get; set; }
        public string Street { get; set; }
        public string House { get; set; }
        public string Flat { get; set; }
        public string Village { get; set; }
        public string VillageCode { get; set; }
        public string Index { get; set; }
    }

    public enum UserType
    {
        User = 0,
        Admin = 1
    }
}