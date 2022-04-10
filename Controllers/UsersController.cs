using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EndedTask.Models;
using EndedTask.Models.ViewModels;
using Excel = Microsoft.Office.Interop.Excel;

namespace EndedTask
{
   // [Authorize(Roles ="admin")]
    public class UsersController : Controller
    {
        UserManager<User> _userManager;
        private readonly ApplicationDbContext _db;

        public UsersController(UserManager<User> userManager,ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }
        public IActionResult Index() => View(_userManager.Users.ToList());

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Email};
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
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
        public async Task<IActionResult> ChangePassword(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ChangePasswordViewModel model = new ChangePasswordViewModel { Id = user.Id, Email = user.Email };
            return View(model);
        }
        public IActionResult Clients() => View(_db.Clients.ToList());
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    var _passwordValidator =
                        HttpContext.RequestServices.GetService(typeof(IPasswordValidator<User>)) as IPasswordValidator<User>;
                    var _passwordHasher =
                        HttpContext.RequestServices.GetService(typeof(IPasswordHasher<User>)) as IPasswordHasher<User>;

                    IdentityResult result =
                        await _passwordValidator.ValidateAsync(_userManager, user, model.NewPassword);
                    if (result.Succeeded)
                    {
                        user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
                        await _userManager.UpdateAsync(user);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Пользователь не найден");
                }
            }
            return View(model);
        }
        public async Task<IActionResult> Edit(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            EditUserViewModel model = new EditUserViewModel { Id = user.Id, Email = user.Email};
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    user.Email = model.Email;
                    user.UserName = model.Email;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Client")) {
                    var h = await _db.Clients.FirstOrDefaultAsync(u => u.Id == Guid.Parse(id));
                    if (h !=null) {
                        _db.Remove(h);
                    await _db.SaveChangesAsync();
                    }
                }

                IdentityResult result = await _userManager.DeleteAsync(user);

               
            }
            if (User.IsInRole("admin"))
            {
                return View("Index", _userManager.Users.ToList());
            } else return RedirectToAction("Index","Home");
            
        }
        public void ExportToExcel()
        {
            Excel.Application ex = new Excel.Application
            {
                Visible = true,
                SheetsInNewWorkbook = 1
            };

            Excel.Workbook workBook = ex.Workbooks.Add(Type.Missing);
            ex.DisplayAlerts = false;
            Excel.Worksheet sheet = (Excel.Worksheet)ex.Worksheets.get_Item(1);
            sheet.Name = $"ONIT DB created of {DateTime.UtcNow.ToShortDateString()}";
            sheet.Columns.ColumnWidth = 20;
            sheet.Cells[1, 1] = "ID";
            sheet.Cells[1, 2] = "Название";
            sheet.Cells[1, 3] = "Тип";
            sheet.Cells[1, 4] = "Цена";
            sheet.Cells[1, 5] = "Ссылка на фото";
            var List = _db.Products.ToList();
            for (int i = 0; i < List.Count; i++)
            {
                sheet.Cells[i + 2, 1] = List[i].Id.ToString();
                sheet.Cells[i + 2, 2] = List[i].Name;
                sheet.Cells[i + 2, 3] = List[i].Category;
                sheet.Cells[i + 2, 4] = List[i].Price;
                sheet.Cells[i + 2, 5] = List[i].image;

            }


        }
        public async Task<IActionResult> ChangeOrder(Order order,string button)
        {
            var copy = await _db.Orders.FirstOrDefaultAsync(u=>u.Id ==order.Id);
            if (User.IsInRole("admin")) { 
            if(button == "Change" && copy!=null )
            {
                    copy.ShipmentDate = order.ShipmentDate;
                    _db.Orders.Update(copy);
                    await _db.SaveChangesAsync();
                    return View("PersonalArea",_db);
            }else
                if (button == "Confirm" && copy != null)
                {
                    copy.Status = "Performed";
                    _db.Orders.Update(copy);
                    await _db.SaveChangesAsync();
                    return View("PersonalArea", _db);

                }else if (button == "Finish" && copy != null)
                {
                    copy.Status = "Completed";
                    _db.Orders.Update(copy);
                    await _db.SaveChangesAsync();
                    return View("PersonalArea", _db);

                }
                else if (button == "Delete" && copy != null)
                {
                    return await DeleteOrder(copy.Id);
                }
            }
            return RedirectToAction("Index","Home");
        }
        public async Task<IActionResult> ChangeClient(Client client,string button)
        {
            var h = await _db.Clients.FirstOrDefaultAsync(u=>u.Id == client.Id);
            if (h!=null)
            {
                if (button == "NameB")
                    h.Name = client.Name;
                else if (button == "AddressB")
                    h.Address = client.Address;
                else if(button == "DiscountB") { 
                    h.Discount = client.Discount;
                    _db.Clients.Update(h);
                    await _db.SaveChangesAsync();
                    return View("Clients", _db.Clients.ToList());
                }else return View("PersonalArea", _db);
                _db.Clients.Update(h);
                await _db.SaveChangesAsync();
                return View("PersonalArea", _db);

            }
            return View("PersonalArea", _db);

        }
        public async Task<IActionResult> DeleteOrder(Guid mark)
        {
            var h = await _db.Orders.FirstOrDefaultAsync(u=>u.Id == mark);
            if (h != null)
            {
                _db.Orders.Remove(h);
                await _db.SaveChangesAsync();
                return View("PersonalArea", _db);
            }
            else
            return View("PersonalArea", _db);
        }
        public IActionResult PersonalArea() => View("PersonalArea", _db);

    }
}
