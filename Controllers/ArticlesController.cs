using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetLab10.Data;
using DotNetLab10.Models;
using Microsoft.Extensions.Hosting;
using DotNetLab10.ViewModels;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Xml.Linq;

namespace DotNetLab10.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ShopDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private static string DEFAULT_IMAGE = "default_image.jpg";
       
        public ArticlesController(ShopDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Articles
        public async Task<IActionResult> Index()
        {
            var articlesContext = _context.Articles.Include(a => a.Category);
            var articles = await articlesContext.ToListAsync();
            articles.ForEach(a => { a.PictureName = a.PictureName == null ? DEFAULT_IMAGE : a.PictureName; });
            return View(articles);
        }

        // GET: Articles/Details/5
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
            article.PictureName = article.PictureName == null ? DEFAULT_IMAGE : article.PictureName;
            return View(article);
        }

        // GET: Articles/Create
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        private string? saveFile(IFormFile? formFile)
        {
            if (formFile != null)
            {
                string name = DateTime.Now.ToString("ddMMyyyyhhmmss") + formFile.FileName;
                string uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "upload", name);

                using (FileStream fs = System.IO.File.Create(uploadPath))
                {
                    formFile.CopyTo(fs);
                }
                return name;
            }
            return null;
        }

        // POST: Articles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArticleId,Name,Price,Picture,CategoryId")] ArticleViewModel articleViewModel)
        {
            if (ModelState.IsValid)
            {
                string? fileName = saveFile(articleViewModel.Picture);
                Article article = new Article()
                {
                    ArticleId = articleViewModel.ArticleId,
                    Name = articleViewModel.Name,
                    Price = articleViewModel.Price,
                    PictureName = fileName,
                    CategoryId = articleViewModel.CategoryId
                };
                
                _context.Add(article);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "Name", articleViewModel.CategoryId);
            return View(articleViewModel);
        }

        // GET: Articles/Edit/5
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
            article.PictureName = article.PictureName == null ? DEFAULT_IMAGE : article.PictureName;
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "Name", article.CategoryId);
            return View(article);
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArticleId,Name,Price,CategoryId,PictureName")] Article article)
        {
            if (id != article.ArticleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // znika obrazek jak sie aktualizuje
                    var articleToUpdate = await _context.Articles
                .Include(a => a.Category)
                .FirstOrDefaultAsync(m => m.ArticleId == id);
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
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "Name", article.CategoryId);

            return View(article);
        }

        // GET: Articles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a  => a.Category)
                .FirstOrDefaultAsync(m => m.ArticleId == id);
            if (article == null)
            {
                return NotFound();
            }
            article.PictureName = article.PictureName == null ? DEFAULT_IMAGE : article.PictureName;
            return View(article);
        }

        void removeImage(Article article)
        {
            if (article.PictureName != null)
            {
                System.IO.File.Delete(Path.Combine(_hostEnvironment.WebRootPath, "upload", article.PictureName));
            }
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            _context.Articles.Remove(article);
            removeImage(article);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.ArticleId == id);
        }
    }
}
