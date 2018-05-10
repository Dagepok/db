using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DBTask.Controllers
{
    public class LetterModel
    {
        public LetterModel()
        {
        }

        public LetterModel(SelectList items)
        {
            Items = items;
        }

        public SelectList Items { get; set; }
        public IEnumerable<string> SelectedIds { get; set; }
        public string Text { get; set; }
    }
}