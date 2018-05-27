using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DBTask.DAL;
using DBTask.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;

namespace DBTask.Controllers
{
    public class AccountController : Controller
    {
        private readonly UsersRepository _repo;

        public AccountController(UsersRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var password = HashPassword(model.Password);
                var user = _repo.GetUserByLoginAndPassword(model.Username, password);

                if (user != null)
                {
                    await Authenticate(model.Username);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Login or password is incorrect");
            }
            return View(model);
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            
            if (ModelState.IsValid)
            {
                try
                {
                    var user = _repo.GetUserByLogin(model.Username);
                }
                catch (Exception)
               
                {
                    var hash = HashPassword(model.Password);
                    //получить юзера без добавления в базу и отправить в создание адреса, с него добавить в базу
                    var user = new Users { Username = model.Username, Password = HashPassword(model.Password), Fullname = model.FullName};
                    
                   
                    
                    //_repo.AddUser(model.Username, hash, UserType.User, model.FullName);
                    UsersRepository.lastRegUser = _repo.AddUser(user);
                    //await Authenticate(model.Username);
                    return RedirectToAction("Index", "RegAddress");


                  

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Login or password are incorrect");
            }
            return View(model);
        }

        private async Task Authenticate(string login)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, login)
            };

            if (_repo.GetUserByLogin(login).Type == UserType.Admin)
                claims.Add(new Claim(ClaimTypes.Role, "admin"));

            var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private static string HashPassword(string password)
        {
            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password,
                new byte[0],
                KeyDerivationPrf.HMACSHA256,
                10000,
                256 / 8));

            return hashed;
        }
    }
}