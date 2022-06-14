using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSQLCommander
{
    public class SQLTestClass
    {
        public void connect(string Server, string Username, string Password, string Database)
        {
            connect(new SqlConnection(generateConnectionString(Server, Username, Password, Database)));
        }

        public void connect(SqlConnection cnn)
        {
            try
            {
                cnn.Open();
                Console.WriteLine("Connection successfull");
                cnn.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("Connection unsuccessfull: " + e);
            }
        }




        public static string generateConnectionString(string ServerName, string Username, string Password, string DatabaseName)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = ServerName;
            builder.UserID = Username;
            builder.Password = Password;
            builder.InitialCatalog = DatabaseName;

            Console.WriteLine(builder.ConnectionString);

            return builder.ConnectionString;
        }

        public void connectToLocal()
        {
            var cs = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Storage;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            connect(new SqlConnection(cs));
        }


    }
}
