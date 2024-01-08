

using DotNetLab10.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetLab10.Data
{
    public class ShopDbContext:DbContext
    {

        public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Article> Articles { get; set; }

        public DbSet<DotNetLab10.Models.BasketItem> BasketItem { get; set; }


    }
}
