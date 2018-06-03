using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DBTask.DAL;
using DBTask.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DBTask.Controllers
{
    public class RegAddressController : Controller
    {
        private readonly MyBDContext _context;
        private readonly UsersRepository _repo;
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
        public async Task<IActionResult> EndReg()
        {
            await Authenticate(UsersRepository.lastRegUser.Username);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult CanselReg()
        {
            _repo.DeleteUser(UsersRepository.lastRegUser.Username);
            UsersRepository.lastRegUser = null;
            return RedirectToAction("Register", "Account");
        }
        public RegAddressController(UsersRepository repo, MyBDContext context)
        {
            _repo = repo;
            _context = context;
        }

        public IActionResult Index()
        {
            return View(UsersRepository.lastRegUser);
        }
        public IActionResult ChangeRegion()
        {
            var regex = new Regex("^[0-9]{2}0{11}");
            var regions = _context.Kladr
                .Where(x => regex.IsMatch(x.Code))
                .OrderBy(x => x.Name).Select(x => new SelectListItem
                {
                    Value = x.Code,
                    Text = x.Name + " " + _context.Socrbase.Where(y => int.Parse(y.Level) == 1 && y.Scname == x.Socr)
                               .Select(y => y.Socrname).Single()
                })
                .ToList();
            if (regions.Count == 0)
                return RedirectToAction("Index");

            var sverdl = regions.FirstOrDefault(x => x.Text.StartsWith("Свердловская"));
            if (sverdl != null)
            {
                var oldRegions = regions;
                regions = new List<SelectListItem> { sverdl };
                oldRegions.Remove(sverdl);
                regions.AddRange(oldRegions);
            }

            return View(new AddressModel(new SelectList(regions, "Value", "Text")));
        }

        [HttpPost]
        public IActionResult ChangeRegion(AddressModel addressModel)
        {
            var region = _context.Kladr
                .First(x => x.Code.Equals(addressModel.Code));
            var socrname = region.Name + " " + _context.Socrbase
                               .Where(y => int.Parse(y.Level) == 1 && y.Scname == region.Socr)
                               .Select(y => y.Socrname).Single();

            _repo.AddRegion(addressModel.Code, socrname, region.PostIndex, UsersRepository.lastRegUser);
            return RedirectToAction("Index");
        }

        public IActionResult ChangeRayon()
        {
            if (UsersRepository.lastRegUser.OblastCode == null)
                return RedirectToAction("Index");
            var precode = UsersRepository.lastRegUser.OblastCode.Substring(0, 2);
            var regex = new Regex($"^{precode}[0-9]{{3}}0{{8}}");

            var rayons = _context.Kladr
                .Where(x => regex.IsMatch(x.Code) && x.Code != UsersRepository.lastRegUser.OblastCode)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Code,
                    Text = x.Name + " " + _context.Socrbase.Where(y => int.Parse(y.Level) == 2 && y.Scname == x.Socr)
                               .Select(y => y.Socrname).Single()
                }).ToList();

            if (rayons.Count == 0)
                return RedirectToAction("Index");

            return View(new AddressModel(new SelectList(rayons, "Value", "Text")));
        }

        [HttpPost]
        public IActionResult ChangeRayon(AddressModel addressModel)
        {
            var region = _context.Kladr.First(x => x.Code == addressModel.Code);
            var socrname = region.Name + " " + _context.Socrbase
                               .Where(y => int.Parse(y.Level) == 2 && y.Scname == region.Socr)
                               .Select(y => y.Socrname).Single();
            _repo.AddRayon(addressModel.Code, socrname, region.PostIndex, UsersRepository.lastRegUser);
            return RedirectToAction("Index");
        }

        public IActionResult ChangeCity()
        {
            if (UsersRepository.lastRegUser.RayonCode == null && UsersRepository.lastRegUser.OblastCode == null)
                return RedirectToAction("Index");
            var precode = UsersRepository.lastRegUser.RayonCode == null
                ? UsersRepository.lastRegUser.OblastCode.Substring(0, 2)
                : UsersRepository.lastRegUser.Rayon.Substring(0, 5);
            var regex = precode.Length == 5
                ? new Regex($"{precode}[0-9]{{3}}00000")
                : new Regex($"{precode}[0-9]{{5}}[1-9]00000");


            var cities = _context.Kladr
                .Where(x => regex.IsMatch(x.Code) && (UsersRepository.lastRegUser.OblastCode != null &&
                                                      UsersRepository.lastRegUser.OblastCode != x.Code ||
                                                      UsersRepository.lastRegUser.OblastCode == null) &&
                            (UsersRepository.lastRegUser.RayonCode != null && UsersRepository.lastRegUser.RayonCode != x.Code ||
                             UsersRepository.lastRegUser.RayonCode == null) && x.Socr == "г")
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Code,
                    Text = x.Name + " " + _context.Socrbase.Where(y =>
                                   (int.Parse(y.Level) == 3 || int.Parse(y.Level) == 4) && x.Socr == y.Scname)
                               .Select(y => y.Socrname).First()
                })
                .ToList();

            if (cities.Count == 0)
                return RedirectToAction("Index");

            return View(new AddressModel(new SelectList(cities, "Value", "Text")));
        }

        [HttpPost]
        public IActionResult ChangeCity(AddressModel addressModel)
        {
            var region = _context.Kladr.First(x => x.Code == addressModel.Code);
            var socrname = region.Name + " " + _context.Socrbase.Where(y =>
                                   (int.Parse(y.Level) == 3 || int.Parse(y.Level) == 4) && region.Socr == y.Scname)
                               .Select(y => y.Socrname).First();
            _repo.AddCity(addressModel.Code, socrname, region.PostIndex, UsersRepository.lastRegUser);
            return RedirectToAction("Index");
        }

        public IActionResult ChangeVillage()
        {
            string precode;
            if (UsersRepository.lastRegUser.CityCode != null)
            {
                precode = UsersRepository.lastRegUser.CityCode.Substring(0, 8);
            }
            else if (UsersRepository.lastRegUser.RayonCode != null)
            {
                precode = UsersRepository.lastRegUser.RayonCode.Substring(0, 5);
            }
            else if (UsersRepository.lastRegUser.OblastCode != null)
            {
                precode = UsersRepository.lastRegUser.OblastCode.Substring(0, 2);
            }
            else
            {
                return RedirectToAction("Index");
            }

            var regex = new Regex($"^{precode}");


            var villages = _context.Kladr
                .Where(x => regex.IsMatch(x.Code) && string.Join("", x.Code.TakeLast(4)) != "0000")
                .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Code,
                Text = x.Name + " " + _context.Socrbase.Where(y =>
                               int.Parse(y.Level) >= 3 && int.Parse(y.Level) <= 5 && x.Socr == y.Scname)
                           .Select(y => y.Socrname).FirstOrDefault()
            })
            .ToList();

            if (villages.Count == 0)
                return RedirectToAction("Index");

            return View(new AddressModel(new SelectList(villages, "Value", "Text")));
        }

        [HttpPost]
        public IActionResult ChangeVillage(AddressModel addressModel)
        {
            var village = _context.Kladr.Single(x => x.Code == addressModel.Code);
            var socrname = village.Name + " " + _context.Socrbase.Where(y =>
                                   int.Parse(y.Level) >= 3 && int.Parse(y.Level) <= 5 && village.Socr == y.Scname)
                               .Select(y => y.Socrname).First();

            _repo.AddVillage(addressModel.Code, socrname, village.PostIndex, UsersRepository.lastRegUser);
            return RedirectToAction("Index");
        }

        public IActionResult ChangeStreet()
        {
            string precode;

            if (UsersRepository.lastRegUser.VillageCode != null)
                precode = UsersRepository.lastRegUser.VillageCode.Substring(0, 11);
            else if (UsersRepository.lastRegUser.CityCode != null)
                precode = UsersRepository.lastRegUser.CityCode.Substring(0, 11);
            else if (UsersRepository.lastRegUser.RayonCode != null)
                precode = UsersRepository.lastRegUser.RayonCode.Substring(0, 11);
            else if (UsersRepository.lastRegUser.OblastCode != null)
                precode = UsersRepository.lastRegUser.OblastCode.Substring(0, 11);
            else
                return RedirectToAction("Index");

            var regex = new Regex($"{precode}[0-9]{{4}}00");

            var streets = _context.Street
                .Where(x => regex.IsMatch(x.Code))
                .Select(x => new SelectListItem
                {
                    Text = x.Name + " " + x.Socrname,
                    Value = x.Code
                })
                .ToList();

            return View(new AddressModel(new SelectList(streets, "Value", "Text")));
        }

        [HttpPost]
        public IActionResult ChangeStreet(AddressModel addressModel)
        {
            var street = _context.Street.Single(x => x.Code == addressModel.Code);
            _repo.AddStreet(addressModel.Code, street.Name + " " + street.Socrname, street.Index, UsersRepository.lastRegUser);
            return RedirectToAction("Index");
        }

        public IActionResult ChangeHouseAndFlat()
        {
            string precode;
            if (UsersRepository.lastRegUser.StreetCode != null)
                precode = UsersRepository.lastRegUser.StreetCode.Substring(0, 15);
            else if (UsersRepository.lastRegUser.VillageCode != null)
                precode = UsersRepository.lastRegUser.VillageCode.Substring(0, 15);
            else if (UsersRepository.lastRegUser.CityCode != null)
                precode = UsersRepository.lastRegUser.CityCode.Substring(0, 15);
            else if (UsersRepository.lastRegUser.RayonCode != null)
                precode = UsersRepository.lastRegUser.RayonCode.Substring(0, 15);
            else if (UsersRepository.lastRegUser.OblastCode != null)
                precode = UsersRepository.lastRegUser.OblastCode.Substring(0, 15);
            else
                return RedirectToAction("Index");

            var regex = new Regex(@"\d+");

            var houses = _context.Doma.Where(x => EF.Functions.Like(x.Code, $"{precode}%"))
                .SelectMany(x => x.Name.Split(',', StringSplitOptions.RemoveEmptyEntries)).OrderBy(x => int.Parse(regex.Match(x).Value));


            if (!houses.Any())
                return RedirectToAction("Index");

            return View(new HouseAndFlatModel(new SelectList(houses), precode));
        }

        [HttpPost]
        public IActionResult ChangeHouseAndFlat(HouseAndFlatModel houseAndFlatModel)
        {
            var houses = _context.Doma
                .FirstOrDefault(x =>
                    EF.Functions.Like(x.Code, $"{houseAndFlatModel.Precode}%") &&
                    EF.Functions.Like(x.Name, $"%{houseAndFlatModel.House}%"));

            _repo.AddHouseAndFlat(houseAndFlatModel.House, houseAndFlatModel.Flat, houses?.Index, UsersRepository.lastRegUser);
            return RedirectToAction("Index");
        }

        public IActionResult DeleteRegion()
        {
            _repo.DeleteRegion(UsersRepository.lastRegUser);
            return RedirectToAction("Index");
        }

        public IActionResult DeleteRayon()
        {
            _repo.DeleteRayon(UsersRepository.lastRegUser);
            return RedirectToAction("Index");
        }

        public IActionResult DeleteCity()
        {
            _repo.DeleteCity(UsersRepository.lastRegUser);
            return RedirectToAction("Index");
        }

        public IActionResult DeleteVillage()
        {
            _repo.DeleteVillage(UsersRepository.lastRegUser);
            return RedirectToAction("Index");
        }

        public IActionResult DeleteStreet()
        {
            _repo.DeleteStreet(UsersRepository.lastRegUser);
            return RedirectToAction("Index");
        }


        public IActionResult DeleteHouseAndFlat()
        {
            _repo.DeleteHouseAndFlat(UsersRepository.lastRegUser);
            return RedirectToAction("Index");
        }
    }

}