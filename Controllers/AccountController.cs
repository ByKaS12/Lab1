using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EndedTask.Mocks;
using EndedTask.Models;
using EndedTask.Models.ViewModels;


namespace EndedTask
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _db;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }
        [HttpGet]
        public IActionResult Register()=> View();

        public async Task<IActionResult> Confirm(Guid id)
        {
            
            var ConfirmOrder= await _db.Orders.FirstOrDefaultAsync(u=>u.Id == id);
            if (ConfirmOrder != null)
            {
                
                ConfirmOrder.OrderNumber = GenerateNumberOrder.generate();
                _db.Orders.Update(ConfirmOrder);
                await _db.SaveChangesAsync();
                return View("Confirm",_db);
            }
            return View("Confirm", null);
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Email};
                
                // добавляем пользователя
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Client");
                    Client client = new Client
                    {
                        Name = user.UserName,
                        Code = GenerateCodeClient.Generate(),
                        Id = Guid.Parse(user.Id)
                    };
                    _db.Clients.Add(client);
                    await _db.SaveChangesAsync();


                    // установка куки
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index","Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);


                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if (user.Email=="admin@gmail.com") { return RedirectToAction("PersonalArea", "Users"); }
                    else
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // удаляем аутентификационные куки
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }

}
