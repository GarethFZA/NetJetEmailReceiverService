using Microsoft.ApplicationBlocks.Data;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Charismatech.MessageQueueClasses
{
    class SqlDb
    {
        internal static SqlConnection getDBConn()
        {
            SqlConnection ret = null;

            ret = new SqlConnection(getDBConnString());
            ret.Open();
            using (SqlCommand cmd = new SqlCommand("set dateformat ymd", ret))
            {
                cmd.ExecuteNonQuery();
            }
            return ret;
        }

        public static string getDBConnString()
        {
            //return ConfigurationManager.AppSettings["DBConnectionString"];
            return ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString;
            //return IO.DBConnectionString;
        }
        public static string getDBConnString(string Name)
        {
            //return ConfigurationManager.AppSettings["DBConnectionString"];
            return ConfigurationManager.ConnectionStrings[Name].ConnectionString;
            //return IO.DBConnectionString;
        }

        internal static DataSet GetDataSet(string sql, string WhichDB)
        {
            return SqlHelper.ExecuteDataset(getDBConnString(), CommandType.Text, sql);
        }
        internal static DataSet GetDataSet(string sql)
        {
            return SqlHelper.ExecuteDataset(getDBConnString(), CommandType.Text, sql);
        }
        internal static DataSet GetDataSet(string sql, params SqlParameter[] sqlParams)
        {
            bool hasRows = false;
            return GetDataSet(sql, out hasRows, null, sqlParams);
        }
        internal static DataSet GetDataSet(string sql, out bool hasRows, params SqlParameter[] sqlParams)
        {
            hasRows = false;
            return GetDataSet(sql, out hasRows, null, sqlParams);
        }
        internal static DataSet GetDataSet(string sql, out bool hasRows, SqlTransaction tran, params SqlParameter[] sqlParams)
        {
            DataSet ds;
            hasRows = false;
            if (tran != null)
                ds = SqlHelper.ExecuteDataset(tran, CommandType.Text, sql, sqlParams);
            else
                ds = SqlHelper.ExecuteDataset(getDBConnString(), CommandType.Text, sql, sqlParams);

            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    hasRows = true;
                }
            }
            return ds;
        }
        internal static DataSet GetDataSet(string sql, out bool hasRows)
        {
            SqlParameter[] sqlParams = null;
            return GetDataSet(sql, out hasRows, sqlParams);
        }

        public static bool execSQL(string sql)
        {
            bool retval;
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, getDBConn()))
                {
                    cmd.ExecuteNonQuery();
                }
                retval = true;
            }
            catch
            {
                retval = false;
            }
            return retval;
        }

        internal static bool execSQL(string sql, params SqlParameter[] sqlParams)
        {
            bool retval;
            try
            {
                SqlHelper.ExecuteNonQuery(getDBConn(), CommandType.Text, sql, sqlParams);
                retval = true;
            }
            catch (Exception ex)
            {
                retval = false;
                throw ex;
            }
            return retval;
        }

        public static bool execSQL(string sql, string WhichDB)
        {
            string errorMsg = "";
            return execSQL(sql, WhichDB, out errorMsg);
        }
        public static bool execSQL(string sql, string WhichDB, out string errorMsg)
        {
            bool retval;
            errorMsg = "";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, getDBConn()))
                {
                    cmd.ExecuteNonQuery();
                }
                retval = true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                retval = false;
            }
            return retval;
        }

    }
}
