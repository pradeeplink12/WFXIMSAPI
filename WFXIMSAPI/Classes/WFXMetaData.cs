using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFXIMSAPI.Models;

namespace WFXIMSAPI.Classes
{
    public class WFXMetaData
    {
        SqlConnection sqlConnection;
        public WFXMetaData()
        {
            var configuration = GetConfiguration();
            sqlConnection = new SqlConnection(configuration.GetSection("ConnectionStrings").GetSection("WFXIMS").Value);
        }
        IConfigurationRoot GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }


        public WFXMetaDataResultModel GetMiscData(string pageParams, string searchParams, string sortParams, string pagingParams)
        {
            WFXMetaData wcl = new WFXMetaData();
            SqlConnection con = wcl.sqlConnection;
            string constr = con.ConnectionString;
            WFXIMSAPISQLHelper mobjSqlHelper = new WFXIMSAPISQLHelper(constr);
            string response = "";
            WFXMetaDataResultModel res = new WFXMetaDataResultModel();
            try
            {
                SqlParameter[] SqlParamters = new SqlParameter[5];
                SqlParamters[0] = mobjSqlHelper.AddSqlParameter("@PageParam", ParameterDirection.Input, SqlDbType.NVarChar, pageParams);
                SqlParamters[1] = mobjSqlHelper.AddSqlParameter("@SearchParam", ParameterDirection.Input, SqlDbType.NVarChar, searchParams);
                SqlParamters[2] = mobjSqlHelper.AddSqlParameter("@SortParam", ParameterDirection.Input, SqlDbType.NVarChar, sortParams);
                SqlParamters[3] = mobjSqlHelper.AddSqlParameter("@PagingParam", ParameterDirection.Input, SqlDbType.NVarChar, pagingParams);
                SqlParamters[4] = mobjSqlHelper.AddSqlParameter("@response", ParameterDirection.Output, SqlDbType.NVarChar, response);
                SqlDataReader reader = mobjSqlHelper.ExecuteReader("xspMetaDataGetList", ref response, SqlParamters);
                var jsonResult = new StringBuilder();
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
                res = JsonConvert.DeserializeObject<WFXMetaDataResultModel>(jsonResult.ToString());
                reader.Close();
            }
            catch (Exception ex)
            {
                res = new WFXMetaDataResultModel();
                res.ErrorMsg = ex.Message.ToString();
                res.Status = "Fail";
            }
            return res;
        }

        public WFXResultModel GetDDLDataAsDynamicData(string pageParams, string searchParams, string sortParams, string pagingParams)
        {
            WFXMetaData wmd = new WFXMetaData();
            SqlConnection con = wmd.sqlConnection;
            string constr = con.ConnectionString;
            WFXIMSAPISQLHelper mobjSqlHelper = new WFXIMSAPISQLHelper(constr);
            string response = "";
            WFXResultModel res = new WFXResultModel();
            try
            {
                SqlParameter[] SqlParamters = new SqlParameter[5];
                SqlParamters[0] = mobjSqlHelper.AddSqlParameter("@PageParam", ParameterDirection.Input, SqlDbType.NVarChar, pageParams);
                SqlParamters[1] = mobjSqlHelper.AddSqlParameter("@SearchParam", ParameterDirection.Input, SqlDbType.NVarChar, searchParams);
                SqlParamters[2] = mobjSqlHelper.AddSqlParameter("@SortParam", ParameterDirection.Input, SqlDbType.NVarChar, sortParams);
                SqlParamters[3] = mobjSqlHelper.AddSqlParameter("@PagingParam", ParameterDirection.Input, SqlDbType.NVarChar, pagingParams);
                SqlParamters[4] = mobjSqlHelper.AddSqlParameter("@response", ParameterDirection.Output, SqlDbType.NVarChar, response);
                SqlDataReader reader = mobjSqlHelper.ExecuteReader("xspMetaDataGetList", ref response, SqlParamters);
                var jsonResult = new StringBuilder();
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
    }
}
