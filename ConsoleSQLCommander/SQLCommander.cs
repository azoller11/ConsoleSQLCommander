using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSQLCommander
{
    public class SQLCommander : ISQLCommander
    {
        
        SqlConnection data;
        string connectionString;
        string logString;
        List<string> logs;

        DateTime openTime;

        public SQLCommander(string connectionString)
        {
            this.data = databaseConnection(connectionString);
            this.connectionString = connectionString;
            this.logString = "";
            this.logs = new List<string>();
        }
        public bool clearItems<T>(T obj)
        {
            return update("DELETE FROM " + obj.GetType().Name);
        }
        public void log(string item) {
            //logString += System.Environment.NewLine + " [" +DateTime.Now + "] " + item;
            logs.Add(" [" + DateTime.Now + "] " + item);
        }
        public string getLog() {
            foreach(string s in logs)
            {
                logString += System.Environment.NewLine + s;
            }
            return logString;
        }
        public bool deleteObject<T>(T obj, Type field, string value)
        {
            string fieldConvert = "CONVERT(" + "TEXT" + ", " + "Name" +")";
            string command = "DELETE FROM " + obj.GetType().Name + " WHERE " + fieldConvert + " = '" + value + "'";
            //Console.WriteLine(command);
            log(command);
            return update(command);
        }
        public bool deleteObject<T>(T obj, string field, string value)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = data;
            sqlCommand.CommandType = System.Data.CommandType.Text;


            string fieldConvert = "CONVERT(" + "VARCHAR" + ", " + field + ")";
            string command = "DELETE FROM " + obj.GetType().Name + " WHERE " + fieldConvert + " = @" + field;
            sqlCommand.Parameters.AddWithValue("@"+ field, value);

            log(command);
            //Console.WriteLine(command);

            sqlCommand.CommandText = command;
            int result = 0;
            try
            {
                open();
                result = sqlCommand.ExecuteNonQuery();
                //Console.WriteLine("Result: " + result);
            }
            catch (Exception e)
            {
                //Console.WriteLine("Result: " + e);
                log("Result: " + e);
            }
            finally
            {
                close();
            }
            return (result == 1);
        }
        public bool deleteObjectTable<T>(T obj)
        {
            return update("DROP TABLE " + obj.GetType().Name);
        }
        public bool modifyObjectTable<T>(T obj)
        {
            int results = 0;
            string command = "ALTER TABLE " + obj.GetType().Name + " ";
            using (SqlConnection conn = getConnection())
            {
                string[] restrictions = new string[4] { null, null, obj.GetType().Name, null };
                open();
                var selectedRows = from info in conn.GetSchema("Columns", restrictions).AsEnumerable()
                                   select new
                                   {
                                       ColumnName = info["COLUMN_NAME"],
                                       DataType = info["DATA_TYPE"]
                                   };
                if (selectedRows.Count() > obj.GetType().GetProperties().Length) {
                    //The SQL table is larger that the given object DROP
                    command += "DROP COLUMN ";
                    for (int i = obj.GetType().GetProperties().Length; i < selectedRows.Count(); i++)
                    {
                        string getCN = selectedRows.ToArray()[i].ColumnName.ToString();
                        command += "" + getCN + "";
                        if (i != selectedRows.Count() - 1)
                            command += ", ";
                        else
                            command += ";";
                    }
                    //Console.WriteLine("The SQL table is larger that the given object DROP");
                } else if (selectedRows.Count() < obj.GetType().GetProperties().Length) {
                    //The SQL Table is smaller than the given object ADD
                    command += "ADD";
                    //Console.WriteLine("The SQL Table is smaller than the given object ADD");
                    for (int i = selectedRows.Count(); i < obj.GetType().GetProperties().Length; i++)
                    {
                        string itemDT = findSQLField(obj.GetType().GetProperties()[i].PropertyType);
                        string itemCN = obj.GetType().GetProperties()[i].Name.ToString();
                        command += " " + itemCN + " [" + itemDT + "]";
                        if (i != obj.GetType().GetProperties().Length - 1)
                            command += ", ";
                        else
                            command += ";";
                    }
                } else if (selectedRows.Count() == obj.GetType().GetProperties().Length) {
                    //The SQL Table is the same size as the given object ALTER
                    //Console.WriteLine("The SQL Table is the same size as the given object ALTER");
                    for (int i = 0; i < selectedRows.Count(); i++)
                    {
                        string getCN = selectedRows.ToArray()[i].ColumnName.ToString();
                        string getDT = selectedRows.ToArray()[i].DataType.ToString();

                        string itemDT = findSQLField(obj.GetType().GetProperties()[i].PropertyType);
                        string itemCN = obj.GetType().GetProperties()[i].Name.ToString();

                        if (getDT != itemDT) // The data types are not the same
                        {
                            command += "ALTER COLUMN " + getCN + " " + itemDT; //changing the column data type to the given items datatype
                        } else if (getCN != itemCN) // The name of the column is not the same
                        {
                            command += "RENAME COLUMN " + getCN + " TO " + itemCN;
                        }
                        //Console.WriteLine("SQL: " + getCN + " : " + getDT + " ITEM: " + itemCN + " : " + itemDT);
                    }
                }
                if (command.Equals("ALTER TABLE " + obj.GetType().Name + " "))
                {
                    //Console.WriteLine("There where no edits to the table");
                    log("There where no edits to the table");
                    close();
                    return false;
                }

                //Console.WriteLine(command);
                log(command);
                SqlCommand myCommand = new SqlCommand(command, data);
                results = myCommand.ExecuteNonQuery();
                close();
            }
            
            return results == 1;
        }
        public string findSQLField<T>(T field)
        {
            //This converts the known system primative datatype into a valid SQL datatype
            string check = field.ToString();
            switch(check)
            {
                case "System.Int32":
                    return "int";
                case "System.String":
                    return "text";
                case "System.DateTime":
                    return "datetime";
                case "System.Boolean":
                    return "bit";
                default:
                    //Console.WriteLine("Can only create tables with primative objects on field: " + check);
                    log("Can only create tables with primative objects on field: " + check);
                    return "text";
            }
        }
        public System.Data.SqlDbType findSQLType<T>(T field)
        {
            switch (Type.GetTypeCode(field.GetType()))
            {
                case TypeCode.String:
                    return System.Data.SqlDbType.Text;
                case TypeCode.Boolean:
                    return System.Data.SqlDbType.Bit;
                case TypeCode.Int32:
                    return System.Data.SqlDbType.Int;
                case TypeCode.DateTime:
                    return System.Data.SqlDbType.DateTime;
                case TypeCode.Int64:
                    return System.Data.SqlDbType.Int;
                default:
                    return System.Data.SqlDbType.Text;
            }
        }
        //------------------Completed Methods----------------------
        public void open()
        {
            //Console.WriteLine("Connecting..");
            log("Connecting..");
            try
            {
                data.Open();
                openTime = DateTime.Now;
                log("Connection open");
                //Console.WriteLine("Connection open");
            }
            catch (Exception e)
            {
                log("Error connecting to Database: " + e);
                //Console.WriteLine("Error connecting to Database: " + e);
            }
        }
        public void close()
        {
            try
            {
                data.Close();
                log("Connection closed: Total Connection Time: " + (DateTime.Now - openTime));
                //Console.WriteLine("Connection closed: Total Connection Time: " + (DateTime.Now - openTime));
                //Console.WriteLine(getLog());
            }
            catch (Exception e)
            {
                log("Error closing the Database: " + e);
                //Console.WriteLine("Error closing the Database: " + e);
            }
        }
        public bool insert(string command)
        {
            open();
            //Preform insert command
            SqlCommand myCommand = new SqlCommand(command, data);
            //Console.WriteLine("Inserting Command: " + command);
            log("Inserting Command: " + command);
            int results = 0;
            try
            {
                results = myCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                //Console.WriteLine("Failed inserting object with error: " + e);
                log("Failed inserting object with error: " + e);
            }
            close();
            return results == 1;
        }
        public bool insert(SqlCommand myCommand)
        {
            open();
            int results = 0;
            try
            {
                results = myCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                log("Failed inserting object with error: " + e);
                //Console.WriteLine("Failed inserting object with error: " + e);
            }
            close();
            return results == 1;
        }
        public bool insertObject<T>(T obj)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = data;
            sqlCommand.CommandType = System.Data.CommandType.Text;
            string command = "INSERT INTO " + obj.GetType().Name + " (";
            string values = "";
            int iteration = 0;
            foreach (var p in obj.GetType().GetProperties())
            {
                
                if (!p.Name.ToLower().Equals("id")) 
                {
                    command += p.Name;
                    values += "@" + p.Name;
                    string name = "@" + p.Name;

                    try
                    {
                        sqlCommand.Parameters.AddWithValue(name, p.GetValue(obj).ToString());
                    } catch (System.NullReferenceException e)
                    {
                        log("Result: " + e.Message);
                        log("Possible Error: Make sure the Object matches the Table in the SQL Database");
                        //Console.WriteLine("Result: " + e.Message);
                        //Console.WriteLine("Possible Error: Make sure the Object matches the Table in the SQL Database");
                    }
                    

                   // Console.WriteLine(name + ": " + p.GetValue(obj).ToString());
                    iteration++;
                    if (iteration < obj.GetType().GetProperties().Length)
                    {
                        values += ", ";
                        command += ", ";
                    }
                    else
                    {
                        command += ") VALUES (" + values + ")";
                    }
                } else
                {
                    iteration++;
                }
                
            }
            //Console.WriteLine(command);
            log(command);
            sqlCommand.CommandText = command;
            int result = 0;
            try
            {
                open();
                result = sqlCommand.ExecuteNonQuery();
                //Console.WriteLine("Result: " + result);
            }
            catch (Exception e)
            {
                log("Result: " + e);
                //Console.WriteLine("Result: " + e);
            }
            finally
            {
                close();
            }
            return (result == 1);
        }
        public bool testConnection()
        {
            try
            {
                open();
                //Console.WriteLine("Test successfull");
                log("Test successfull");
                close();
                return true;
            }
            catch (Exception E)
            {
                //Console.WriteLine("Connection failed, with exception" + E.Message);
                log("Connection failed, with exception" + E.Message);
                return false;
            }
        }
        //This needs work, if there is a field skipped (due to it not being supported, a comma is still left behind)
        public bool createObjectTable<T>(T type)
        {
            //To creat a table, the object must have an ID;
            //SUGGESTION: If it does not, maybe we can modify the class to have an ID?

            //Check if the table has an ID

            if (!checkObjForID(type))
            {
                //Console.WriteLine("Object must contain an ID field to be inserted.");
                log("Object must contain an ID field to be inserted.");
                return false;
            }
            string command = "CREATE TABLE " + type.GetType().Name + " (";
            var props = type.GetType().GetProperties();
            int iteration = 0;
            foreach (var p in props)
            {
                //Check if the field is a valid SQL Field.
                string field = findSQLField(p.PropertyType); //This is the object
                string fieldName = p.Name;                    //This is the objects name
                if (field != null)
                {
                    if (fieldName.ToLower().Equals("id"))  //This sets the primary Key.
                    {
                        command += fieldName + " " + field + " NOT NULL IDENTITY(1, 1) PRIMARY KEY";
                    } else
                    {
                        command += fieldName + " " + field;
                    }
                       

                   

                    if (iteration != props.Length - 1)
                        command += ", ";
                }
                iteration++;
                if (iteration == props.Length)
                    command += ")";
            }
            /*
              If the resulted command line does not have any commas that means 
              there is not SQL supported fields in the table so it cannot be inserted
              into the database. This is my only thought on an easy way to distinguish the two.
            */
            if (!command.Contains(","))
            {
                //Console.WriteLine("The table does not have any insertable values.");
                log("The table does not have any insertable values.");
                return false;
            }

            //Console.WriteLine(command);
            log(command);
            return insert(command);
        }
        public SqlConnection databaseConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
        public bool checkObjForID<T>(T type)
        {
            var props = type.GetType().GetProperties();
            foreach (var p in props)
            {
                if (p.Name.ToLower().Equals("id"))
                    return true;
            }
            return false;
        }
        public bool update(string command)
        {
            open();

            //Preform insert command
            SqlCommand myCommand = new SqlCommand(command, data);
            int results = 0;
            try
            {
                results = myCommand.ExecuteNonQuery();
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                //Console.WriteLine("Failed updating object. Result: " + e);
                log("Failed updating object. Result: " + e);
            }

            close();
            return results == 1;
        }
        public bool updateObject<T>(T obj, List<Object> whatFields, List<string> newValues)
        {
            List<string> swhat = new List<string>();
            for (int i = 0; i < whatFields.Count; i++)
            {
                swhat.Add(whatFields[i].GetType().Name);
            }

            return updateObject(obj, swhat, newValues);

        }
        public bool updateObject<T>(T obj, List<string> whatFields, List<string> newValues)
        {
            int ID = 0;
            //Start Developing the SQL String:
            string SQLCommand = "UPDATE " + obj.GetType().Name + " ";
            var props = obj.GetType().GetProperties();

            foreach (var p in props)
            {
                for (int i = 0; i < whatFields.Count; i++)
                {
                    if (whatFields[i].Equals("ID"))
                    {
                        //Console.WriteLine("Cannot update ID of Object");
                        log("Cannot update ID of Object");
                    }
                    else
                    {
                        if (p.Name.Equals(whatFields[i]))
                        {
                            SQLCommand += (p.Name + " = '" + newValues[0] + "'");
                        }
                        if (p.Name.Equals("ID"))
                        {
                            ID = (int)p.GetValue(obj);
                        }

                    }

                }

            }
            SQLCommand += " WHERE ID = " + ID;

            //Console.WriteLine(SQLCommand);
            log(SQLCommand);
            return update(SQLCommand);
        }
        public SqlConnection getConnection()
        {
            return data;
        }
        public Object getItem<T>(T obj, List<string> whatFields, string where, string wherevalue)
        {
            // if the what fields does not contain the ID, add it in the command string to get that value.

            Type type = obj.GetType();
            //Console.WriteLine(obj.GetType().Name);
            string command = "SELECT TOP 1 ";
            for (int i = 0; i < whatFields.Count; i++)
            {
                command += whatFields[i];
                if (i + 1 < whatFields.Count)
                    command += ", ";
                else
                    command += " ";
            }
            command += "FROM " + type.Name + " WHERE " + where + " = '" + wherevalue + "'";
           // Console.WriteLine(command);
            SqlCommand sqlcommand = new SqlCommand(command, getConnection());

            try
            {
                open();
                SqlDataReader reader = sqlcommand.ExecuteReader();
                if (!reader.HasRows)
                {
                    close();
                    //Console.WriteLine("Result: Object not found in database");
                    log("Result: Object not found in database");
                    return obj;
                }

                while (reader.Read())
                {
                    for (int i = 0; i < whatFields.Count; i++)
                    {
                        foreach (var prop in type.GetProperties())
                        {
                            string n1 = prop.Name.ToLower();
                            string n2 = whatFields[i].ToLower();
                            string qvalue = reader[whatFields[i]].ToString();
                            if (n1 == n2)
                            {
                                string fieldType = Type.GetTypeCode(prop.PropertyType).ToString();
                                string value = qvalue;

                                //Console.WriteLine(Type.GetTypeCode(prop.PropertyType) + " : " + prop.Name + " : ");
                                Object item = Convert.ChangeType(value, Type.GetTypeCode(prop.PropertyType));
                                prop.SetValue(obj, item);
                                //Console.WriteLine(item + " : " + item.GetType().Name);
                            }

                        }
                    }
                }

            }
            catch (SqlException e)
            {
                ///Console.WriteLine("Result: " + e.Message);
                log("Result: " + e.Message);
            }
            close();
            return obj;
        }
        public SqlDataReader read(string command)
        {
            SqlCommand sqlcommand = new SqlCommand(command, getConnection());
            SqlDataReader reader = null;
            try
            {
                open();
                reader = sqlcommand.ExecuteReader();

            }
            catch (SqlException e)
            {
                //Console.WriteLine("Result: " + e.Message);
                log("Result: " + e.Message);
            }
            close();
            return reader;
        }
        public List<T> getItems<T>(T obj, List<string> whatFields, string where, string wherevalue, int limit)
        {
            Type type = obj.GetType();
            string amount = "Top ";
            List<T> items = new List<T>();

            if (limit > 0)
                amount += limit + " ";
            else
                amount = " ";
            string command = "SELECT " + amount;

            for (int i = 0; i < whatFields.Count; i++)
            {
                command += whatFields[i];
                if (i + 1 < whatFields.Count)
                    command += ", ";
                else
                    command += " ";
            }
            command += "FROM " + type.Name + " WHERE " + where + " = '" + wherevalue + "'";
            SqlCommand sqlcommand = new SqlCommand(command, getConnection());
            //Console.WriteLine(command);
            log(command);
            try
            {
                open();
                SqlDataReader reader = sqlcommand.ExecuteReader();

                if (!reader.HasRows)
                {
                    close();
                    //Console.WriteLine("Result: Object not found in database");
                    log("Result: Object not found in database");
                    return new List<T>();
                }

                while (reader.Read())
                {
                    T mo = (T)Activator.CreateInstance(typeof(T));
                    for (int i = 0; i < whatFields.Count; i++)
                    {
                        foreach (var prop in type.GetProperties())
                        {
                            string n1 = prop.Name.ToLower();
                            string n2 = whatFields[i].ToLower();
                            string qvalue = reader[whatFields[i]].ToString();
                            if (n1 == n2)
                            {
                                string fieldType = Type.GetTypeCode(prop.PropertyType).ToString();
                                string value = qvalue;

                                // Console.WriteLine(Type.GetTypeCode(prop.PropertyType) + " : " + prop.Name + " : ");
                                Object item = Convert.ChangeType(value, Type.GetTypeCode(prop.PropertyType));
                                prop.SetValue(mo, item);
                                // Console.WriteLine(item + " : " + item.GetType().Name);
                            }

                        }
                    }
                    items.Add(mo);
                }

            }
            catch (SqlException e)
            {
                //Console.WriteLine("Result: " + e.Message);
                log("Result: " + e.Message);
            }
            close();


            return items;
        }
        public List<T> getLikeItems<T>(T obj, List<string> whatFields, string where, string wherevalue, int limit)
        {
            Type type = obj.GetType();
            string amount = "Top ";
            List<T> items = new List<T>();

            if (limit > 0)
                amount += limit + " ";
            else
                amount = " ";
            string command = "SELECT " + amount;

            for (int i = 0; i < whatFields.Count; i++)
            {
                command += whatFields[i];
                if (i + 1 < whatFields.Count)
                    command += ", ";
                else
                    command += " ";
            }
            command += "FROM " + type.Name + " WHERE " + where + " LIKE '%" + wherevalue + "%'";
            SqlCommand sqlcommand = new SqlCommand(command, getConnection());
            //Console.WriteLine(command);
            log(command);
            try
            {
                open();
                SqlDataReader reader = sqlcommand.ExecuteReader();

                if (!reader.HasRows)
                {
                    close();
                    //Console.WriteLine("Result: Object not found in database");
                    log("Result: Object not found in database");
                    return new List<T>();
                }

                while (reader.Read())
                {
                    T mo = (T)Activator.CreateInstance(typeof(T));
                    for (int i = 0; i < whatFields.Count; i++)
                    {
                        foreach (var prop in type.GetProperties())
                        {
                            string n1 = prop.Name.ToLower();
                            string n2 = whatFields[i].ToLower();
                            string qvalue = reader[whatFields[i]].ToString();
                            if (n1 == n2)
                            {
                                string fieldType = Type.GetTypeCode(prop.PropertyType).ToString();
                                string value = qvalue;

                                // Console.WriteLine(Type.GetTypeCode(prop.PropertyType) + " : " + prop.Name + " : ");
                                Object item = Convert.ChangeType(value, Type.GetTypeCode(prop.PropertyType));
                                prop.SetValue(mo, item);
                                // Console.WriteLine(item + " : " + item.GetType().Name);
                            }

                        }
                    }
                    items.Add(mo);
                }

            }
            catch (SqlException e)
            {
                //Console.WriteLine("Result: " + e.Message);
                log("Result: " + e.Message);
            }
            close();


            return items;
        }
        public string generateConnectionString(string ServerName, string Username, string Password, string DatabaseName, bool encrypt)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = ServerName;
            builder.UserID = Username;
            builder.Password = Password;
            builder.InitialCatalog = DatabaseName;
            builder.Encrypt = encrypt;

            Console.WriteLine(builder.ConnectionString);

            return builder.ConnectionString;
        }
    }
}
