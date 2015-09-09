using DBOrm.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBOrm.TableAccessors
{
    public abstract class TableRowsBase
    {
        #region Variables
        private DatabaseFactory databridge;
        private Type _childtype;
        private int _currentindex = -1;
        private TableRowBase _currentrow;
        private DataTable _table;
        private string _selectcommand;
        private DataRow _currentDatarow;
        #endregion

        #region Constructors

        protected TableRowsBase() { }
        protected TableRowsBase(DatabaseFactory dbfactory)
        {
            this.DataBridge = dbfactory;
        }
        protected TableRowsBase(Type objecttype)
        {
            this.ObjectType = objecttype;
        }

        protected TableRowsBase(Type objecttype, DatabaseFactory dbfactory)
            : this(dbfactory)
        {
            this.ObjectType = objecttype;
        }

        protected TableRowsBase(DataTable dt, DatabaseFactory dbfactory)
            : this(dbfactory)
        {
            this.Table = dt;
        }

        protected TableRowsBase(Type objecttype, DataTable dt, DatabaseFactory dbfactory)
            : this(dt, dbfactory)
        {
            this.ObjectType = objecttype;
        }

        protected TableRowsBase(string cmdtext, Type objecttype, DataTable dt, DatabaseFactory dbfactory)
            : this(objecttype, dt, dbfactory)
        {
            this.SetTable(cmdtext);
        }
        #endregion

        #region Properties

        public int Count
        {
            get
            {
                if (this.Table == null)
                    return 0;
                return this.Table.Rows.Count;
            }
        }

        public TableRowBase this[int index]
        {
            get
            {
                if (index < this.Count)
                {
                    if (this.CurrentIndex != index)
                    {
                        this.CurrentRow = (TableRowBase)Activator.CreateInstance(this.ObjectType,
                            this.ConstructorArguments(index));
                        this.CurrentIndex = index;
                    }

                    return this.CurrentRow;
                }
                return null;
            }
        }

        public Type ObjectType
        {
            get { return this._childtype; }
            set { this._childtype = value; }
        }

        private object[] ConstructorArguments(int index)
        {
            object[] args = new object[2];
            this.CurrentDataRow = this.Table.Rows[index];
            args[0] = this.CurrentDataRow;
            args[1] = this.DBFactory;
            return args;
        }

        protected DatabaseFactory DBFactory
        {
            get
            {
                if (this.databridge == null)
                    throw new ApplicationException("Cannot connect to the database. Because databridge is not set!");
                return this.databridge;
            }
            set { this.databridge = value; }
        }

        public string SelectCommandText
        {
            get
            {
                if (this._selectcommand == null && this.Table != null)
                {
                    try
                    {
                        this._selectcommand = this.DBFactory.GetSelectCommandText(this.Table);
                    }
                    catch
                    {
                        this._selectcommand = "";
                        foreach (DataColumn dc in this.Table.Columns)
                            this._selectcommand += this._selectcommand == "" ? "Select " + dc.ColumnName : ", " + dc.ColumnName;
                        this._selectcommand += " " + this.Table.TableName;
                    }
                }
                return this._selectcommand;
            }
        }

        public DataRow CurrentDataRow
        {
            get { return this._currentDatarow; }
            set { this._currentDatarow = value; }
        }

        protected DatabaseFactory DataBridge
        {
            get
            {
                if (this.databridge == null)
                    throw new ApplicationException("Cannot connect to the database. Because databridge is not set!");
                return this.databridge;
            }
            set { this.databridge = value; }
        }

        public DataTable Table
        {
            get { return this._table; }
            set { this._table = value; }
        }

        public TableRowBase CurrentRow
        {
            get { return this._currentrow; }
            set { this._currentrow = value; }
        }

        public int CurrentIndex
        {
            get { return this._currentindex; }
            set { this._currentindex = value; }
        }
        #endregion

        #region Methods

        public void Save()
        {
            for (int i = 0; i < this.Count; i++)
                this[i].Save();
        }

        public void RemoveAll()
        {
            for (int i = 0; i < this.Count; i++)
                this[i].Remove();
        }

        public virtual void Remove(int Index)
        {
            this[Index].Remove();
        }

        public void SetTable(string cmdText)
        {
            if (this.Table == null)
                this.Table = this.DBFactory.ExecuteTable(cmdText);
            else
                this.FillTable(cmdText);
        }

        public void SetTable(IDbCommand cmd)
        {
            if (this.Table == null)
                this.Table = this.DBFactory.ExecuteTable(cmd);
            else
                this.FillTable(cmd);
        }

        public void FillTable(string cmdText)
        {
            this.DBFactory.FillTable(cmdText, this.Table);
        }

        public void FillTable(IDbCommand cmd)
        {
            this.DBFactory.FillTable(cmd, this.Table);
        }

        public virtual void Dispose()
        {
            this.CurrentRow = null;
            this.CurrentIndex = -1;
            this.ObjectType = null;
            this.Table = null;
            this.DBFactory = null;
        }

        public virtual bool Dirty
        {
            set
            {
                for (int i = 0; i < this.Count; i++)
                    this[i].Dirty = value;
            }
        }

        public DataRow NewRow()
        {
            if (this.Table != null)
                return this.Table.NewRow();
            return null;
        }

        public void AddRow(DataRow row)
        {
            if (this.Table != null)
                this.Table.Rows.Add(row);
        }

        public int UpdateTable()
        {
            if (this.Table != null && this.DBFactory != null)
                return this.DBFactory.UpdateTable(this.SelectCommandText, this.Table);
            return 0;
        }

        public int UpdateInsertedRows()
        {
            if (this.Table != null && this.DBFactory != null)
                return this.DBFactory.UpdateInsertedRow(this.SelectCommandText, this.Table);
            return 0;
        }

        public int UpdateDeletedRow()
        {
            if (this.Table != null && this.DBFactory != null)
                return this.DBFactory.UpdateDeletedRow(this.SelectCommandText, this.Table);
            return 0;
        }

        public int UpdateModifiedRow()
        {
            if (this.Table != null && this.DBFactory != null)
                return this.DBFactory.UpdateModifiedRow(this.SelectCommandText, this.Table);
            return 0;
        }

        public DataRow Find(object key)
        {
            if (this.Table != null)
            {
                this.CurrentDataRow = this.Table.Rows.Find(key);
                return this.CurrentDataRow;
            }
            return null;
        }

        public DataRow Find(object[] keys)
        {
            if (this.Table != null)
            {
                this.CurrentDataRow = this.Table.Rows.Find(keys);
                return this.CurrentDataRow;
            }
            return null;
        }

        public DataTable FetchAll()
        {
            if (this.Table != null)
                this.SetTable("Select * From " + this.Table.TableName);
            return this.Table;
        }

        #endregion
    }
}
