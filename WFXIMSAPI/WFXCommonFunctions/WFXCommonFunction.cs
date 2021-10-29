//using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nancy.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using WFXIMSAPI.Classes;
using WFXIMSAPI.Models;
using System.Net;

namespace WFXIMSAPI.WFXCommonFunctions
{
    public class WFXCommonFunction
    {
        private WFXCommonClass objCls = new WFXCommonClass();
        public string GetParamValueFromPageParam(string pageParams, string ParamName)
        {
            DataTable dtPageParam = new DataTable();
            dtPageParam = JsonConvert.DeserializeObject<DataTable>(pageParams);
            string ParamValue = string.Empty;
            for (int i = 0; i < dtPageParam.Rows.Count; i++)
            {
                if (dtPageParam.Rows[i]["ParamName"].ToString().ToLower() == ParamName.ToLower())
                {
                    ParamValue = dtPageParam.Rows[i]["ParamValue"].ToString();
                    break;
                }
            }
            return ParamValue;
        }

        public string GetObjectNameFromAPIParams(string apiParams)
        {
            return GetParamValueFromAPIParam(apiParams, "ObjectName");
        }

        public string GetParamValueFromAPIParam(string apiParams, string ParamName)
        {
            DataTable dtPageParam = new DataTable();
            dtPageParam = JsonConvert.DeserializeObject<DataTable>(apiParams);
            string ParamValue = string.Empty;
            for (int i = 0; i < dtPageParam.Rows.Count; i++)
            {
                if (dtPageParam.Rows[i]["ParamName"].ToString().ToLower() == ParamName.ToLower())
                {
                    ParamValue = dtPageParam.Rows[i]["ParamValue"].ToString();
                    break;
                }
            }
            return ParamValue;
        }

        public WFXResultModel returnNoSPError()
        {
            WFXResultModel result = new WFXResultModel();
            result.ResponseID = 0;
            result.ErrorMsg = "No Object Name found!";
            result.Status = "Fail";
            result.ResponseData = "";

            return result;
        }

        public WFXResultModel GetDataResult(IHeaderDictionary headers, string ObjectName)
        {
            string pageParams = "", apiParams = "", searchParams = "", sortParams = "", pagingParams = "", methodName = "";
            foreach (StringValues keys in headers.Keys)
            {
                if ((keys == "pageParams")|| (keys == "pageparams"))
                {
                    pageParams = headers[keys];
                }
                if ((keys == "apiParams") || (keys == "apiparams"))
                {
                    apiParams = headers[keys];
                }
                if ((keys == "searchParams") || (keys == "searchparams"))
                {
                    searchParams = headers[keys];
                    searchParams = WebUtility.UrlDecode(searchParams);
                }
                if ((keys == "sortParams")|| (keys == "sortparams"))
                {
                    sortParams = headers[keys];
                }
                if ((keys == "pagingParams")|| (keys == "pagingparams"))
                {
                    pagingParams = headers[keys];
                }
            }
          
            if (pageParams != "")
            {
                methodName = GetParamValueFromPageParam(pageParams, "methodName");
                if (ObjectName == "" || ObjectName == null)
                {
                    ObjectName = GetObjectNameFromAPIParams(apiParams);
                }
            }
            if (string.IsNullOrEmpty(methodName))
                methodName = "GetList";
            WFXResultModel result = new WFXResultModel();
            try
            {
                if (ObjectName != "")
                {
                    if (methodName.ToLower() == "getdetail")
                    {
                        result = objCls.GetDetailData(pageParams, ObjectName);
                    }
                    else
                    {
                        result = objCls.GetListData(pageParams, searchParams, sortParams, pagingParams, ObjectName);
                    }
                }
                else
                {
                    result = returnNoSPError();
                }
                return result;
            }
            catch (Exception ex)
            {
                result = new WFXResultModel();
                result.ErrorMsg = ex.Message.ToString();
                result.Status = "Fail";
                return result;
            }
            finally { }
        }
        public WFXResultModel SaveDataResult(IHeaderDictionary headers, dynamic dataObj, string ObjectName)
        {
            string pageParams = "", apiParams = "", content = "";
            WFXResultModel result = new WFXResultModel();
            foreach (StringValues keys in headers.Keys)
            {
                if ((keys == "pageParams")|| (keys == "pageparams"))
                {
                    pageParams = headers[keys];
                }
                if ((keys == "apiParams") || (keys == "apiparams")) 
                {
                    apiParams = headers[keys];
                }
               
            }
            //if (headers.Contains("pageParams"))
            //    pageParams = headers.GetValues("pageParams").FirstOrDefault();
            //if (headers.Contains("apiParams"))
            //    apiParams = headers.GetValues("apiParams").FirstOrDefault();
            try
            {
                if (dataObj != null)
                {
                    content = JsonConvert.SerializeObject(dataObj);
                }
                if (ObjectName == "" || ObjectName == null)
                {
                    ObjectName = GetObjectNameFromAPIParams(apiParams);
                }
                if (ObjectName != "")
                {
                    result = objCls.InsertUpdateDeleteData(pageParams, content, ObjectName);
                }
                else
                {
                    result = returnNoSPError();
                }

                return result;
            }
            catch (Exception ex)
            {
                result = new WFXResultModel();
                result.ErrorMsg = ex.ToString();
                result.Status = "Fail";
                return result;
            }
        }


        public WFXResultModel GetValidateDataResult(IHeaderDictionary headers, dynamic dataObj, string ObjectName)
        {
            string pageParams = "", apiParams = "", content = "";
            WFXResultModel result = new WFXResultModel();
            foreach (StringValues keys in headers.Keys)
            {
                if ((keys == "pageParams") || (keys == "pageparams"))
                {
                    pageParams = headers[keys];
                }
                if ((keys == "apiParams") || (keys == "apiparams"))
                {
                    apiParams = headers[keys];
                }
            }
            try
            {
                if (dataObj != null)
                {
                    content = JsonConvert.SerializeObject(dataObj);
                }
                if (ObjectName == "" || ObjectName == null)
                {
                    ObjectName = GetObjectNameFromAPIParams(apiParams);
                }
                if (ObjectName != "")
                {
                    result = objCls.ValidateImportData(pageParams, content, ObjectName);
                }
                else
                {
                    result = returnNoSPError();
                }

                return result;
            }
            catch (Exception ex)
            {
                result = new WFXResultModel();
                result.ErrorMsg = ex.ToString();
                result.Status = "Fail";
                return result;
            }
        }
        public List<string> CreateJson(DataSet ds, ref string errorMsg)
        {
            List<string> lstJson = new List<string>();
            var js = new JavaScriptSerializer();
            //System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            //js.MaxJsonLength = Int32.MaxValue;
            try
            {
                for (int k = 0; k < ds.Tables.Count; k++)
                {
                    DataTable dtData;
                    dtData = ds.Tables[k];
                    int rowCount = dtData.Rows.Count;
                    int colCount = dtData.Columns.Count;
                    if (rowCount > 0 || colCount > 0)
                    {
                        StringBuilder json = new StringBuilder();
                        json.Append("[");
                        if (rowCount > 0)
                        {
                            Boolean NotIsfirstRow = false;
                            for (int j = 0; j < rowCount; j++)
                            {
                                if (NotIsfirstRow)
                                    json.Append(",{");
                                else
                                    json.Append("{");

                                Boolean NotIsfirstColumn = false;
                                for (Int16 i = 0; i < colCount; i++)
                                {
                                    if (NotIsfirstColumn)
                                        json.Append(",");
                                    string RowData = js.Serialize(dtData.Rows[j][i].ToString());//done for backslashes in RowData for example in URL path.
                                    json.Append(string.Format("\"{0}\":{1}", dtData.Columns[i].ToString(), RowData));
                                    NotIsfirstColumn = true;
                                }
                                NotIsfirstRow = true;
                                json.Append("}");
                            }
                        }

                        json.Append("]");
                        lstJson.Add(json.ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                errorMsg = ex.ToString();
            }
            finally
            {
            }
            return lstJson;
        }

    }

    
}
