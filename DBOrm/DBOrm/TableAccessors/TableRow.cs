using DBOrm.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBOrm.TableAccessors
{
    public abstract class TableRow : TableRowBase
    {
         public TableRow()
        {
            this.Initialize();
        }

        public TableRow(DatabaseFactory dbfactory) : this()
        {
            base.DataBridge = dbfactory;
        }

        public TableRow(DataRow dr, DatabaseFactory dbfactory) : this(dbfactory)
        {
            base.Row = dr;            
        }

        public TableRow(Guid id, DatabaseFactory dbfactory)
            : this(dbfactory)
        {
            string sql = "Select * From " + base.TableName + " Where " + base.Keys(0) + "=" + id;
            base.SetRow(sql);
            this.Id = id;
        }

        public abstract void Initialize();

        public abstract Guid Id { get; set;}


        public override void Save()
        {
            if (base.NewRow)
                this.Id = Guid.NewGuid();
            base.Save();
        }
    }
}
