using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp4
{
    public class CompassNodes : TableEntity
    {
        public string Node_GUID { get; set; }
        public string Parent_GUID { get; set; }
        public string Node_Name { get; set; }
        public int Service_TypeID { get; set; }
        public string TML { get; set; }
        public int Function_TypeID { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public bool Postpone { get; set; }
        public int Node_Name_Order_Number { get; set; }
        public string ConditionType { get; set; }
        public string FocusValue { get; set; }
    }
}
