using System;
using System.Collections.Generic;
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
        public SQLCommander(string connectionString)
        {
            this.data = databaseConnection(connectionString);
            this.connectionString = connectionString;
        }

        public bool deleteObject(object obj)
        {
            throw new NotImplementedException();
        }

        public bool deleteObjectTable(object obj)
        {
            throw new NotImplementedException();
        }

        public bool modifyObjectTable(object obj)
        {
            throw new NotImplementedException();
        }

        public string findSQLField(Type field)
        {
            //This converts the known system primative datatype into a valid SQL datatype
            string check = field.ToString();
            if (!check.Equals("System.String") && !check.Equals("System.Int32") && !check.Equals("System.DateTime"))
            {
                Console.WriteLine("Can only create tables with primative objects on field: " + check);
                return null;
            }
            if (check.Equals("System.Int32"))
                return "INT";
            if (check.Equals("System.String"))
                return "TEXT";
            if (check.Equals("System.DateTime"))
                return "DATETIME";
            return field.ToString();
        }

        public System.Data.SqlDbType findSQLType(Type field)
        {
            switch (Type.GetTypeCode(field))
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

        public bool delete(string command)
        {
            open();

            //Preform delete command

            close();

            return false;
        }

        public List<T> getItems<T>(T obj, List<string> whatFields, string where, string wherevalue, int limit)
        {
            Type type = obj.GetType();
            string amount = "Top ";
            if (limit > 0)
                amount += limit + " ";
            else
                amount = "* ";
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
             Console.WriteLine(command);



            return new List<T>();
        }

        

       
        


        //------------------Completed Methods----------------------
        public void open()
        {
            Console.WriteLine("Connecting..");
            try
            {
                data.Open();
                Console.WriteLine("Connection open");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting to Database: " + e);
            }
        }
        public void close()
        {
            try
            {
                data.Close();
                Console.WriteLine("Connection closed");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error closing the Database: " + e);
            }
        }
        public bool insert(string command)
        {
            open();
            //Preform insert command
            SqlCommand myCommand = new SqlCommand(command, data);
            Console.WriteLine("Inserting Command: " + command);
            int results = 0;
            try
            {
                results = myCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed inserting object with error: " + e);
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
                Console.WriteLine("Failed inserting object with error: " + e);
            }
            close();
            return results == 1;
        }
        public bool insertObject(object obj)
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

                    sqlCommand.Parameters.AddWithValue(name, p.GetValue(obj).ToString());

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
                Console.WriteLine("Result: " + e);
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
                Console.WriteLine("Test successfull");
                close();
                return true;
            }
            catch (Exception E)
            {
                Console.WriteLine("Connection failed, with exception" + E.Message);
                return false;
            }
        }
        //This needs work, if there is a field skipped (due to it not being supported, a comma is still left behind)
        public bool createObjectTable(Type type)
        {
            //To creat a table, the object must have an ID;
            //SUGGESTION: If it does not, maybe we can modify the class to have an ID?

            //Check if the table has an ID
            if (!checkObjForID(type))
            {
                Console.WriteLine("Object must contain an ID field to be inserted.");
                return false;
            }
            string command = "CREATE TABLE " + type.Name + " (";
            var props = type.GetProperties();
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
                Console.WriteLine("The table does not have any insertable values.");
                return false;
            }

            Console.WriteLine(command);
            return insert(command);
        }
        public SqlConnection databaseConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
        public bool checkObjForID(Type type)
        {
            var props = type.GetProperties();
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
            catch
            {
                Console.WriteLine("Failed updating object");
            }

            close();
            return results == 1;
        }
        public bool updateObject(object obj, List<Object> whatFields, List<string> newValues)
        {
            List<string> swhat = new List<string>();
            for (int i = 0; i < whatFields.Count; i++)
            {
                swhat.Add(whatFields[i].GetType().Name);
            }

            return updateObject(obj, swhat, newValues);

        }
        public bool updateObject(object obj, List<string> whatFields, List<string> newValues)
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
                        Console.WriteLine("Cannot update ID of Object");
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

            Console.WriteLine(SQLCommand);
            return false;
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
                    Console.WriteLine("Result: Object not found in database");
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
                Console.WriteLine("Result: " + e.Message);
            }
            close();
            return obj;
        }
        public Object read(string command)
        {
            SqlCommand sqlcommand = new SqlCommand(command, getConnection());
            object returnReader = null;
            try
            {
                open();
                SqlDataReader reader = sqlcommand.ExecuteReader();
                returnReader = reader;

            }
            catch (SqlException e)
            {
                Console.WriteLine("Result: " + e.Message);
            }
            close();
            return returnReader;
        }
    }
}
