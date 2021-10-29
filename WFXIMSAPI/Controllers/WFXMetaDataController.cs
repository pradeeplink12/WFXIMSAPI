using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Linq;
//using System.Web.Http;
using WFXIMSAPI.Classes;
using WFXIMSAPI.Models;

namespace WFXIMSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    public class WFXMetaDataController : ControllerBase
    {
        ArrayList oArrayList = new ArrayList();
        WFXMetaData objMetaData = new WFXMetaData();


        [HttpGet]
        //[Route("api/WFXMetaData/GetDDLData")]
        [Route("GetDDLData")]
        [EnableCors("AllowAllOrigins")]
        public IActionResult GetDDLData()
        {
            WFXMetaDataResultModel res = new WFXMetaDataResultModel();
            string pageParams = "", searchParams = "", sortParams = "", pagingParams = "";
            var headers = Request.Headers;
            foreach (StringValues keys in headers.Keys)
            {
                if ((keys == "pageParams") || (keys == "pageparams"))
                {
                    pageParams = headers[keys];
                }
                if ((keys == "searchParams") || (keys == "searchparams"))
                {
                    searchParams = headers[keys];
                }
                if ((keys == "sortParams") || (keys == "sortparams"))
                {
                    sortParams = headers[keys];
                }
                if ((keys == "pagingParams") || (keys == "pagingparams"))
                {
                    pagingParams = headers[keys];
                }

            }
          
            res = objMetaData.GetMiscData(pageParams, searchParams, sortParams, pagingParams);
            try
            {
                return new OkObjectResult(res);
            }
            catch (Exception ex)
            {
                res = new WFXMetaDataResultModel();
                res.ErrorMsg = ex.Message.ToString();
                res.Status = "Fail";
                return new OkObjectResult(res);
            }
            finally { }
        }

        [HttpGet]
        // [Route("api/WFXMetaData/GetDDLDataAsDynamicData")]
        [Route("GetDDLDataAsDynamicData")]
        [EnableCors("AllowAllOrigins")]
        public IActionResult GetDDLDataAsDynamicData()
        {
            WFXResultModel res = new WFXResultModel();
            string pageParams = "", searchParams = "", sortParams = "", pagingParams = "";
            var headers = Request.Headers;
            foreach (StringValues keys in headers.Keys)
            {
                if ((keys == "pageParams") || (keys == "pageparams"))
                {
                    pageParams = headers[keys];
                }
                if ((keys == "searchParams") || (keys == "searchparams"))
                {
                    searchParams = headers[keys];
                }
                if ((keys == "sortParams") || (keys == "sortparams"))
                {
                    sortParams = headers[keys];
                }
                if ((keys == "pagingParams") || (keys == "pagingparams"))
                {
                    pagingParams = headers[keys];
                }

            }
            res = objMetaData.GetDDLDataAsDynamicData(pageParams, searchParams, sortParams, pagingParams);
            try
            {
                return new OkObjectResult(res);
            }
            catch (Exception ex)
            {
                res = new WFXResultModel();
                res.ErrorMsg = ex.Message.ToString();
                res.Status = "Fail";
                return new OkObjectResult(res);
            }
            finally { }
        }
    }
}
