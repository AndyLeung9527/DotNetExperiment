using AspNetAccessMongoDB.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetAccessMongoDB.DataPersist
{
    public class MongoDBAccessor: IMongoDBAccessor
    {
        protected IConfiguration _configuration;
        protected MongoClient _mongoClient;
        private object _lock = new object();
        public MongoDBAccessor(IConfiguration configuration)
        {
            _configuration = configuration;
            if (_mongoClient == null)
                lock (_lock)
                    if (_mongoClient == null)
                    {
                        var connectionString = _configuration.GetSection("ConnectionStrings").GetSection("ERP_Mail_TEST").Value;
                        _mongoClient = new MongoClient(connectionString);
                    }
        }

        public CssSystemLog Find(string id)
        {
            return _mongoClient.GetDatabase("ERP_Mail").GetCollection<CssSystemLog>("SellerCube.CSS.Model.MongoDBModel.CssSystemLog").AsQueryable().Where(o => o._id == id).FirstOrDefault();
        }
    }
}
