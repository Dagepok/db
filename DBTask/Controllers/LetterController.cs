using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DBTask.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SautinSoft.Document;

namespace DBTask.Controllers
{
    [Authorize(Roles = "admin")]
    public class LetterController : Controller
    {
        private readonly PinchukContext _context;

        public LetterController(PinchukContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var users = _context.Users.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Fullname
            });
            return View(new LetterModel(new SelectList(users, "Value", "Text")));
        }

        [HttpPost]
        public IActionResult Index(LetterModel letterModel)
        {
            //return Content(string.Join('\n', letterModel.SelectedIds));
            var docx = new DocumentCore();
            foreach (var id in letterModel.SelectedIds)
            {
                var section = GetSectionForUser(docx, _context.Users.Single(x => x.Id == int.Parse(id)));
                var par = new Paragraph(docx);
                section.Blocks.Add(par);

                var separators = new List<char>();
                for (var i = 0; i < 32; i++)
                    separators.Add((char) i);

                for (var i = 0; i < 4; i++)
                {
                    var run = new SpecialCharacter(docx, SpecialCharacterType.LineBreak);
                    par.Inlines.Add(run);
                }

                var text = letterModel.Text.Split(separators.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var str in text)
                {
                    var run = new Run(docx, str);
                    par.Inlines.Add(run);
                    var @break = new SpecialCharacter(docx, SpecialCharacterType.LineBreak);
                    par.Inlines.Add(@break);
                }

                par.ParagraphFormat.Alignment = HorizontalAlignment.Center;
                docx.Sections.Add(section);
            }

            var filename = Path.GetTempFileName();
            docx.Save(filename, SaveOptions.DocxDefault);
            return File(System.IO.File.ReadAllBytes(filename), "application/msword", "letter.docx");
        }

        private static Section GetSectionForUser(DocumentCore docx, Users user)
        {
            var section = new Section(docx) {PageSetup = {PaperType = PaperType.A4}};

            var par = new Paragraph(docx);
            section.Blocks.Add(par);

            var house = user.House + (user.Flat != null ? " кв. " + user.Flat : "");
            foreach (var str in new[]
                {user.Index, user.Oblast, user.Rayon, user.City, user.Village, user.Street, house, user.Fullname})
            {
                if (string.IsNullOrEmpty(str)) continue;
                var run = new Run(docx, str);
                par.Inlines.Add(run);
                par.Inlines.Add(new SpecialCharacter(docx, SpecialCharacterType.LineBreak));
            }

            par.ParagraphFormat.Alignment = HorizontalAlignment.Right;

            return section;
        }
    }
}