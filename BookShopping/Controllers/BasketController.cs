﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookShopping.Models.Authentication;
using BookShopping.Models.Context;
using BookShopping.Models.ShoppingModel;
using BookShopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookShopping.Controllers
{
    public class BasketController : Controller
    {
        readonly ShoppingDbContext _context;
        readonly UserManager<AppUser> _userManager;
        public object Session { get; private set; }
        public BasketController(ShoppingDbContext context, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index(int id, decimal total)
        {
            var user = _userManager.FindByNameAsync(User.Identity.Name).Result;
            List<Basket> model = _context.Baskets.Include(x => x.Product).Where(x => x.UserId == user.Id).ToList();
            if (model != null)
            {
                total = _context.Baskets.Sum(x => x.TotalFee);
                ViewBag.total = +total + "₺";
            }
            ViewBag.ProductId = id;
            return View(model);
        }
        public ActionResult List(int id)
        {
            List<Basket> model = _context.Baskets.Include(x => x.Product).ToList();
            ViewBag.ProductId = id;
            return View(model);
        }
        //[Route("Basket/Index/{id}")]
        public IActionResult Create(int id)
        {

            if (User.Identity.IsAuthenticated)
            {
                var userName = User.Identity.Name;
                var model = _userManager.Users.FirstOrDefault(x => x.UserName == userName);
                var product = _context.Products.Find(id);
                var basket = _context.Baskets.FirstOrDefault(x => x.ProductId == id);
                if (model != null)
                {
                    if (basket != null)
                    {
                        basket.Quantity++;
                        basket.TotalFee = product.Price * basket.Quantity;
                        _context.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    var addBasket = new Basket
                    {
                        UserId = model.Id,
                        ProductId = product.Id,
                        Quantity = 1,
                        Name = product.Name,
                        Price = product.Price,
                        TotalFee = product.Price,
                        Date = DateTime.Now
                    };
                    _context.Baskets.Add(addBasket);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var product = _context.Baskets.Find(id);
            _context.Baskets.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("Index", new { id = product.ProductId });
        }
    }
}
