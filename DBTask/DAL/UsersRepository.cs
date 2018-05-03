using System.Linq;
using DBTask.Models;
using Microsoft.AspNetCore.Http;

namespace DBTask.DAL
{
    public class UsersRepository
    {
        private readonly PinchukContext context;
        private readonly IHttpContextAccessor httpAccessor;

        public UsersRepository(PinchukContext context, IHttpContextAccessor httpAccessor)
        {
            this.context = context;
            this.httpAccessor = httpAccessor;
        }

        public Users GetCurrentUser()
        {
            var username = httpAccessor.HttpContext.User.Identity.Name;
            return context.Users.First(x => Equals(x.Username, username));
        }

        public void AddUser(Users user)
        {
            var lastId = context.Users.OrderByDescending(x => x.Id).First().Id;
            user.Id = lastId + 1;

            context.Users.Add(user);
            context.SaveChanges();
        }

        public void AddUser(string username, string password, UserType type, string fullname)
        {
            AddUser(new Users {Username = username, Password = password, Type = type, Fullname = fullname});
        }

        public Users GetUserByLoginAndPassword(string username, string password)
        {
            return context.Users.FirstOrDefault(x => Equals(x.Username, username) && Equals(x.Password, password));
        }

        public Users GetUserByLogin(string username)
        {
            return context.Users.First(x => Equals(x.Username, username));
        }
    }
}