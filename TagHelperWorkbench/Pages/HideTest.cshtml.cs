using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TagHelperWorkbench.Pages
{
    public class HideTestModel : PageModel
    {
        private static Random rng = new Random();

        public bool Show { get; private set; }

        public string Message { get; } = "Some sample message.";

        public void OnGet()
        {
            Show = rng.NextDouble() < .5;
        }
    }
}