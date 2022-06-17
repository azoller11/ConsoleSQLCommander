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
        public bool updateObject(Object obj, List<string> whatFields, List<string> newValues);
        public bool updateObject(Object obj, List<Object> whatFields, List<string> newValues);
        public bool deleteObject(Object obj);
        public bool insertObject(Object obj);
        public bool createObjectTable(Type type);
        public bool modifyObjectTable(Object obj);
        public bool deleteObjectTable(Object obj);
        public bool checkObjForID(Type type);
        public SqlConnection databaseConnection(string connectionString);
        public string findSQLField(Type field);
        public System.Data.SqlDbType findSQLType(Type field);
        public void open();
        public void close();
        public bool insert(string command);
        public bool update(string command);
        public bool insert(SqlCommand myCommand);
        public bool delete(string command);
        public SqlDataReader read(string command);
        public bool testConnection();
        public Object getItem<T>(T obj, List<string> whatFields, string where, string wherevalue);
        public List<T> getItems<T>(T obj, List<string> whatFields, string where, string wherevalue, int limit);
        public List<T> getLikeItems<T>(T obj, List<string> whatFields, string where, string wherevalue, int limit);
        public SqlConnection getConnection();
    }
}
