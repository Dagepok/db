using Microsoft.AspNetCore.Mvc.Rendering;

namespace DBTask.Controllers
{
    public class RegAddressModel
    {
        public RegAddressModel()
        {
        }

        public RegAddressModel(SelectList itemsList)
        {
            ItemsList = itemsList;
        }

        //public CodeAndName SelectedValue { get; set; }
        public string Code { get; set; }

        public SelectList ItemsList { get; set; }
    }
}