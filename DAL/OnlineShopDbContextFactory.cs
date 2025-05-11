using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL
{
    public class OnlineShopDbContextFactory : IDesignTimeDbContextFactory<OnlineShopDbContext>
    {
        public OnlineShopDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OnlineShopDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=OnlineShopDb;Trusted_Connection=True;TrustServerCertificate=True;");

            return new OnlineShopDbContext(optionsBuilder.Options);
        }
    }
}
