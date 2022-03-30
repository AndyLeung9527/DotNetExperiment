using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetAccessMySql.CodeFirst
{
    public class TestDbContext : DbContext
    {
        /// <summary>
        /// Book表
        /// </summary>
        protected DbSet<Book>? Books { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=172.16.6.40;port=3306;user=root;password=root;database=TestDatabase;sslmode=none;CharSet=utf8;", MySqlServerVersion.LatestSupportedServerVersion);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}
