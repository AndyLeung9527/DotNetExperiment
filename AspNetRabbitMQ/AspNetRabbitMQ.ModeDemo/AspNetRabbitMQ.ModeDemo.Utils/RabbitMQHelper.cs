using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetRabbitMQ.ModeDemo.Utils
{
    public class RabbitMQHelper
    {
        public static IConnection GetConnection()
        {
            ConnectionFactory connectionFactory = new ConnectionFactory
            {
                HostName = "172.16.6.35",
                Port = 5672,
                UserName = "admin",
                Password = "admin",
                VirtualHost = "/"
            };
            return connectionFactory.CreateConnection();
        }
        public static string Exchange_3 => "Exchange_3";
        public static string Queue_1 => "Queue_1";
        public static string Queue_2 => "Queue_2";
        public static string Queue_3_1 => "Queue_3_1";
        public static string Queue_3_2 => "Queue_3_2";
        public static string Queue_3_3 => "Queue_3_3";
    }
}
