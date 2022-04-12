using Microsoft.EntityFrameworkCore;

namespace TRS.Console
{
    class Program
    {
        private static Data.ApplicationDbContext _dbContext;

        static void Main()
        {
            var options = new DbContextOptionsBuilder<Data.ApplicationDbContext>();
            options.UseSqlServer("Server=.;Database=TRS;Integrated Security=True");

            _dbContext = new Data.ApplicationDbContext(options.Options);



            System.Console.WriteLine("Hello World!");
        }

        public static void AddDefaultUser()
        {
            var user = new Data.Models.ApplicationUser
            {
                FirstName = "Ali",
                LastName = "Zeynalli",
                UserName = "alizeynali",
                Email = "ali.zeynalli@hotmail.com"
            };
        }
    }
}
