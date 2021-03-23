using AspNetAccessMySql.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetAccessMySql.DbContexts
{
    /// <summary>
    /// Database: erp_css
    /// </summary>
    public class erp_css : DbContext
    {
        /// <summary>
        /// Table: test_user
        /// </summary>
        protected DbSet<test_user> test_user { get; set; }

        public erp_css(DbContextOptions options) : base(options) { }
    }
}
