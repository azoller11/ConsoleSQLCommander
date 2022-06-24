using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSQLCommander
{
    public class TestData
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }


        public TestData() { }

        public TestData(int id, string name, DateTime time)
        {
            this.ID = id;
            this.Name = name;
            this.Time = time;
        }
        public TestData(string name, DateTime time)
        {
            this.Name = name;
            this.Time = time;
        }

        public string display()
        {
            return "ID: " + this.ID + " Name: " + this.Name + " Time: " + this.Time;
        }

    }
}
