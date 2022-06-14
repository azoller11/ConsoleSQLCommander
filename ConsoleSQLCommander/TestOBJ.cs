using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSQLCommander
{
    public class TestOBJ
    {

        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int OrgID { get; set; }
        public TestOBJ() { }
        public TestOBJ(int id, string name, string email, int orgId)
        {
            this.ID = id;
            this.Name = name;
            this.Email = email;
            this.OrgID = orgId;
        }

        public string display()
        {
            return "ID: " + this.ID + " Name: " + this.Name + " Email: " + this.Email + " OrgID: " + this.OrgID;
        }
    }
}
