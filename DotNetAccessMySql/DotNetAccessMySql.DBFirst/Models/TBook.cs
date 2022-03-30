using System;
using System.Collections.Generic;

namespace DotNetAccessMySql.DBFirst.Models
{
    public partial class TBook
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime PubTime { get; set; }
        public double Price { get; set; }
        public string AuthorName { get; set; } = null!;
    }
}
