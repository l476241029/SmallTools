using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 批量注释
{
    public class OracleCode
    {
        private string conectString = string.Empty;
        private static OracleConnection con = null;
        private string mTableName = string.Empty;


        public void ConnDB()
        {
            try
            {
                var dbName = ConfigurationManager.AppSettings["dbName"].ToString();
                if (string.IsNullOrEmpty(dbName))
                {
                    Console.WriteLine("请配置好数据库连接信息");
                    return;
                }
                con = new OracleConnection(dbName);
                con.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
           
        }

        public List<ColumnInfo> GetTables()
        {
            var sql = $@"
select a.* from user_tab_comments a
inner join USER_TABLES t
    on a.table_name = t.table_name
where a.comments is null";
            OracleCommand com = new OracleCommand(sql, con);
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            var list = new List<ColumnInfo>();
            OracleDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                var info = new ColumnInfo();
                info.TableName = reader["table_name"].ToString();
                info.Comments = reader["comments"].ToString();
                list.Add(info);

            }
            reader.Close();
            con.Close();
            return list;
        }

        public int UpdateTable(string tableName, string str)
        {
            var sql = $"comment on table {tableName} is '{str} '";
            OracleCommand com = new OracleCommand(sql, con);
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            return com.ExecuteNonQuery();
        }

        public List<ColumnInfo> GetColumnInfo()
        {
            string strSQL = $@"SELECT t.table_name,
       t.column_name,
       t.comments  FROM USER_COL_COMMENTS   t 
where t.comments is  null 
and table_name in (select table_name from USER_TABLES) ";
            OracleCommand com = new OracleCommand(strSQL, con);
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            var list = new List<ColumnInfo>();
            OracleDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                var info = new ColumnInfo();
                info.TableName = reader["table_name"].ToString();
                info.ColumnName = reader["column_name"].ToString();
                info.Comments = reader["comments"].ToString();
                list.Add(info);

            }
            reader.Close();
            con.Close();
            return list;
        }

        public int UpdateColumn(string tableName, string col, string str)
        {
            var sql = $"comment on column {tableName}.{col} is '{str} '";
            OracleCommand com = new OracleCommand(sql, con);
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            return com.ExecuteNonQuery();
        }
    }

    public class ColumnInfo
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string Comments { get; set; }
    }
}
