using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace EndedTask.Controllers
{
    public class LayoutController : Controller
    {
        public IActionResult _Layout() => PartialView();

    }
}
