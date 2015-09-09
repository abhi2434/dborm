using DBOrm.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBOrm.TableAccessors
{
    public class TableRows : TableRowsBase
    {
         public TableRows()
        {
            this.Initialize();            
        }

         public TableRows(DatabaseFactory dbfactory)
             : this()
        {
            base.DBFactory = dbfactory;
        }

         public abstract void Initialize();
    }
}
