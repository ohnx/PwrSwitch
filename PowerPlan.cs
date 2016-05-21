using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwrSwitch
{
    public class PowerPlan
    {
        public Guid guid   { get; }
        public string name { get; }

        public PowerPlan(Guid guid, string name)
        {
            this.guid = guid;
            this.name = name;
        }
    }
}
