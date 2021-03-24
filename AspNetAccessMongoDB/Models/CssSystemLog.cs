using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetAccessMongoDB.Models
{
    public class CssSystemLog
    {
        public string _id { get; set; }
        public string Key { get; set; }
        public string SecondaryKey { get; set; }
        public string LogType { get; set; }
        public string Message { get; set; }
        public DateTime? CreateTime { get; set; }
        public string StackTrace { get; set; }
        public string InnerMessage { get; set; }
        public string InnerStackTrace { get; set; }
    }
}
