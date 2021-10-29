using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using WFXIMSAPI.Models;
using WFXIMSAPI.WFXCommonFunctions;

namespace WFXIMSAPI.Classes
{

    public class WFXCommonClass
    {
        SqlConnection sqlConnection;
        IConfigurationRoot configuration ;
        public WFXCommonClass()
        {

             configuration = GetConfiguration();
             sqlConnection = new SqlConnection(configuration.GetSection("ConnectionStrings").GetSection("WFXIMS").Value);
        
           
        }
        IConfigurationRoot GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }

        public string FileUploadServerPath()
        {
            return configuration.GetSection("FileUploadServerPath").GetSection("ServerPath").Value;
        }

        public string GetImportLocation()
        {
            return configuration.GetSection("ImportLocation").GetSection("ImportLocation").Value;
        }

        public WFXResultModel GetSessionVariable(string guid, string getAsJson)
        {
            WFXCommonClass wcl = new WFXCommonClass();
            SqlConnection con = wcl.sqlConnection;
            string constr = con.ConnectionString;
            WFXIMSAPISQLHelper mobjSqlHelper = new WFXIMSAPISQLHelper(constr);
            string response = "";
            WFXResultModel res = new WFXResultModel();
            var jsonResult = new StringBuilder();
            try
            {
                SqlParameter[] SqlParamters = new SqlParameter[2];
                SqlParamters[0] = mobjSqlHelper.AddSqlParameter("@guid", ParameterDirection.Input, SqlDbType.NVarChar, guid);
                SqlParamters[1] = mobjSqlHelper.AddSqlParameter("@response", ParameterDirection.Output, SqlDbType.NVarChar, response);
                SqlDataReader reader = mobjSqlHelper.ExecuteReader("xspGetSessionVariableAsJson", ref response, SqlParamters);
                if (!reader.HasRows)
                {
                    jsonResult.Append("[]");
                }
                else
                {
                    while (reader.Read())
                    {
                        jsonResult.Append(reader.GetValue(0).ToString());
                    }
                    res = JsonConvert.DeserializeObject<WFXResultModel>(jsonResult.ToString());
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                res = new WFXResultModel();
                res.ErrorMsg = ex.Message.ToString();
                res.Status = "Fail";
            }
            return res;
        }

        public WFXResultModel GetDetailData(string pageParams, string spName)
        {
            WFXCommonClass wcl = new WFXCommonClass();
            SqlConnection con = wcl.sqlConnection;
            string constr = con.ConnectionString;
            WFXIMSAPISQLHelper mobjSqlHelper = new WFXIMSAPISQLHelper(constr);
            spName = "xsp" + spName + "GetDetail";
            string response = "";
            WFXResultModel res = new WFXResultModel();
            var jsonResult = new StringBuilder();
            try
            {
                SqlParameter[] SqlParamters = new SqlParameter[2];
                SqlParamters[0] = mobjSqlHelper.AddSqlParameter("@PageParam", ParameterDirection.Input, SqlDbType.NVarChar, pageParams);
                SqlParamters[1] = mobjSqlHelper.AddSqlParameter("@response", ParameterDirection.Output, SqlDbType.NVarChar, response);
                SqlDataReader reader = mobjSqlHelper.ExecuteReader(spName, ref response, SqlParamters);
                if (!reader.HasRows)
                {
                    jsonResult.Append("[]");
                }
                else
                {
                    while (reader.Read())
                    {
                        jsonResult.Append(reader.GetValue(0).ToString());
                    }
                }
                res = JsonConvert.DeserializeObject<WFXResultModel>(jsonResult.ToString());
                reader.Close();
            }
            catch (Exception ex)
            {
                res = new WFXResultModel();
                res.ErrorMsg = ex.Message.ToString();
                res.Status = "Fail";
            }
            return res;
        }

        public WFXResultModel GetListData(string pageParams, string searchParams, string sortParams,
            string pagingParams, string spName)
        {
            WFXCommonClass wcl=new WFXCommonClass();
            SqlConnection con = wcl.sqlConnection;
            string constr = con.ConnectionString;
            WFXIMSAPISQLHelper mobjSqlHelper = new WFXIMSAPISQLHelper(constr);

            spName = "xsp" + spName + "GetList";
            string response = "";
            WFXResultModel res = new WFXResultModel();
            var jsonResult = new StringBuilder();
            try
            {
                SqlParameter[] SqlParamters = new SqlParameter[5];
                SqlParamters[0] = mobjSqlHelper.AddSqlParameter("@PageParam", ParameterDirection.Input, SqlDbType.NVarChar, pageParams);
                SqlParamters[1] = mobjSqlHelper.AddSqlParameter("@SearchParam", ParameterDirection.Input, SqlDbType.NVarChar, searchParams);
                SqlParamters[2] = mobjSqlHelper.AddSqlParameter("@SortParam", ParameterDirection.Input, SqlDbType.NVarChar, sortParams);
                SqlParamters[3] = mobjSqlHelper.AddSqlParameter("@PagingParam", ParameterDirection.Input, SqlDbType.NVarChar, pagingParams);
                SqlParamters[4] = mobjSqlHelper.AddSqlParameter("@response", ParameterDirection.Output, SqlDbType.NVarChar, response);
                SqlDataReader reader = mobjSqlHelper.ExecuteReader(spName, ref response, SqlParamters);
                if (!reader.HasRows)
                {
                    jsonResult.Append("[]");
                }
                else
                {
                    while (reader.Read())
                    {
                        jsonResult.Append(reader.GetValue(0).ToString());
                    }
                }
                res = JsonConvert.DeserializeObject<WFXResultModel>(jsonResult.ToString());
                reader.Close();
            }
            catch (Exception ex)
            {
                res = new WFXResultModel();
                res.ErrorMsg = ex.Message.ToString();
                res.Status = "Fail";
            }
            return res;
        }
        public WFXResultModel InsertUpdateDeleteData(string pageParams, string content, string spName)
        {
            WFXCommonClass wcl = new WFXCommonClass();
            SqlConnection con = wcl.sqlConnection;
            string constr = con.ConnectionString;
            WFXIMSAPISQLHelper mobjSqlHelper = new WFXIMSAPISQLHelper(constr);

            spName = "xsp" + spName + "SaveData";
            WFXResultModel res = new WFXResultModel();
            string response = "";
            try
            {
                SqlParameter[] SqlParamters = new SqlParameter[3];
                SqlParamters[0] = mobjSqlHelper.AddSqlParameter("@pageParam", ParameterDirection.Input, SqlDbType.VarChar, pageParams);
                SqlParamters[1] = mobjSqlHelper.AddSqlParameter("@Data", ParameterDirection.Input, SqlDbType.NVarChar, content);
                SqlParamters[2] = mobjSqlHelper.AddSqlParameter("@response", ParameterDirection.Output, SqlDbType.NVarChar, response);
                DataSet ds = null;
                mobjSqlHelper.SaveData(spName, ref response, SqlParamters, ref ds);
                res = JsonConvert.DeserializeObject<WFXResultModel>(response.ToString());
            }
            catch (Exception ex)
            {
                res = new WFXResultModel();
                res.ErrorMsg = ex.ToString();
                res.Status = "Fail";
            }
            return res;
        }
        public WFXResultModel ValidateImportData(string pageParams, string content, string spName)
        {
            WFXCommonClass wcl = new WFXCommonClass();
            SqlConnection con = wcl.sqlConnection;
            string constr = con.ConnectionString;
            WFXIMSAPISQLHelper mobjSqlHelper = new WFXIMSAPISQLHelper(constr);

            spName = "xsp" + spName + "ValidateData";
            WFXResultModel res = new WFXResultModel();
            string response = "";
            try
            {
                SqlParameter[] SqlParamters = new SqlParameter[3];
                SqlParamters[0] = mobjSqlHelper.AddSqlParameter("@pageParam", ParameterDirection.Input, SqlDbType.VarChar, pageParams);
                SqlParamters[1] = mobjSqlHelper.AddSqlParameter("@Data", ParameterDirection.Input, SqlDbType.NVarChar, content);
                SqlParamters[2] = mobjSqlHelper.AddSqlParameter("@response", ParameterDirection.Output, SqlDbType.NVarChar, response);
                DataSet ds = null;
                mobjSqlHelper.SaveData(spName, ref response, SqlParamters, ref ds);
                res = JsonConvert.DeserializeObject<WFXResultModel>(response.ToString());
            }
            catch (Exception ex)
            {
                res = new WFXResultModel();
                res.ErrorMsg = ex.ToString();
                res.Status = "Fail";
            }
            return res;
        }

        public WFXResultModel GetDataFromFunction(string pageParams, string ObjectName)
        {
            WFXCommonFunction commonf = new WFXCommonFunction();
            string response = "", query = "";
            WFXResultModel res = new WFXResultModel();
            WFXCommonClass wcl = new WFXCommonClass();
            SqlConnection con = wcl.sqlConnection;
            string constr = con.ConnectionString;
            WFXIMSAPISQLHelper mobjSqlHelper = new WFXIMSAPISQLHelper(constr);
            var jsonResult = new StringBuilder();
            try
            {
                if (ObjectName == "")
                {
                    return commonf.returnNoSPError();
                }
                else
                {
                    query = "Select dbo." + ObjectName + "('";
                }

                DataTable dtPageParam = new DataTable();
                dtPageParam = JsonConvert.DeserializeObject<DataTable>(pageParams);
                string ParamValue = string.Empty;
                for (int i = 0; i < dtPageParam.Rows.Count; i++)
                {
                    if (dtPageParam.Rows[i]["ParamValue"].ToString().ToLower() != "ObjectName")
                    {
                        query = query + dtPageParam.Rows[i]["ParamValue"].ToString() + "'";
                        if (i < dtPageParam.Rows.Count - 1)
                        {
                            query = query + ",'";
                        }
                    }
                }
                query = query + ")";
                SqlDataReader reader = mobjSqlHelper.ExecuteReaderForQuery(query, ref response);
                if (!reader.HasRows)
                {
                    jsonResult.Append("[]");
                }
                else
                {
                    while (reader.Read())
                    {
                        jsonResult.Append(reader.GetValue(0).ToString());
                    }
                }
                res.ResponseData = jsonResult.ToString();
                res.Status = "Success";
                reader.Close();
            }
            catch (Exception ex)
            {
                res = new WFXResultModel();
                res.ErrorMsg = ex.Message.ToString();
                res.Status = "Fail";
            }
            return res;
        }
    }
}
