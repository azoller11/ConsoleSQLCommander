using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSQLCommander
{
    public interface ISQLCommander
    {
        public bool updateObject<T>(T obj, List<string> whatFields, List<string> newValues);
        public bool updateObject<T>(T obj, List<Object> whatFields, List<string> newValues);
        public bool deleteObject<T>(T obj, Type field, string value);
        public bool deleteObject<T>(T obj, string field, string value);
        public bool insertObject<T>(T obj);
        public bool createObjectTable<T>(T type);
        public bool modifyObjectTable<T>(T obj);
        public bool deleteObjectTable<T>(T obj);
        public bool checkObjForID<T>(T type);
        public SqlConnection databaseConnection(string connectionString);
        public string findSQLField<T>(T field);
        public System.Data.SqlDbType findSQLType<T>(T field);
        public void open();
        public void close();
        public bool insert(string command);
        public bool update(string command);
        public bool insert(SqlCommand myCommand);
        public SqlDataReader read(string command);
        public bool testConnection();
        public Object getItem<T>(T obj, List<string> whatFields, string where, string wherevalue);
        public List<T> getItems<T>(T obj, List<string> whatFields, string where, string wherevalue, int limit);
        public List<T> getLikeItems<T>(T obj, List<string> whatFields, string where, string wherevalue, int limit);
        public bool clearItems<T>(T obj);
        public SqlConnection getConnection();
        public void log(string item);
        public string getLog();
        public string generateConnectionString(string ServerName, string Username, string Password, string DatabaseName, bool encrypt)
    }
}
