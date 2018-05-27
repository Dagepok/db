using System.Linq;
using DBTask.Models;
using Microsoft.AspNetCore.Http;

namespace DBTask.DAL
{
    public class UsersRepository
    {
        private readonly MyBDContext _context;
        private readonly IHttpContextAccessor _httpAccessor;
        public static Users lastRegUser;

        public UsersRepository(MyBDContext context, IHttpContextAccessor httpAccessor)
        {
            _context = context;
            _httpAccessor = httpAccessor;
        }

        public Users GetCurrentUser()
        {
            var username = _httpAccessor.HttpContext.User.Identity.Name;
            return _context.Users.First(x => Equals(x.Username, username));
        }

        public void DeleteUser(string login) => _context.Users.Remove(_context.Users.First(x => x.Username == login));

        public Users AddUser(Users user)
        {
            var lastId = _context.Users.OrderByDescending(x => x.Id).First().Id;
            user.Id = lastId + 1;

            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public Users AddUser(string username, string password, UserType type, string fullname)
            => AddUser(new Users { Username = username, Password = password, Type = type, Fullname = fullname });

        public Users GetUserByLoginAndPassword(string username, string password)
            => _context.Users.FirstOrDefault(x => Equals(x.Username, username) && Equals(x.Password, password));


        public Users GetUserByLogin(string username) => _context.Users.First(x => Equals(x.Username, username));

        public void AddRegion(string code, string name, string index, Users user = null)
        {
            if (user is null)
                user = GetCurrentUser();
            user.OblastCode = code;
            user.Oblast = name;
            if (index != null)
                user.Index = index;

            user.RayonCode = null;
            user.Rayon = null;
            user.CityCode = null;
            user.City = null;
            user.VillageCode = null;
            user.Village = null;
            user.StreetCode = null;
            user.Street = null;
            user.House = null;
            user.Flat = null;

            _context.Update(user);
            _context.SaveChanges();
        }

        public void DeleteRegion(Users user = null)
        {
            if (user is null)
                user = GetCurrentUser();

            user.OblastCode = null;
            user.Oblast = null;
            user.Index = null;


            _context.Update(user);
            _context.SaveChanges();
        }

        public void AddRayon(string code, string name, string index, Users user = null)
        {
            if (user is null)
                user = GetCurrentUser();
            user.RayonCode = code;
            user.Rayon = name;
            if (index != null)
                user.Index = index;

            user.CityCode = null;
            user.City = null;
            user.VillageCode = null;
            user.Village = null;
            user.StreetCode = null;
            user.Street = null;
            user.House = null;
            user.Flat = null;

            _context.Update(user);
            _context.SaveChanges();
        }

        public void DeleteRayon(Users user = null)
        {
            if (user is null)
                user = GetCurrentUser();

            user.RayonCode = null;
            user.Rayon = null;
            user.Index = null;


            _context.Update(user);
            _context.SaveChanges();
        }

        public void AddCity(string code, string name, string index, Users user = null)
        {
            if (user is null)
                user = GetCurrentUser();
            user.CityCode = code;
            user.City = name;
            if (index != null)
                user.Index = index;

            user.VillageCode = null;
            user.Village = null;
            user.StreetCode = null;
            user.Street = null;
            user.House = null;
            user.Flat = null;

            _context.Update(user);
            _context.SaveChanges();
        }


        public void DeleteCity(Users user = null)
        {
            if (user is null)
                user = GetCurrentUser();
            user.CityCode = null;
            user.City = null;
            user.Index = null;


            _context.Update(user);
            _context.SaveChanges();
        }

        public void AddVillage(string code, string name, string index, Users user = null)
        {
            if (user is null)
                user = GetCurrentUser();
            user.VillageCode = code;
            user.Village = name;
            if (index != null)
                user.Index = index;

            user.StreetCode = null;
            user.Street = null;
            user.House = null;
            user.Flat = null;

            _context.Update(user);
            _context.SaveChanges();
        }


        public void DeleteVillage(Users user = null)
        {
            if (user is null)
                user = GetCurrentUser();

            user.VillageCode = null;
            user.Village = null;
            user.Index = null;


            _context.Update(user);
            _context.SaveChanges();
        }

        public void AddStreet(string code, string name, string index, Users user = null)
        {
            if (user is null)
                user = GetCurrentUser();
            user.StreetCode = code;
            user.Street = name;
            if (index != null)
                user.Index = index;

            user.House = null;
            user.Flat = null;

            _context.Update(user);
            _context.SaveChanges();
        }


        public void DeleteStreet(Users user = null)
        {
            if (user is null)
                user = GetCurrentUser();

            user.StreetCode = null;
            user.Street = null;
            user.Index = null;


            _context.Update(user);
            _context.SaveChanges();
        }

        public void AddHouseAndFlat(string house, string flat, string index, Users user = null)
        {
            if (user is null)
                user = GetCurrentUser();
            user.House = house;
            user.Flat = flat;
            if (index != null)
                user.Index = index;
            _context.Update(user);
            _context.SaveChanges();
        }

        public void DeleteHouseAndFlat(Users user = null)
        {
            if (user is null)
                user = GetCurrentUser();

            user.House = null;
            user.Flat = null;
            user.Index = null;

            _context.Update(user);
            _context.SaveChanges();
        }
    }
}