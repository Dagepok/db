using System.Runtime.Serialization;

namespace DBTask.Models
{
    public partial class Users
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public UserType Type { get; set; }
    }

    public enum UserType
    {
        User = 0,
        Admin = 1
    }
}
