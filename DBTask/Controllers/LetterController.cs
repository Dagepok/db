using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DBTask.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SautinSoft.Document;

namespace DBTask.Controllers
{
    [Authorize(Roles = "admin")]
    public class LetterController : Controller
    {
        private readonly PinchukContext context;

        public LetterController(PinchukContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var regex = new Regex("^[0-9]{2}0{11}");
            var regions = context.Kladr
                .Where(x => regex.IsMatch(x.Code))
                .OrderBy(x => x.Name).Select(x =>
                    x.Name + " " + context.Socrbase.Where(y => int.Parse(y.Level) == 1 && y.Scname == x.Socr)
                        .Select(y => y.Socrname).Single()).ToList();

            if (regions.FirstOrDefault(x => x.StartsWith("Свердловская")) != null)
            {
                var oldRegions = regions;
                regions = new List<string> {"Свердловская Область"};
                oldRegions.Remove("Свердловская Область");
                regions.AddRange(oldRegions);
            }

            var model = new LetterModel {RegionsList = new SelectList(regions)};

            return View(model);
        }

        [HttpPost]
        [ActionName("Index")]
        public IActionResult IndexPost(LetterModel model)
        {
            return RedirectToAction("Rayon", model);
        }

        [HttpGet]
        public IActionResult Rayon(LetterModel model)
        {
            var regex = new Regex("^[0-9]{2}0{11}");
            model.Code1 = context.Kladr.Where(x =>
                    regex.IsMatch(x.Code) && model.Region.StartsWith(x.Name)).FirstOrDefault().Code
                .Substring(0, 2);

            var regex2 = new Regex($"{model.Code1}[0-9]{{3}}0{{8}}");
            var rayons = context.Kladr.Where(x => regex2.IsMatch(x.Code))
                .OrderBy(x => x.Name).Select(x =>
                    x.Name + " " + context.Socrbase.FirstOrDefault(y => y.Level == "2" && y.Scname == x.Socr).Socrname).ToList();

            for(var i = 0; i < rayons.Count; i++)
                if (rayons[i] == null)
                    rayons[i] = "Без района";

            model.RayonsList = new SelectList(rayons);

            return View(model);
        }

        [HttpPost]
        [ActionName("Rayon")]
        public IActionResult RayonPost(LetterModel model)
        {
            return RedirectToAction("City", model);
        }

        [HttpGet]
        public IActionResult City(LetterModel model)
        {
            var socr = new HashSet<string>
            {
                "АО",
                "Аобл",
                "г",
                "край",
                "обл",
                "Респ",
                "р-н",
                "тер",
                "у",
                "волость",
                "г",
                "дп",
                "кп",
                "пгт",
                "п/о",
                "рп",
                "с/а",
                "с/тер",
                "с/о",
                "с/мо",
                "с/пос",
                "с/с",
                "тер"
            };

            var regex2 = new Regex($"{model.Code1}[0-9]{{3}}0{{8}}");
            if (model.Rayon == "Без района")
                model.Code2 = model.Code1 + "000";
            else
                model.Code2 = context.Kladr.First(x => regex2.IsMatch(x.Code) && (model.Rayon == "Без района" || model.Rayon.StartsWith(x.Name))).Code
                    .Substring(0, 5);

            var regex3 = new Regex($"{model.Code2}[0-9]{{8}}");
            var regex4 = new Regex($"{model.Code2}0{{8}}");
            var cities = context.Kladr.Where(x => regex3.IsMatch(x.Code) && !regex4.IsMatch(x.Code) && (model.Rayon == "Без района" && socr.Contains(x.Socr) || model.Rayon != "Без района"))
                .OrderBy(x => x.Name).Select(x =>
                x.Name + " " + context.Socrbase.FirstOrDefault(y => y.Level != "1" && y.Level != "2" && y.Scname == x.Socr).Socrname).ToList();

            model.CitiesList = new SelectList(cities);

            return View(model);
        }

        [HttpPost]
        [ActionName("City")]
        public IActionResult CityPost(LetterModel model)
        {
            return RedirectToAction("Street", model);
        }

        [HttpGet]
        public IActionResult Street(LetterModel model)
        {
            var regex3 = new Regex($"{model.Code2}[0-9]{{6}}");
            model.Code3 = context.Kladr.First(x => regex3.IsMatch(x.Code) && model.City.StartsWith(x.Name)).Code
                .Substring(0, 11);

            var streets = context.Street.Where(x => EF.Functions.Like(x.Code, $"{model.Code3}%")).Select(x =>
                x.Name + " " + x.Socrname/* + " " + context.Socrbase.FirstOrDefault(y => int.Parse(y.Level) > 3 && y.Scname == x.Socr)
                    .Socrname*/).ToHashSet();

            model.StreetsList = new SelectList(streets);

            return View(model);
        }

        [HttpPost]
        [ActionName("Street")]
        public IActionResult StreetPost(LetterModel model)
        {
            return RedirectToAction("House", model);
        }

        [HttpGet]
        public IActionResult House(LetterModel model)
        {
            var code = context.Street.First(x => EF.Functions.Like(x.Code, model.Code3 + '%') && model.Street.StartsWith(x.Name)).Code + '%';

            var houses = context.Doma.Where(x => EF.Functions.Like(x.Code, code)).ToList();

            model.HouseList = new SelectList(houses.SelectMany(x => x.Name.Split(",", StringSplitOptions.RemoveEmptyEntries))
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase));

            return View(model);
        }

        [HttpPost]
        [ActionName("House")]
        public IActionResult HousePost(LetterModel model)
        {
            return RedirectToAction("User", model);
        }

        [HttpGet]
        public IActionResult User(LetterModel model)
        {
            var users = context.Users.Select(x => x.Fullname);
            model.UsersList = new SelectList(users);
            return View(model);
        }

        [HttpPost]
        [ActionName("User")]
        public IActionResult UserPost(LetterModel model)
        {
            var docx = new DocumentCore();

            var section = new Section(docx);
            docx.Sections.Add(section);

            section.PageSetup.PaperType = PaperType.A4;
            var par = new Paragraph(docx);
            section.Blocks.Add(par);
            var run1 = new Run(docx, $"{model.Region}");
            var run2 = new Run(docx, $"{model.City}");
            var run3 = new Run(docx, $"{model.Street}, д. { model.House }, кв. { model.Flat}");
            var run4 = new Run(docx, $"{model.User}");

            par.Inlines.Add(run1);
            par.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            par.Inlines.Add(run2);
            par.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            par.Inlines.Add(run3);
            par.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            par.Inlines.Add(run4);
            par.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));


            par.ParagraphFormat.Alignment = HorizontalAlignment.Left;
            
            var filename = Path.GetTempFileName();

            docx.Save(filename, SaveOptions.DocxDefault);
            //return File(memory, "application/ms", "letter.docx");
            return File(System.IO.File.ReadAllBytes(filename), "application/msword", "letter.docx");
        }
    }
}