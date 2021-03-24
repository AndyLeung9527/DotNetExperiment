using AspNetAccessMongoDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetAccessMongoDB.DataPersist
{
    public interface IMongoDBAccessor
    {
        public CssSystemLog Find(string id);
    }
}
