using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace TestApp
{
    public class ConnectionDB : IDisposable
    {
        public MySqlConnection Connection { get; }

        string ipServer = "localhost";

        public ConnectionDB()
        {
            var config = new ConfigurationBuilder()
                        .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json").Build();

            Connection = new MySqlConnection("SERVER=" + ipServer + ";database=test;" + config["ConnectionStrings:db"] + "");
        }

        public void Dispose() => Connection.Dispose();
        public void Close() => Connection.Close();
    }
}
