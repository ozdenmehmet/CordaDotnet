using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CordaApp.DatabaseClass
{
    public class SqlDatabase
    {
        private MySqlConnection Connection;
        static IConfiguration _config = (new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build());
        public IConfigurationRoot GetConnection()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appSettings.json").Build();
            return builder;
        }
        public SqlDatabase()
        {
            string connString = _config.GetValue<string>("ConnectionStrings:defaultConnection");
            Connection = new MySqlConnection(connString);
            Connection.Open();
        }
        public MySqlConnection SqlConnection()
        {
            return Connection;
        }
        public void SqlClose()
        {
            Connection.Close();
            Connection.Dispose();
        }
    }
}
