using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ConsoleSQLCommander
{
    class Program
    {
        public static void Main(string[] args)
        {
            string cs = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Storage;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            SQLCommander sql = new SQLCommander(cs);

            SQLTestClass stc = new SQLTestClass();

            //TestOBJ t = new TestOBJ(1, "Alex Zoller", "azoller11@gmail.com", 20);
            //sql.updateObject(t, new List<string> { "Name", "ID", "orgID" }, new List<string> { "James Zoller", "5" });
            //sql.updateObject(t, new List<Object> { t.Name, t.OrgID }, new List<string> { "Zoller", "34" });
            //sql.updateObject(t, new List<Object> { t.Name }, new List<string> { "James Zoller" });

            //sql.testConnection();
            //sql.createObjectTable(typeof(TestData));
            //Console.WriteLine(sql.checkObjForID(t));


            //TestOBJ t = new TestOBJ(0, "Test Name", "azoller11@gmail.com", 20);

            //TestOBJ f = (TestOBJ) sql.getItem(new TestOBJ(), new List<string> { "Name", "ID", "OrgID", "Email" }, "ID", "1");
            //Console.WriteLine(f.display());
            //sql.insertObject(t);
            //sql.testInsertObject(t);


            //sql.modifyObjectTable(new TestData());

            //sql.deleteObject(new TestData() { Name = "Hello"}, "Name", "Item4");

            sql.clearItems(new TestData());

            for (int i = 0; i < 10; i++)
            {
                TestData data = new TestData("Item" + i + DateTime.Now.Ticks, DateTime.Now);
                sql.insertObject(data);
            }


            List<TestData> f = new List<TestData>();
            //f.AddRange(sql.getItems(new TestData(), new List<string> { "Name", "ID", "Time"}, "ID", "2", 0));
            //f.AddRange(sql.getLikeItems(new TestData(), new List<string> { "Name", "ID", "Time" }, "Time", ""+DateTime.Today.ToShortDateString(), 0));
            //foreach (TestData t in f)
               // Console.WriteLine(t.display());

            // Console.WriteLine(f.display());

            //sql.modifyObjectTable(new TestData());


            /*
            
            */

        }
    }
}
