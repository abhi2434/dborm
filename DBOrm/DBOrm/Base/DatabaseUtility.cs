using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBOrm.Base
{
    public enum etFilterType
    {
        EQUAL, IN, LIKE
    };
    public class DatabaseUtility
    {
        DataColumn[] _filtercolumns;
        DataTable _filtertable;
        etFilterType _filtertype = etFilterType.EQUAL;
        Dictionary<string, object> _filtervalues;
        DatabaseFactory _dbfactory;

        public DatabaseFactory DBFactory
        {
            get { return this._dbfactory; }
            set { this._dbfactory = value; }
        }

        public DataTable FilterTable
        {
            get { return this._filtertable; }
            set { this._filtertable = value; }
        }

        public Dictionary<string, object> FilterValues
        {
            get
            {
                if (this._filtervalues == null)
                    this._filtervalues = new Dictionary<string, object>();
                return this._filtervalues;
            }
        }

        public etFilterType FilterType
        {
            get { return this._filtertype; }
            set { this._filtertype = value; }
        }

        private string FilterTypeString
        {
            get
            {
                switch (this.FilterType)
                {
                    case etFilterType.IN:
                        return " In(";
                    case etFilterType.LIKE:
                        return " Like ";
                    default:
                        return " =";
                }
            }
        }

        public DataColumn[] FilterColumns
        {
            get { return this._filtercolumns; }
            set { this._filtercolumns = value; }
        }

        public void FillTables(DataTable dt, string joinquery, DataRelation prel, bool fl)
        {
            string tablename = dt.TableName;
            string tablefeilds = "";
            foreach (DataColumn dc in dt.Columns)
                if (!dc.ReadOnly)
                    tablefeilds += tablefeilds == "" ? tablename + "." + dc.ColumnName : "," + tablename + "." + dc.ColumnName;
            string sql = "Select " + tablefeilds + " From ";
            if (joinquery == "")
                joinquery = tablename;
            else
                joinquery += " Inner Join " + tablename + " On ";
            if (prel != null)
            {
                joinquery += "(";
                for (int i = 0; i < prel.ParentColumns.Length; i++)
                {
                    if (i > 0)
                        joinquery += " And ";
                    joinquery += prel.ParentTable.TableName + "." + prel.ParentColumns[i].ColumnName +
                                 "=" + tablename + "." + prel.ChildColumns[i].ColumnName;
                }
                joinquery += ")";
            }
            sql += joinquery + this.FilterQuery;
            if (fl)
                this.DBFactory.FillTable(sql, dt);

            foreach (DataRelation rel in dt.ChildRelations)
                this.FillTables(rel.ChildTable, joinquery, rel, true);
        }

        public string FilterQuery
        {
            get
            {
                string filterQuery = "";
                if (this.FilterTable != null)
                    foreach (DataColumn dc in this.FilterColumns)
                        if (this.FilterValues.ContainsKey(dc.ColumnName))
                        {
                            string filtervalue = this.FilterValues[dc.ColumnName].ToString();
                            filterQuery += (filterQuery == "" ? " Where " : " And ");
                            filterQuery += this.FilterTable.TableName + "." + dc.ColumnName + " " + this.FilterTypeString;
                            filterQuery += filtervalue;
                            if (this.FilterType == etFilterType.IN)
                                filterQuery += ")";
                        }
                return filterQuery;
            }
        }
    }
}
