using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

public class WFXIMSAPISQLHelper
{ 
        public string mConnectionString;
    private int mCommandTimeOut = 7200;

    public WFXIMSAPISQLHelper(string ConnectionString)
    {
        mConnectionString = ConnectionString;
    }
    private void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
    {
        if (command == null) throw new ArgumentNullException("command");
        if (commandParameters != null)
        {
            foreach (SqlParameter p in commandParameters)
            {
                if (p != null)
                {
                    // Check for derived output value with no value assigned
                    if ((p.Direction == ParameterDirection.InputOutput ||
                        p.Direction == ParameterDirection.Input) &&
                        (p.Value == null))
                    {
                        p.Value = DBNull.Value;
                    }
                    command.Parameters.Add(p);
                }
            }
        }
    }
    private SqlConnection GetConnection()
    {
        var vConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[mConnectionString];
        if (vConnectionString == null && mConnectionString.IndexOf("Report") > 0)
            vConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[mConnectionString.Replace("Report", "")];
        string strConnectionString;
        if (vConnectionString != null)
        {
            strConnectionString = vConnectionString.ToString();
        }
        else
        {
            strConnectionString = mConnectionString;
        }
        if (strConnectionString == null || strConnectionString.Length == 0) throw new ArgumentNullException("connectionString");
        SqlConnection Conn;
        Conn = new SqlConnection(strConnectionString);
        return Conn;
    }
    private void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, out bool mustCloseConnection)
    {
        if (command == null) throw new ArgumentNullException("command");
        if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

        // If the provided connection is not open, we will open it
        if (connection.State != ConnectionState.Open)
        {
            mustCloseConnection = true;
            connection.Open();
        }
        else
        {
            mustCloseConnection = false;
        }

        // Associate the connection with the command
        command.Connection = connection;

        // Set the command text (stored procedure name or SQL statement)
        command.CommandText = commandText;

        // If we were provided a transaction, assign it
        if (transaction != null)
        {
            if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            command.Transaction = transaction;
        }

        // Set the command type
        command.CommandType = commandType;
        command.CommandTimeout = mCommandTimeOut;
        // Attach the command parameters if they are provided
        if (commandParameters != null)
        {
            AttachParameters(command, commandParameters);
        }
        return;
    }
    private string GetOutPutValue(SqlCommand cmd, string parameterName)
    {
        string retVal = string.Empty;
        foreach (SqlParameter p in cmd.Parameters)
        {
            if (p != null)
            {
                if (p.Direction == ParameterDirection.Output && p.ParameterName == parameterName)
                {
                    retVal = p.Value.ToString();
                }
            }
        }
        return retVal;
    }
    private void TransExecuteNonQuery(SqlTransaction transaction, string spName, ref string errorMsg, SqlParameter[] commandParameters)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, transaction.Connection, transaction, CommandType.StoredProcedure, spName, commandParameters, out mustCloseConnection);

        // Finally, execute the command
        int roweffected = cmd.ExecuteNonQuery();
        errorMsg = GetOutPutValue(cmd, "@response");
    }
    private void NonTransExecuteQuery(string spName, ref string retVal, SqlParameter[] commandParameters, DataSet objDs, DataTable objDt)
    {
        SqlConnection connection = GetConnection();
        if (connection == null) throw new ArgumentNullException("connection");

        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, connection, (SqlTransaction)null, CommandType.StoredProcedure, spName, commandParameters, out mustCloseConnection);

        // Create the DataAdapter & DataSet
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            // Fill the DataSet using default values for DataTable names, etc
            if (objDs != null)
                da.Fill(objDs);
            else
                da.Fill(objDt);
            retVal = GetOutPutValue(cmd, "@response");
            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();

            if (mustCloseConnection)
                connection.Close();
        }
    }
    private void TransExecuteQuery(SqlTransaction transaction, string spName, ref string errorMsg, SqlParameter[] commandParameters, DataSet ds)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, transaction.Connection, transaction, CommandType.StoredProcedure, spName, commandParameters, out mustCloseConnection);

        // Create the DataAdapter & DataSet
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            // Fill the DataSet using default values for DataTable names, etc
            da.Fill(ds);
            errorMsg = GetOutPutValue(cmd, "@response");
        }
    }
    public void SaveData(string spName, ref string response, SqlParameter[] commandParameters, ref DataSet ds)
    {
        response = "";
        string errorMsg = "";
        SqlTransaction transaction;
        SqlConnection connection = GetConnection();
        connection.Open();
        transaction = connection.BeginTransaction();
        try
        {
            if (ds == null)
                TransExecuteNonQuery(transaction, spName, ref response, commandParameters);
            else
            {
                ds = new DataSet();
                TransExecuteQuery(transaction, spName, ref response, commandParameters, ds);
            }
            errorMsg = JsonConvert.DeserializeObject<WFXResultModel>(response.ToString()).ErrorMsg;
            //errorMsg = JsonConvert.DeserializeObject<DataTable>(response).Rows["ErrorMsg"].ToString();
            if (!string.IsNullOrEmpty(errorMsg))
            {
                transaction.Rollback();
            }
            else
                transaction.Commit();
        }
        catch (Exception ex)
        {
            if (string.IsNullOrEmpty(response))
                response = GetCommandParameterValue(commandParameters, "@response");
            if (!string.IsNullOrEmpty(response))
            {
                //merrorMsg = JsonConvert.DeserializeObject<DataTable>(merrorMsg).Rows[0]["ErrorMsg"].ToString();
                errorMsg = JsonConvert.DeserializeObject<WFXResultModel>(response.ToString()).ErrorMsg;
            }
            else
            {
                errorMsg = ex.Message.ToString();
                var res = new WFXResultModel();
                res.ErrorMsg = errorMsg;
                res.Status = "Fail";
                response = JsonConvert.SerializeObject(res);
            }
            transaction.Rollback();
            connection.Close();
        }
        finally
        {
            connection.Close();
        }
    }

    //public void LogError(string spName, SqlParameter[] commandParameters)
    //{
    //    string parametersList = "EXEC " + spName + " ";
    //    string query = "";
    //    string errMsg = "";
    //    DataTable dt = new DataTable();
    //    if (commandParameters != null)
    //    {
    //        foreach (SqlParameter p in commandParameters)
    //        {
    //            if (p != null)
    //            {
    //                parametersList += p.ParameterName.ToString() + "=''";
    //                if (p.Value != null)
    //                    parametersList += p.Value.ToString().Replace("'", "''") + "'',";
    //                else
    //                    parametersList += "''";
    //            }
    //        }
    //    }
    //    query = "EXEC xspLogError @spName='" + spName + "', @paramList='" + parametersList + "'";

    //    dt = ExecuteQuery(query, ref errMsg);

    //}
    private string GetCommandParameterValue(SqlParameter[] commandParameters, string parameterName)
    {
        string merrorMsg = string.Empty;
        try
        {
            foreach (SqlParameter p in commandParameters)
            {
                if (p != null)
                {
                    if ((p.Direction == ParameterDirection.Output || p.ParameterName == parameterName))
                    {
                        if (p.Value != DBNull.Value)
                            merrorMsg = p.Value.ToString();
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            merrorMsg = ex.Message.ToString();
        }
        return merrorMsg;
    }
    public SqlParameter AddSqlParameter(string parameterName, ParameterDirection paramDirection, SqlDbType sqldbType, string parameterValue)
    {
        SqlParameter sqlParam = new SqlParameter();
        sqlParam.Direction = paramDirection;
        sqlParam.ParameterName = parameterName;
        sqlParam.SqlDbType = sqldbType;
        sqlParam.Value = parameterValue;
        if ((paramDirection == ParameterDirection.Output || paramDirection == ParameterDirection.InputOutput) && (sqldbType == SqlDbType.VarChar || sqldbType == SqlDbType.NVarChar))
            sqlParam.Size = 4000;
        return sqlParam;
    }
    public object ExecuteScalar(string spName, ref string retVal, SqlParameter[] commandParameters)
    {
        SqlConnection connection = GetConnection();
        if (connection == null) throw new ArgumentNullException("connection");

        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, connection, (SqlTransaction)null, CommandType.StoredProcedure, spName, commandParameters, out mustCloseConnection);

        // Execute the command & return the results
        object retObj = cmd.ExecuteScalar();

        retVal = GetOutPutValue(cmd, "@response");
        // Detach the SqlParameters from the command object, so they can be used again
        cmd.Parameters.Clear();

        if (mustCloseConnection)
            connection.Close();

        return retObj;
    }
    public string ExecuteXMLReader(string spName, ref string retVal, SqlParameter[] commandParameters)
    {
        SqlConnection connection = GetConnection();
        if (connection == null) throw new ArgumentNullException("connection");

        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        StringBuilder SbReturn = new StringBuilder();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, connection, (SqlTransaction)null, CommandType.StoredProcedure, spName, commandParameters, out mustCloseConnection);

        // Execute the command & return the results
        XmlReader xmlr = cmd.ExecuteXmlReader();
        xmlr.Read();
        while (xmlr.ReadState != System.Xml.ReadState.EndOfFile)
        {
            SbReturn.Append(xmlr.ReadOuterXml());
        }

        retVal = GetOutPutValue(cmd, "@response");
        // Detach the SqlParameters from the command object, so they can be used again
        cmd.Parameters.Clear();

        if (mustCloseConnection)
            connection.Close();

        return SbReturn.ToString();
    }
    public DataSet ExecuteDataset(string spName, ref string retVal, SqlParameter[] commandParameters)
    {
        DataSet ds = new DataSet();
        this.NonTransExecuteQuery(spName, ref retVal, commandParameters, ds, null);
        return ds;

    }
    public DataTable ExecuteDataTable(string spName, ref string retVal, SqlParameter[] commandParameters)
    {
        DataTable dt = new DataTable();
        this.NonTransExecuteQuery(spName, ref retVal, commandParameters, null, dt);
        return dt;
    }
    public DataTable ExecuteQuery(string Query, ref string retVal)
    {
        SqlConnection connection = GetConnection();
        if (connection == null) throw new ArgumentNullException("connection");

        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, connection, (SqlTransaction)null, CommandType.Text, Query, (SqlParameter[])null, out mustCloseConnection);

        // Create the DataAdapter & DataSet
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            DataTable dt = new DataTable();

            // Fill the DataSet using default values for DataTable names, etc
            da.Fill(dt);
            retVal = GetOutPutValue(cmd, "@response");
            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();

            if (mustCloseConnection)
                connection.Close();

            // Return the dataset
            return dt;
        }
    }
    public DataSet ExecuteDatasetForQuery(string Query, ref string retVal)
    {
        SqlConnection connection = GetConnection();
        if (connection == null) throw new ArgumentNullException("connection");

        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, connection, (SqlTransaction)null, CommandType.Text, Query, (SqlParameter[])null, out mustCloseConnection);

        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            DataSet ds = new DataSet();

            // Fill the DataSet using default values for DataTable names, etc
            da.Fill(ds);
            retVal = GetOutPutValue(cmd, "@response");
            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();

            if (mustCloseConnection)
                connection.Close();

            // Return the dataset
            return ds;
        }

    }
    public DataRow ExecuteDataRow(string spName, ref string retVal, SqlParameter[] commandParameters)
    {
        DataTable dt = new DataTable();
        this.NonTransExecuteQuery(spName, ref retVal, commandParameters, null, dt);
        return dt.Rows[0];
    }
    public SqlDataReader ExecuteReader(string spName, ref string retVal, SqlParameter[] commandParameters)
    {
        SqlDataReader sqlReader;
        SqlConnection connection = GetConnection();
        if (connection == null) throw new ArgumentNullException("connection");

        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, connection, (SqlTransaction)null, CommandType.StoredProcedure, spName, commandParameters, out mustCloseConnection);

        sqlReader = cmd.ExecuteReader();
        return sqlReader;
    }
    public SqlDataReader ExecuteReaderForQuery(string Query, ref string retVal)
    {
        SqlConnection connection = GetConnection();
        if (connection == null) throw new ArgumentNullException("connection");

        SqlDataReader sqlReader;
        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, connection, (SqlTransaction)null, CommandType.Text, Query, (SqlParameter[])null, out mustCloseConnection);
        sqlReader = cmd.ExecuteReader();
        return sqlReader;
    }

    public string LogActivity(string spName, SqlParameter[] commandParameters, string ErrorMsg, string MethodType, int DBActivityLogID, int RecordCount)
    {
        var strEnableActivityModeInconfig = System.Configuration.ConfigurationManager.AppSettings["EnableActivityMode"];
        if (string.IsNullOrEmpty(strEnableActivityModeInconfig))
        {
            strEnableActivityModeInconfig = "0";
        }


        if (ErrorMsg == "" && strEnableActivityModeInconfig == "0")
        {
            return "";
        }
        string parametersList = "EXEC " + spName + " ", strGUID = "";
        DataTable dt = new DataTable();
        if (commandParameters != null)
        {
            foreach (SqlParameter p in commandParameters)
            {
                if (p != null)
                {
                    parametersList += p.ParameterName.ToString() + "='";
                    if (p.ParameterName.ToString() == "@errorMsg" && !string.IsNullOrEmpty(ErrorMsg))
                    {
                        p.Value = ErrorMsg;
                    }
                    if (p.Value != null)
                        parametersList += p.Value.ToString().Replace("'", "''") + "',";
                    else
                        parametersList += "',";

                    if (p.ParameterName.ToString().ToLower() == "@guid" && p.Value != null)
                    {
                        strGUID = p.Value.ToString();
                    }

                }
            }
        }
        ErrorMsg = ErrorMsg.Replace("'", "''");
        parametersList = parametersList.Substring(0, parametersList.Length - 1);
        SqlParameter[] SqlParamters = new SqlParameter[7];
        string retVal = "";

        SqlParamters[0] = AddSqlParameter("@spName", ParameterDirection.Input, SqlDbType.NVarChar, spName);
        SqlParamters[1] = AddSqlParameter("@paramList", ParameterDirection.Input, SqlDbType.NVarChar, parametersList);
        SqlParamters[2] = AddSqlParameter("@MethodType", ParameterDirection.Input, SqlDbType.NVarChar, MethodType);
        SqlParamters[3] = AddSqlParameter("@ErrorMsg", ParameterDirection.Input, SqlDbType.NVarChar, ErrorMsg);
        SqlParamters[4] = AddSqlParameter("@GUID", ParameterDirection.Input, SqlDbType.NVarChar, strGUID);
        SqlParamters[5] = AddSqlParameter("@DBActivityLogID", ParameterDirection.Input, SqlDbType.Int, DBActivityLogID.ToString());
        SqlParamters[6] = AddSqlParameter("@RecordCount", ParameterDirection.Input, SqlDbType.NVarChar, RecordCount.ToString());

        DataRow row = ExecuteDataRow("xspDBActivityLogSaveData", ref retVal, SqlParamters);
        DBActivityLogID = Convert.ToInt32(row[0]);
        return "";
    }

    public class WFXResultModel
    {
        public int ResponseID { get; set; }
        public dynamic ResponseData { get; set; }
        public string ErrorMsg { get; set; }
        public string Status { get; set; }
    }
}

