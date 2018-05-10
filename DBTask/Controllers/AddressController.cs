using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DBTask.DAL;
using DBTask.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DBTask.Controllers
{
    [Authorize]
    public class AddressController : Controller
    {
        private readonly PinchukContext _context;
        private readonly UsersRepository _repo;

        public AddressController(UsersRepository repo, PinchukContext context)
        {
            _repo = repo;
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_repo.GetCurrentUser());
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

            var sverdl = regions.FirstOrDefault(x => x.Text.StartsWith("Свердловская"));
            if (sverdl != null)
            {
                var oldRegions = regions;
                regions = new List<SelectListItem> {sverdl};
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

            _repo.AddRegion(addressModel.Code, socrname, region.PostIndex);
            return RedirectToAction("Index");
        }

        public IActionResult ChangeRayon()
        {
            if (_repo.GetCurrentUser().OblastCode == null)
                return RedirectToAction("Index");
            var precode = _repo.GetCurrentUser().OblastCode.Substring(0, 2);
            var regex = new Regex($"^{precode}[0-9]{{3}}0{{8}}");

            var rayons = _context.Kladr
                .Where(x => regex.IsMatch(x.Code) && x.Code != _repo.GetCurrentUser().OblastCode)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Code,
                    Text = x.Name + " " + _context.Socrbase.Where(y => int.Parse(y.Level) == 2 && y.Scname == x.Socr)
                               .Select(y => y.Socrname).Single()
                }).ToList();

            return View(new AddressModel(new SelectList(rayons, "Value", "Text")));
        }

        [HttpPost]
        public IActionResult ChangeRayon(AddressModel addressModel)
        {
            var region = _context.Kladr.First(x => x.Code == addressModel.Code);
            var socrname = region.Name + " " + _context.Socrbase
                               .Where(y => int.Parse(y.Level) == 2 && y.Scname == region.Socr)
                               .Select(y => y.Socrname).Single();
            _repo.AddRayon(addressModel.Code, socrname, region.PostIndex);
            return RedirectToAction("Index");
        }

        public IActionResult ChangeCity()
        {
            if (_repo.GetCurrentUser().RayonCode == null && _repo.GetCurrentUser().OblastCode == null)
                return RedirectToAction("Index");
            var precode = _repo.GetCurrentUser().RayonCode == null
                ? _repo.GetCurrentUser().OblastCode.Substring(0, 5)
                : _repo.GetCurrentUser().Rayon.Substring(0, 5);
            var regex = new Regex($"{precode}[0-9]{{3}}00000");

            var cities = _context.Kladr
                .Where(x => regex.IsMatch(x.Code) && (_repo.GetCurrentUser().OblastCode != null &&
                                                      _repo.GetCurrentUser().OblastCode != x.Code ||
                                                      _repo.GetCurrentUser().OblastCode == null) &&
                            (_repo.GetCurrentUser().RayonCode != null && _repo.GetCurrentUser().RayonCode != x.Code ||
                             _repo.GetCurrentUser().RayonCode == null))
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Code,
                    Text = x.Name + " " + _context.Socrbase.Where(y =>
                                   (int.Parse(y.Level) == 3 || int.Parse(y.Level) == 4) && x.Socr == y.Scname)
                               .Select(y => y.Socrname).First()
                })
                .ToList();

            return View(new AddressModel(new SelectList(cities, "Value", "Text")));
        }

        [HttpPost]
        public IActionResult ChangeCity(AddressModel addressModel)
        {
            var region = _context.Kladr.First(x => x.Code == addressModel.Code);
            var socrname = region.Name + " " + _context.Socrbase.Where(y =>
                                   (int.Parse(y.Level) == 3 || int.Parse(y.Level) == 4) && region.Socr == y.Scname)
                               .Select(y => y.Socrname).First();
            _repo.AddCity(addressModel.Code, socrname, region.PostIndex);
            return RedirectToAction("Index");
        }

        public IActionResult ChangeVillage()
        {
            string precode, rayonCode, regionCode;
            var cityCode = rayonCode = regionCode = "";
            if (_repo.GetCurrentUser().CityCode != null)
            {
                precode = _repo.GetCurrentUser().CityCode.Substring(0, 8);
                cityCode = _repo.GetCurrentUser().CityCode;
            }
            else if (_repo.GetCurrentUser().RayonCode != null)
            {
                precode = _repo.GetCurrentUser().RayonCode.Substring(0, 8);
                rayonCode = _repo.GetCurrentUser().RayonCode;
            }
            else if (_repo.GetCurrentUser().OblastCode != null)
            {
                precode = _repo.GetCurrentUser().OblastCode.Substring(0, 8);
                regionCode = _repo.GetCurrentUser().OblastCode;
            }
            else
            {
                return RedirectToAction("Index");
            }

            var regex = new Regex($"^{precode}[0-9]{{3}}00");

            var villages = _context.Kladr
                .Where(x => regex.IsMatch(x.Code) && x.Code != cityCode && x.Code != rayonCode && x.Code != regionCode)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Code,
                    Text = x.Name + " " + _context.Socrbase.Where(y =>
                                   int.Parse(y.Level) >= 3 && int.Parse(y.Level) <= 5 && x.Socr == y.Scname)
                               .Select(y => y.Socrname).First()
                })
                .ToList();

            return View(new AddressModel(new SelectList(villages, "Value", "Text")));
        }

        [HttpPost]
        public IActionResult ChangeVillage(AddressModel addressModel)
        {
            var village = _context.Kladr.Single(x => x.Code == addressModel.Code);
            var socrname = village.Name + " " + _context.Socrbase.Where(y =>
                                   int.Parse(y.Level) >= 3 && int.Parse(y.Level) <= 5 && village.Socr == y.Scname)
                               .Select(y => y.Socrname).First();

            _repo.AddVillage(addressModel.Code, socrname, village.PostIndex);
            return RedirectToAction("Index");
        }

        public IActionResult ChangeStreet()
        {
            string precode;

            if (_repo.GetCurrentUser().VillageCode != null)
                precode = _repo.GetCurrentUser().VillageCode.Substring(0, 11);
            else if (_repo.GetCurrentUser().CityCode != null)
                precode = _repo.GetCurrentUser().CityCode.Substring(0, 11);
            else if (_repo.GetCurrentUser().RayonCode != null)
                precode = _repo.GetCurrentUser().RayonCode.Substring(0, 11);
            else if (_repo.GetCurrentUser().OblastCode != null)
                precode = _repo.GetCurrentUser().OblastCode.Substring(0, 11);
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
            _repo.AddStreet(addressModel.Code, street.Name + " " + street.Socrname, street.Index);
            return RedirectToAction("Index");
        }

        public IActionResult ChangeHouseAndFlat()
        {
            string precode;
            if (_repo.GetCurrentUser().StreetCode != null)
                precode = _repo.GetCurrentUser().StreetCode.Substring(0, 15);
            else if (_repo.GetCurrentUser().VillageCode != null)
                precode = _repo.GetCurrentUser().VillageCode.Substring(0, 15);
            else if (_repo.GetCurrentUser().CityCode != null)
                precode = _repo.GetCurrentUser().CityCode.Substring(0, 15);
            else if (_repo.GetCurrentUser().RayonCode != null)
                precode = _repo.GetCurrentUser().RayonCode.Substring(0, 15);
            else if (_repo.GetCurrentUser().OblastCode != null)
                precode = _repo.GetCurrentUser().OblastCode.Substring(0, 15);
            else
                return RedirectToAction("Index");

            var houses = _context.Doma.Where(x => EF.Functions.Like(x.Code, $"{precode}%"))
                .SelectMany(x => x.Name.Split(',', StringSplitOptions.RemoveEmptyEntries)).OrderBy(x => x);

            return View(new HouseAndFlatModel(new SelectList(houses), precode));
        }

        [HttpPost]
        public IActionResult ChangeHouseAndFlat(HouseAndFlatModel houseAndFlatModel)
        {
            var houses = _context.Doma
                .FirstOrDefault(x =>
                    EF.Functions.Like(x.Code, $"{houseAndFlatModel.Precode}%") &&
                    EF.Functions.Like(x.Name, $"%{houseAndFlatModel.House}%"));

            _repo.AddHouseAndFlat(houseAndFlatModel.House, houseAndFlatModel.Flat, houses?.Index);
            return RedirectToAction("Index");
        }

        public IActionResult DeleteRegion()
        {
            _repo.DeleteRegion();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteRayon()
        {
            _repo.DeleteRayon();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteCity()
        {
            _repo.DeleteCity();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteVillage()
        {
            _repo.DeleteVillage();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteStreet()
        {
            _repo.DeleteStreet();
            return RedirectToAction("Index");
        }


        public IActionResult DeleteHouseAndFlat()
        {
            _repo.DeleteHouseAndFlat();
            return RedirectToAction("Index");
        }
    }

    public class HouseAndFlatModel
    {
        public HouseAndFlatModel()
        {
        }

        public HouseAndFlatModel(SelectList itemsList, string precode)
        {
            ItemsList = itemsList;
            Precode = precode;
        }

        public string House { get; set; }
        public string Flat { get; set; }
        public SelectList ItemsList { get; set; }
        public string Precode { get; set; }
    }

    public class AddressModel
    {
        public AddressModel()
        {
        }

        public AddressModel(SelectList itemsList)
        {
            ItemsList = itemsList;
        }

        //public CodeAndName SelectedValue { get; set; }
        public string Code { get; set; }

        public SelectList ItemsList { get; set; }
    }
}