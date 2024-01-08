using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetLab10.Data;
using DotNetLab10.Models;
using Microsoft.AspNetCore.Http;

namespace DotNetLab10.Controllers
{
    public class BasketItemsController : Controller
    {
        private readonly ShopDbContext _context;

        private string DEFAULT_IMAGE = "default_image.jpg";
        private static string ALL_CATEGORIES = "-1";

        public BasketItemsController(ShopDbContext context)
        {
            _context = context;
        }

        public void SetOrUpdateArticleCookie(int articleId, int addValue = 1, int numberOfDays = 7)
        {

            string artId = articleId.ToString();
            int newValue = addValue;
            if (Request.Cookies.ContainsKey(artId))
            {
                newValue += int.Parse(Request.Cookies[artId]);
                Response.Cookies.Delete(artId);
            }

            if (newValue > 0)
            {
                CookieOptions option = new CookieOptions();
                option.Expires = DateTime.Now.AddDays(numberOfDays);
                Response.Cookies.Append(artId, newValue.ToString(), option);
            }

        }

        public IActionResult AddToBasket(int? id)
        {
            Add(id);
            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromBasket(int? id)
        {
            Remove(id);
            return RedirectToAction("Index");
        }

        public void Add(int? id)
        {
            var article = _context.Articles
                    .Include(a => a.Category)
                    .FirstOrDefault(m => m.ArticleId == id);

            SetOrUpdateArticleCookie(article.ArticleId);
        }

        public void Remove(int? id)
        {
            var article = _context.Articles
                    .Include(a => a.Category)
                    .FirstOrDefault(m => m.ArticleId == id);

            SetOrUpdateArticleCookie(article.ArticleId, addValue: -1);
        }

        // GET: BasketItems
        public async Task<IActionResult> Index()
        {
            var shopDbContext = _context.Articles.Include(a => a.Category);
            var articles = await shopDbContext.ToListAsync();
            articles.ForEach(a => { a.PictureName = a.PictureName == null ? DEFAULT_IMAGE : a.PictureName; });

            var basketItems = new List<BasketItem>();

            double summary = 0;

            foreach (var item in articles)
            {
                if (Request.Cookies.ContainsKey(item.ArticleId.ToString()))
                {
                    var itemCount = int.Parse(Request.Cookies[item.ArticleId.ToString()]);
                    double itemValue = itemCount * item.Price;
                    var basketItem = new BasketItem(item.ArticleId, itemCount);
                    basketItem.Article = item;
                    basketItems.Add(basketItem);
                    summary += itemValue;
                }
            }

            ViewData["summary"] = String.Format("{0:0.00}", summary);

            if (basketItems.Count() == 0)
            {
                return View("EmptyBasket");
            }

            return View("Index", basketItems);
        }
    }
     
}
