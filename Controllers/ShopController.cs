using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetLab10.Data;
using DotNetLab10.Models;
using System.Configuration;
using Microsoft.AspNetCore.Http;

namespace DotNetLab10.Controllers
{
    public class ShopController : Controller
    {
        private readonly ShopDbContext _context;

        private string DEFAULT_IMAGE = "default_image.jpg";
        private static string ALL_CATEGORIES = "-1";

        public ShopController(ShopDbContext context)
        {
            _context = context;

        }

        // GET: Shop
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

        public IActionResult AddFromList(int? id)
        {
            Add(id);
            return RedirectToAction("FilterArticles");
        }

        public IActionResult RemoveFromList(int? id)
        {
            Remove(id);
            return RedirectToAction("FilterArticles");
        }

        public IActionResult AddFromBasket(int? id)
        {
            Add(id);
            return RedirectToAction("ShowBasket");
        }

        public IActionResult RemoveFromBasket(int? id)
        {
            Remove(id);
            return RedirectToAction("ShowBasket");
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

        public async Task<IActionResult> ShowBasket()
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
                    basketItems.Add(new BasketItem(item, itemCount));
                    summary += itemValue;
                } 
            }

            ViewData["summary"] = String.Format("{0:0.00}", summary);

            if (basketItems.Count() == 0)
            {
                return View("EmptyBasket");
            }

            return View("Basket", basketItems);
        }

        public async Task<IActionResult> FilterArticles(string Category)
        {
            Category = Category == null ? "-1" : Category;
            var shopDbContext = _context.Articles.Include(a => a.Category);
            List<SelectListItem> categories = new List<SelectListItem>();
            categories.Add(new SelectListItem { Text = "All", Value = "-1" });

            foreach (var category in _context.Categories)
            {
                categories.Add(new SelectListItem { Text = category.Name, Value = category.CategoryId.ToString() });
            }
           
            categories.Find(c => c.Value == Category).Selected = true;

            ViewBag.Category = categories;

            var articles = await shopDbContext.ToListAsync();
            articles.ForEach(a => { a.PictureName = a.PictureName == null ? DEFAULT_IMAGE : a.PictureName; });
            if (Category != "-1")
            {
                articles = articles.Where(a => a.CategoryId.ToString() == Category).ToList();
            }
            
            return View("FilterArticles", articles);
        }

        // GET: Shop/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.Category)
                .FirstOrDefaultAsync(m => m.ArticleId == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // GET: Shop/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId");
            return View();
        }

        // POST: Shop/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArticleId,Name,Price,PictureName,CategoryId")] Article article)
        {
            if (ModelState.IsValid)
            {
                _context.Add(article);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", article.CategoryId);
            return View(article);
        }

        // GET: Shop/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", article.CategoryId);
            return View(article);
        }

        // POST: Shop/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArticleId,Name,Price,PictureName,CategoryId")] Article article)
        {
            if (id != article.ArticleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(article);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArticleExists(article.ArticleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", article.CategoryId);
            return View(article);
        }

        // GET: Shop/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.Category)
                .FirstOrDefaultAsync(m => m.ArticleId == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // POST: Shop/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.ArticleId == id);
        }
    }
}
