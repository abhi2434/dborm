using DBOrm.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBOrm.TableAccessors
{
    public enum RowType
    {
        ORIGINAL,
        TRANSACTIONAL
    };
    public class TableRowBase
    {
        #region Variables
        private DataRow row;
        private List<string> keyColumns = new List<string>();
        private bool isNewRow;
        private bool isDirty;
        private DatabaseFactory dataFactory;

        private Dictionary<string, string> columnDictionary = new Dictionary<string, string>();

        private string tableName = null;

        #endregion

        #region Constructors

        protected TableRowBase() { }
        protected TableRowBase(DatabaseFactory dbfactory) { this.DataBridge = dbfactory; }

        protected TableRowBase(string keys)
        {
            this.SetKeys(keys);
        }

        protected TableRowBase(DataRow dr, List<string> ar)
        {
            this.Row = dr;
            this.KeyColumns = ar;
        }

        protected TableRowBase(DataRow dr)
        {
            this.Row = dr;
        }

        protected TableRowBase(DataRow dr, string keys, string tablename)
        {
            this.SetKeys(keys);
            this.Row = dr;
            this.TableName = tablename;
        }


        #endregion

        #region Properties

        public bool IsExists
        {
            get { return !this.NewRow; }
            set { this.NewRow = !value; }
        }

        protected string TableName
        {
            get { return this.tableName; }
            set
            {
                this.tableName = value;
                if (row != null)
                {
                    row.Table.TableName = value;
                }
            }
        }

        protected bool NewRow
        {
            get { return isNewRow; }
            set { isNewRow = value; }
        }

        public bool Dirty
        {
            get { return isDirty; }
            set { isDirty = value; }
        }

        public void SetDirty(bool val)
        {
            this.Dirty = val;
        }

        protected object this[string columnName]
        {
            get { return this.Row[columnName]; }
            set
            {
                if (this.ActualValues.ContainsKey(columnName.ToUpper()) == false)
                    this.ActualValues.Add(columnName.ToUpper(), this.Row[columnName].ToString());
                this.Row[columnName] = value;

            }
        }

        protected object this[string columnName, bool validateDBNull]
        {
            get
            {
                if (validateDBNull && this.Row[columnName] == DBNull.Value)
                {
                    switch (this.Column(columnName).DataType.Name)
                    {
                        case "String":
                            return "";
                        case "Boolean":
                            return false;
                        case "DateTime":
                            return DateTime.Now;
                        case "Int16":
                        case "Int32":
                        case "Int64":
                        case "Decimal":
                        case "Single":
                        case "Double":
                        case "float":
                        case "long":
                            return 0;
                    }
                }
                return this.Row[columnName];
            }
            set
            {
                if (this.ActualValues.ContainsKey(columnName.ToUpper()) == false)
                    this.ActualValues.Add(columnName.ToUpper(), this.Row[columnName].ToString());
                this.Row[columnName] = value;

            }
        }

        private object this[string columnName, RowType rowtyp]
        {
            get
            {
                if (rowtyp == RowType.TRANSACTIONAL)
                    return this.Row[columnName];
                else
                    return this.ActualValues.ContainsKey(columnName.ToUpper()) ? this.ActualValues[columnName.ToUpper()].ToString() : "";
            }
        }

        protected int KeyCount
        {
            get { return this.keyColumns.Count; }
        }

        protected DatabaseFactory DataBridge
        {
            get
            {
                if (this.dataFactory == null)
                    throw new ApplicationException("Cannot connect to the database. Because databridge is not set!");
                return this.dataFactory;
            }
            set { this.dataFactory = value; }
        }


        public DataRow Row
        {
            get { return row; }
            set { row = value; }
        }

        private Dictionary<string, string> ActualValues
        {
            get { return columnDictionary; }
        }

        protected bool IsRowExists
        {
            get { return (this.Row.Table.Rows.Count > 0); }
        }

        protected List<string> KeyColumns
        {
            get { return this.keyColumns; }
            set { this.keyColumns = value; }
        }
        protected int ColumnCount
        {
            get { return this.Row.Table.Columns.Count; }
        }

        private string InsertQuery
        {
            get
            {
                string sUpdateQuery = string.Empty, sValuesQuery = string.Empty;
                sUpdateQuery = "Insert Into " + TableName + "(";
                sValuesQuery = " Values(";
                for (int i = 0; i < this.ColumnCount; i++)
                {
                    if (this.Column(i).AutoIncrement == false)
                    {
                        if (this[ColumnName(i)] != DBNull.Value)
                        {
                            string strcolname = ColumnName(i);
                            if (strcolname.Equals("POSITION")) strcolname = "[POSITION]";

                            sUpdateQuery += strcolname + ",";
                            if (Column(i).DataType.Name == "String")
                                sValuesQuery += " '" + this[ColumnName(i)].ToString().Replace("'", "''") + "',";
                            else if (Column(i).DataType.Name == "DateTime")
                            {
                                if (this.DataBridge.Provider == ProviderType.OLEDB)
                                    sValuesQuery += "#" + this[ColumnName(i)] + "#,";
                                else
                                    sValuesQuery += "'" + this[ColumnName(i)] + "',";
                            }
                            else if (Column(i).DataType.Name == "Boolean")
                                sValuesQuery += Convert.ToString(((bool)this[ColumnName(i)] == true ? 1 : 0)) + ",";
                            else
                                sValuesQuery += this[ColumnName(i)] + ",";
                        }
                    }
                }
                if (sUpdateQuery.Substring(sUpdateQuery.Length - 1, 1) == ",")
                {
                    sUpdateQuery = sUpdateQuery.Substring(0, sUpdateQuery.Length - 1);
                    sUpdateQuery += ") ";
                }
                if (sValuesQuery.Substring(sValuesQuery.Length - 1, 1) == ",")
                {
                    sValuesQuery = sValuesQuery.Substring(0, sValuesQuery.Length - 1);
                    sValuesQuery += ") ";
                }
                sUpdateQuery += sValuesQuery;
                return sUpdateQuery;
            }
        }

        private string UpdateQuery
        {
            get
            {
                string sUpdateQuery = string.Empty, sValuesQuery = string.Empty;
                sUpdateQuery = "Update " + this.TableName + " Set ";
                for (int i = 0; i < ColumnCount; i++)
                {
                    if (this.IsColumnDataChanged(ColumnName(i)))
                    {
                        if (Column(i).AutoIncrement == false)
                        {
                            int j;
                            for (j = 0; j < KeyCount; j++)
                                if (ColumnName(i) == Keys(j)) break;

                            if (j >= KeyCount)
                                if (this[ColumnName(i)] != DBNull.Value)
                                {
                                    string strcolname = ColumnName(i);
                                    if (strcolname.Equals("POSITION")) strcolname = "[POSITION]";

                                    if (Column(i).DataType.Name == "String")
                                        sUpdateQuery += strcolname + "='" + this[ColumnName(i)].ToString().Replace("'", "''") + "',";
                                    else if (Column(i).DataType.Name == "DateTime")
                                    {
                                        if (this.DataBridge.Provider == ProviderType.OLEDB)
                                            sUpdateQuery += strcolname + "=#" + this[ColumnName(i)] + "#,";
                                        else
                                            sUpdateQuery += strcolname + "='" + this[ColumnName(i)] + "',";
                                    }
                                    else if (Column(i).DataType.Name == "Boolean")
                                        sUpdateQuery += strcolname + "=" + Convert.ToString(((bool)this[ColumnName(i)] == true ? 1 : 0)) + ",";
                                    else
                                        sUpdateQuery += strcolname + "=" + this[ColumnName(i)] + ",";
                                }
                        }
                    }
                }

                if (sUpdateQuery.Substring(sUpdateQuery.Length - 1, 1) == ",")
                    sUpdateQuery = sUpdateQuery.Substring(0, sUpdateQuery.Length - 1);
                else
                    return "";

                sUpdateQuery += this.FilterQuery;
                return sUpdateQuery;
            }
        }

        private string FilterQuery
        {
            get
            {
                string sFilterQuery = "", sValuesQuery = "";
                DataColumn dtColumn;
                for (int i = 0; i < KeyCount; i++)
                {
                    dtColumn = Column(Keys(i));
                    if (dtColumn.DataType.Name == "String")
                        sValuesQuery = dtColumn.ColumnName + "='" + this[dtColumn.ColumnName] + "'";
                    else if (dtColumn.DataType.Name == "DateTime")
                    {
                        if (this.DataBridge.Provider == ProviderType.OLEDB)
                            sValuesQuery = dtColumn.ColumnName + "=#" + this[dtColumn.ColumnName] + "#";
                        else
                            sValuesQuery = dtColumn.ColumnName + "='" + this[dtColumn.ColumnName] + "'";
                    }
                    else if (dtColumn.DataType.Name == "Boolean")
                        sValuesQuery = dtColumn.ColumnName + "=" + Convert.ToString(((bool)this[dtColumn.ColumnName]) == true ? 1 : 0);
                    else
                        sValuesQuery = dtColumn.ColumnName + "=" + this[dtColumn.ColumnName];
                    if (i == 0)
                    {
                        sFilterQuery += " Where ";
                        sFilterQuery += sValuesQuery;
                    }
                    else
                        sFilterQuery += " And " + sValuesQuery;
                }
                return sFilterQuery;
            }
        }

        private string FilterExpressionForLog
        {
            get
            {
                string sFilterQuery = string.Empty;
                DataColumn dtColumn;
                for (int i = 0; i < KeyCount; i++)
                {
                    dtColumn = Column(Keys(i));
                    sFilterQuery += dtColumn.ColumnName + "=" + this[dtColumn.ColumnName].ToString() + " ";
                }
                return sFilterQuery;
            }
        }

        protected string SearchQuery
        {
            get
            {
                string sSearchQuery = string.Empty;
                bool bflag = false;
                sSearchQuery = "Select * From " + this.TableName;

                for (int i = 0; i < ColumnCount; i++)
                {
                    if (Column(i).AutoIncrement == true && (long)this[ColumnName(i)] == 0)
                        continue;

                    if (this[ColumnName(i)] != DBNull.Value)
                    {
                        if (bflag == false)
                        {
                            sSearchQuery += " Where ";
                            bflag = true;
                        }
                        else
                            sSearchQuery += " And ";

                        if (Column(i).DataType.Name == "String")
                            sSearchQuery += ColumnName(i) + " Like '" + this[ColumnName(i)].ToString().ToUpper() + "%'";
                        else if (Column(i).DataType.Name == "DateTime")
                        {
                            if (this.DataBridge.Provider == ProviderType.OLEDB)
                                sSearchQuery += ColumnName(i) + "=#" + this[ColumnName(i)] + "#";
                            else
                                sSearchQuery += ColumnName(i) + "='" + this[ColumnName(i)] + "'";
                        }
                        else if (Column(i).DataType.Name == "Boolean")
                            sSearchQuery += ColumnName(i) + "=" + Convert.ToString(((bool)this[ColumnName(i)] == true ? 1 : 0));
                        else
                            sSearchQuery += ColumnName(i) + "=" + this[ColumnName(i)];
                    }
                }
                return sSearchQuery;
            }
        }

        #endregion

        #region Methods

        #region SetRows

        protected void SetRow(string cmdText)
        {
            DataTable dt = this.DataBridge.ExecuteTable(cmdText);
            if (dt.Rows.Count == 0)
            {
                this.Row = dt.NewRow();
                this.NewRow = true;
            }
            else
                this.Row = dt.Rows[0];
        }

        protected void SetRow(string cmdText, string keys, string tablename)
        {
            DataTable dt = this.DataBridge.ExecuteTable(cmdText);
            if (dt.Rows.Count == 0)
            {
                this.Row = dt.NewRow();
                this.NewRow = true;
            }
            else
                this.Row = dt.Rows[0];
            this.TableName = tablename;
            this.SetKeys(keys);
        }

        protected void SetRow(string cmdText, string keys, string tablename, bool maintainlog, GenericDataFactory dbfactory)
        {
            this.DataBridge = dbfactory;

            DataTable dt = this.DataBridge.ExecuteTable(cmdText);
            if (dt.Rows.Count == 0)
            {
                this.Row = dt.NewRow();
                this.NewRow = true;
            }
            else
                this.Row = dt.Rows[0];

            this.TableName = tablename;
            this.SetKeys(keys);
        }

        protected void SetRow(string cmdText, string keys, string tablename, bool maintainlog)
        {
            DataTable dt = this.DataBridge.ExecuteTable(cmdText);
            if (dt.Rows.Count == 0)
            {
                this.Row = dt.NewRow();
                this.NewRow = true;
            }
            else
                this.Row = dt.Rows[0];

            this.TableName = tablename;
            this.SetKeys(keys);
        }

        protected void SetRow(string cmdText, string keys, string tablename, bool maintainlog, string log_Table_Name)
        {
            DataTable dt = this.DataBridge.ExecuteTable(cmdText);
            if (dt.Rows.Count == 0)
            {
                this.Row = dt.NewRow();
                this.NewRow = true;
            }
            else
                this.Row = dt.Rows[0];

            this.TableName = tablename;
            this.SetKeys(keys);
        }

        private void SetKeys(string keys)
        {
            string[] strKeys = keys.Split(',');
            for (int i = 0; i < strKeys.Length; i++)
                this.KeyColumns.Add(strKeys[i].Trim().ToUpper());
        }

        protected void SetRow(IDbCommand cmd, string keys, string tableName, DatabaseFactory dbfactory)
        {
            this.DataBridge = dbfactory;
            DataTable dt = this.DataBridge.ExecuteTable(cmd);
            if (dt.Rows.Count == 0)
            {
                this.Row = dt.NewRow();
                this.NewRow = true;
            }
            else
                this.Row = dt.Rows[0];
            this.TableName = tableName;
            this.SetKeys(keys);
        }

        #endregion

        #region Column Methods

        protected bool IsColumnDataChanged(string columnname)
        {
            if (this.ActualValues.ContainsKey(columnname.ToUpper()) && this[columnname].ToString() != this[columnname, RowType.ORIGINAL].ToString())
                return true;
            return false;
        }


        private string ColumnName(int Index)
        {
            return this.GetColumn(Index).ColumnName.ToUpper();
        }


        private DataColumn Column(int Index)
        {
            return this.GetColumn(Index);
        }


        private DataColumn Column(string colName)
        {
            return this.GetColumn(colName);
        }


        private DataColumn GetColumn(int Index)
        {
            return this.Row.Table.Columns[Index];
        }


        private DataColumn GetColumn(string colName)
        {
            return this.Row.Table.Columns[colName];
        }


        #endregion

        #region Column Value Methods

        protected object GetColumnValue(string sColumnName)
        {
            return row[sColumnName];
        }


        protected object GetColumnValue(int ColumnIndex)
        {
            return row[ColumnIndex];
        }


        protected void SetColumnValue(int colIndex, object Value)
        {
            this[this.ColumnName(colIndex)] = Value;
        }


        protected void SetColumnValue(string sColumnName, object Value)
        {
            this[sColumnName] = Value;
        }


        #endregion

        #region Data Update Methods

        public virtual void Update()
        {
            string sQuery;
            if (!this.Dirty)
            {
                if (this.NewRow == true)
                {
                    sQuery = this.InsertQuery;
                }
                else
                {
                    sQuery = this.UpdateQuery;
                }

                if (sQuery != "")
                {
                    this.DataBridge.ExecuteNonQuery(sQuery);
                }
            }
        }

        public virtual void Remove()
        {
            string sSql = "Delete From " + this.TableName + this.FilterQuery;
            if (!this.Dirty)
            {
                this.DataBridge.ExecuteNonQuery(sSql);
            }
        }

        public virtual void Save()
        {
            this.Update();
        }

        private string GetAllColumnValues()
        {
            string sColValues = "";
            for (int i = 0; i < ColumnCount; i++)
            {
                sColValues += ColumnName(i) + "=" + this[ColumnName(i)].ToString() + " ";
            }
            return sColValues;
        }

        #endregion

        #region Key Methods

        protected string Keys(int Index)
        {
            return ((string)this.KeyColumns[Index]).ToUpper();
        }

        protected void AddKey(string sKey)
        {
            this.KeyColumns.Add(sKey.Trim().ToUpper());
        }

        protected void AddKeys(string skeys)
        {
            this.SetKeys(skeys);
        }


        #endregion

        private void SetLogValues(out string prev_values, out string new_values, out string filter)
        {
            prev_values = new_values = filter = string.Empty;

            for (int i = 0; i < ColumnCount; i++)
            {
                if (this.IsColumnDataChanged(ColumnName(i)) && Column(i).AutoIncrement == false)
                {
                    int j;
                    for (j = 0; j < KeyCount; j++)
                        if (ColumnName(i) == Keys(j)) break;

                    if (j >= KeyCount)
                        if (this[ColumnName(i)] != DBNull.Value)
                        {
                            if (this[ColumnName(i), RowType.ORIGINAL].ToString() != "") prev_values += ColumnName(i) + "=" + this[ColumnName(i), RowType.ORIGINAL].ToString() + " ";
                            new_values += ColumnName(i) + "=" + this[ColumnName(i), RowType.TRANSACTIONAL].ToString() + " ";
                        }
                }
            }
            filter = this.FilterExpressionForLog;
        }

        public virtual void Dispose()
        {
            this.row = null;
            this.keyColumns.Clear();
            this.keyColumns = null;

            this.dataFactory = null;
            this.columnDictionary.Clear();
            this.columnDictionary = null;
        }


        #endregion

    }
}
