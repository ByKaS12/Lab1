using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EndedTask.Models;
using EndedTask.Mocks;
using Microsoft.AspNetCore.Identity;

namespace EndedTask.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        public ProductController(ApplicationDbContext dbContext, UserManager<User> userManager)
        {
            _db = dbContext;
            _userManager = userManager;

        }
        public IActionResult ProductList()=> View(_db.Products.ToList());
        public IActionResult Find(string Category)
        {
            var FindList = _db.Products.Where(u => u.Category == Category).ToList();
            if (FindList != null)
            return View("ProductList", FindList);
            return View("ProductList", _db.Products.ToList());

        }
        public async Task<IActionResult> DeleteOrderItems(Guid id)
        {
            var h = await _db.OrderItems.FirstOrDefaultAsync(u => u.Id == id);
            if (h != null)
                _db.OrderItems.Remove(h);
            await _db.SaveChangesAsync();
            return View("Buy",_db);

        }
        public IActionResult Buy() => View("Buy", _db);
        public async Task<IActionResult> Select(Guid item,string button) { 
            if(button == "delete")
            {
               var h = await _db.Products.FirstOrDefaultAsync(u => u.Id == item);
                _db.Products.Remove(h);
                await _db.SaveChangesAsync();
                return View("ProductList", _db.Products.ToList());
            }
            if(button == "change")
                return View("Shift", item);
            if (button == "buy") {
              //  User user = await _userManager.FindByIdAsync(Convert.ToString(item));
               // if ((await _userManager.IsInRoleAsync(user, "Client") == true) && (user!=null))
               if(User.IsInRole("Client"))
                {
                    var user = await _userManager.FindByNameAsync(User.Identity.Name);
                    var h = await _db.Clients.FirstOrDefaultAsync(u => u.Id == Guid.Parse(user.Id));
                    if (h == null) {
                        Client client = new Client
                        {
                            Name = User.Identity.Name,
                            Code = GenerateCodeClient.Generate(),
                            Id = Guid.Parse(user.Id)
                        };
                        _db.Clients.Add(client);
                        await _db.SaveChangesAsync();

                        BuyingProduct.AddToCan(_db,client, await _db.Products.FirstOrDefaultAsync(u => u.Id == item));
                        
                        return View("Buy",_db);
                    }
                    else {
                        BuyingProduct.AddToCan(_db,h, await _db.Products.FirstOrDefaultAsync(u => u.Id == item));
                        
                        return View("Buy",_db);
                        
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            return View("ProductList", _db.Products.ToList());
        }
   
        public IActionResult Create()
        {

            return View("Create");
        }
        public async Task<IActionResult> Add(Product product)
        {
           
            var h = await _db.Products.FirstOrDefaultAsync(u => u.Name == product.Name);
            var copy = product;
            if (h == null)
            {
               copy.Code = GenerateCodeProduct.GenerateCode();
                if(await _db.Products.FirstOrDefaultAsync(u => u.Code == copy.Code) == null) { 
                _db.Products.Add(copy);
                await _db.SaveChangesAsync();
                }
            }
            return View("ProductList", _db.Products.ToList());
        }
        public async Task<IActionResult> Changed(Guid mark,Product product)
        {
            var h = await _db.Products.FirstOrDefaultAsync(u => u.Id == mark);
            var g =  _db.OrderItems.Where(u => u.ProductId == h.Id);
            bool flag = false;
            if (h!=null) {
                if (product.image != null) {
                    flag = true; 
                    h.image = product.image;
                }
                if (product.Name != null) {
                    flag = true;
                    h.Name = product.Name;
                }
                if (product.Price > 0) {
                    flag = true;
                    h.Price = product.Price;
                }
                if (product.Category != null) {
                    flag = true;
                    h.Category = product.Category;
                }
                if (g != null && flag==true) {
                    foreach (var item in g)
                        _db.OrderItems.Remove(item);
                        _db.SaveChanges();
                }
                if (flag == true) { 
                _db.Products.Update(h);           
                await _db.SaveChangesAsync();
                }
                await _db.SaveChangesAsync();

            }
            return View("ProductList", _db.Products.ToList());
        }
    }
}
