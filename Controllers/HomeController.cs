﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EndedTask.Models;

namespace EndedTask.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
        

    }
}
