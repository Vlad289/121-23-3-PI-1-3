using DAL;
using Microsoft.EntityFrameworkCore;

namespace PL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OnlineShopDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=OnlineShopDb;Trusted_Connection=True;TrustServerCertificate=True;");

            using var context = new OnlineShopDbContext(optionsBuilder.Options);

            context.Database.Migrate(); // замість EnsureCreated

            Console.WriteLine("Міграції застосовано. База даних готова.");
        }
    }
}
