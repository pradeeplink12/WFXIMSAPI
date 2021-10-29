
using System;
using WFXIMSAPI.Classes;
using WFXIMSAPI.Models;
using WFXIMSAPI.WFXCommonFunctions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Cors;
using System.Data;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using System.Drawing.Imaging;

namespace WFXIMSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    public class WFXCommonController : ControllerBase
    {
        private WFXCommonClass objCls = new WFXCommonClass();
        private WFXCommonFunction commonf = new WFXCommonFunction();

        [HttpGet]
        [Route("WFXAPITest")]
        [EnableCors("AllowAllOrigins")]
        public IActionResult WFXAPITest()
        {
            return new OkObjectResult("API is working!");
        }


        [HttpGet]
        [Route("GetSessionVariable")]
        [EnableCors("AllowAllOrigins")]
        public IActionResult GetSessionVariable()
        {
            string pageParams = "", guid = "", getAsJson = "";
            var headers = Request.Headers;
            foreach (StringValues keys in headers.Keys)
            {
                if ((keys == "pageParams") || (keys == "pageparams"))
                {
                    pageParams = headers[keys];
                }
               
            }

            guid = commonf.GetParamValueFromPageParam(pageParams, "GUID");
            getAsJson = commonf.GetParamValueFromPageParam(pageParams, "getAsJson");
            WFXResultModel res = new WFXResultModel();
            try
            {
                res = objCls.GetSessionVariable(guid, getAsJson);
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




        [HttpGet]
        [Route("GetData")]
        [EnableCors("AllowAllOrigins")]
        public ActionResult GetData()
            {
            var headers = Request.Headers;
            WFXResultModel result;
            try
            {
                result = commonf.GetDataResult( headers, null);
             
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                result = new WFXResultModel();
                result.ErrorMsg = ex.Message.ToString();
                result.Status = "Fail";
                return new OkObjectResult(result);
            }
            finally { }
        }


        [HttpDelete]
        [Route("DeleteData")]
        [EnableCors("AllowAllOrigins")]
        public IActionResult Delete()
        {
            return SaveData(null);
        }

        [HttpPut]
        [Route("PutData")]
        [EnableCors("AllowAllOrigins")]
        public IActionResult Put(dynamic dataObj)
        {
            return SaveData(dataObj);

        }

        [HttpPost]
        [Route("SaveData")]
        [EnableCors("AllowAllOrigins")]
        public IActionResult SaveData(dynamic dataObj)
        {

            var re = Request;
            var headers = re.Headers;
            WFXResultModel result = new WFXResultModel();
            try
            {
                result = commonf.SaveDataResult(headers, dataObj, null);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                result = new WFXResultModel();
                result.ErrorMsg = ex.Message.ToString();
                result.Status = "Fail";
                return new OkObjectResult(result);
            }
            finally { }
        }

        [HttpPost]
        [Route("ValidateDataForImportExcel")]
        [EnableCors("AllowAllOrigins")]
        public ActionResult ValidateDataforImportExcel(dynamic dataObj)
        {
            var headers = Request.Headers;
            WFXResultModel result;
            try
            {
                result = commonf.GetValidateDataResult(headers, dataObj, null);

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                result = new WFXResultModel();
                result.ErrorMsg = ex.Message.ToString();
                result.Status = "Fail";
                return new OkObjectResult(result);
            }
            finally { }
        }

        [HttpGet]
        [Route("GetDataFromFunction")]
        [EnableCors("AllowAllOrigins")]
        public IActionResult GetDataFromFunction()
        {
            string pageParams = "", ObjectName = "";
            var re = Request;
            var headers = re.Headers;
            foreach (StringValues keys in headers.Keys)
            {
                if ((keys == "pageParams") || (keys == "pageparams"))
                {
                    pageParams = headers[keys];
                }
                if (keys == "ObjectName")
                {
                    ObjectName = headers[keys];
                }

            }
           
            WFXResultModel result = new WFXResultModel();
            try
            {
                result = objCls.GetDataFromFunction(pageParams, ObjectName);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                result = new WFXResultModel();
                result.ErrorMsg = ex.Message.ToString();
                result.Status = "Fail";
                return new OkObjectResult(result);
            }
            finally { }
        }



        [HttpPost]
        [Route("FileUpload")]
        [EnableCors("AllowAllOrigins")]
        public IActionResult FileUpload()
        {
            


            WFXResultModel result = new WFXResultModel();
            var httpRequest = HttpContext.Request;
            try 
            {
                String errorMsg = "", PhysicalPath, FileNameWithExtension, FileNameWithoutExtension, FileExtension, PartialPath, RelativeUrl, userFileName;
                Boolean ValidFileExtension;

                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                dt.Columns.Add("userFileName");
                dt.Columns.Add("physicalFileName");
                dt.Columns.Add("url");
                dt.Columns.Add("size");
                dt.Columns.Add("error");

                WFXCommonClass wcl = new WFXCommonClass();
                string FileUploadServerPath= wcl.FileUploadServerPath();

                String ServerPath = FileUploadServerPath;
                String FileUploadPath = httpRequest.Form["FileUploadPath"];
                String AllowedFileExt = httpRequest.Form["AllowedFileExt"];
                Int64 FileSize, MaxFileSize = Convert.ToInt64(httpRequest.Form["MaxFileSize"]);

                String CompanyCode = GetCompanyCode();
                String ImportLocation = wcl.GetImportLocation();
                //String CompanyCode = "87400"; // mobjCommon.GetSessionVariable(mGUID, "CompanyCode");
               // String ImportLocation = "0";// ConfigurationManager.AppSettings["ImportLocation"];


                
                Array arrAllowedFileExt = AllowedFileExt.Split('|');
                if (FileUploadPath != null && FileUploadPath.Length > 0)
                {
                    PhysicalPath = Directory.GetParent(ServerPath).Parent.FullName + FileUploadPath + @"\";
                    RelativeUrl = FileUploadPath + @"\";
                }
                else
                {
                    if (httpRequest.Form["ItemSubType"] == "Image")
                    {
                        PartialPath = "Company\\" + CompanyCode + "\\Pictures\\";
                        RelativeUrl = @"\Company\" + CompanyCode + @"\Pictures\";
                    }
                    else
                    {
                        if (ImportLocation == "1")
                        {
                            string Path = ServerPath + "Company\\" + CompanyCode + "\\Imports";
                            if (!Directory.Exists(Path))
                            {
                                DirectoryInfo di = Directory.CreateDirectory(Path);
                            }
                            PartialPath = "Company\\" + CompanyCode + "\\Imports\\";
                            RelativeUrl = @"\Company\" + CompanyCode + @"\Imports\";
                        }
                        else
                        {
                            PartialPath = "Company\\" + CompanyCode + "\\Documents\\";
                            RelativeUrl = @"\Company\" + CompanyCode + @"\Documents\";
                        }
                    }
                    PhysicalPath = ServerPath + PartialPath;
                }

                System.IO.Directory.CreateDirectory(PhysicalPath);
                Int32 PhysicalPathLen = PhysicalPath.Length;
                var Files = HttpContext.Request.Form.Files;


                foreach (IFormFile formFile in Files)
                {
                    DataRow dr = dt.NewRow();
                    FileSize = formFile.Length;
                    userFileName = Path.GetFileName(formFile.FileName);
                    userFileName = userFileName.Replace("'", "").Replace("~", "").Replace("%20", " ").Replace("%", "").Replace("#", "").Replace("+", "").Replace("|", "").Replace("&nbsp;", "").Replace(",", "").Replace("@", "").Replace("&", "");

                    if (FileSize <= MaxFileSize)
                    {
                        FileNameWithExtension = Path.GetFileName(formFile.FileName);

                        if (FileNameWithExtension == "blob") //Image Pasted by PrintScreen
                        {
                            FileNameWithoutExtension = "blob";
                            FileExtension = ".jpg";
                            ValidFileExtension = true;
                        }
                        else
                        {
                            FileNameWithoutExtension = FileNameWithExtension.Substring(0, FileNameWithExtension.LastIndexOf('.'));
                            FileNameWithoutExtension = FileNameWithoutExtension.Replace("'", "").Replace("~", "").Replace("%20", " ").Replace("%", "").Replace("#", "").Replace("+", "").Replace("|", "").Replace("&nbsp;", "").Replace(",", "").Replace("@", "").Replace("&", "");
                            string pattern = "\\s+";
                            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex(pattern);
                            FileNameWithoutExtension = rgx.Replace(FileNameWithoutExtension, " ");

                            FileExtension = FileNameWithExtension.Substring(FileNameWithExtension.LastIndexOf('.')).ToLower();
                            ValidFileExtension = false;
                            foreach (string ext in arrAllowedFileExt)
                            {
                                if (FileExtension == ext)
                                {
                                    ValidFileExtension = true;
                                    break;
                                }
                            }
                        }
                        if (ValidFileExtension)
                        {
                            #region //Code to make the Physical FilePath's Length not more than 256 characters. Ex: D:\APPLICATION\wfx_Home\Company\22000\Documents\Filename.ext
                            if (FileExtension.ToLower() != ".rdl")
                                FileNameWithoutExtension = DateTime.Now.Ticks.ToString() + "_" + FileNameWithoutExtension;
                            Int32 FileLength = PhysicalPathLen + FileNameWithoutExtension.Length;
                            Int32 AllowedMaxFileLength = 256 - FileExtension.Length;
                            if (FileLength > AllowedMaxFileLength)
                                FileNameWithoutExtension = FileNameWithoutExtension.Substring(0, FileNameWithoutExtension.Length - (FileLength - AllowedMaxFileLength));
                            FileNameWithExtension = FileNameWithoutExtension + FileExtension;

                            #endregion //Code to make the Physical FilePath's Length not more than 256 characters. Ex: D:\APPLICATION\wfx_Home\Company\22000\Documents\Filename.ext

                            if (httpRequest.Form["ItemSubType"] == "Image")
                            {
                                FileNameWithExtension = ProcessImage(httpRequest.Form["ImageDimension"], formFile, PhysicalPath, FileNameWithoutExtension);
                            }
                            else
                            {
                                if (formFile.Length > 0)
                                {
                                    var filePath = PhysicalPath;
                                    if (System.IO.Directory.Exists(filePath))
                                    {
                                        using (var stream = System.IO.File.Create(filePath+FileNameWithExtension))
                                        {
                                            formFile.CopyToAsync(stream);
                                        }
                                    }
                                    
                                }
                            }
                            
                            dr["userFileName"] = userFileName;
                            dr["physicalFileName"] = FileNameWithExtension;
                            dr["url"] = RelativeUrl + FileNameWithExtension;
                            dr["size"] = FileSize;
                            dr["error"] = "";
                            dt.Rows.Add(dr);
                        }
                        else
                        {
                            dr["userFileName"] = userFileName;
                            dr["size"] = FileSize;
                            dr["error"] = FileExtension + " File Type Not Allowed.";
                            dt.Rows.Add(dr);
                        }
                    }
                    else
                    {
                        dr["userFileName"] = userFileName;
                        dr["size"] = FileSize;
                        dr["error"] = "Max File Size Limit is " + MaxFileSize / 1048576 + " MB";
                        dt.Rows.Add(dr);
                    }
                   
                   
                }

                ds.Tables.Add(dt);
                result.ResponseData = commonf.CreateJson(ds, ref errorMsg)[0].ToString();
                return  new OkObjectResult( result);

            }
            catch (Exception ex)
            {
                result = new WFXResultModel();
                result.ErrorMsg = ex.Message.ToString();
                result.Status = "Fail";
                return new OkObjectResult(result);
            }
            finally { }
        }


        //process Image
        private string ProcessImage(String ImageDimensions, IFormFile Image, String PhysicalPath, String FileNameWithoutExtension)
        {
            String DefaultFileNameWithExtension = "", FileNameWithExtension = "";
            try
            {
                Int32 PhysicalPathLen = PhysicalPath.Length;
                WFXImageOptimization ImageOptimization = new WFXImageOptimization();
                String[] arrImageDimensions = ImageDimensions.Split(',');
                int canvasWidth = 0, canvasHeight = 0;
                int length = (int)Image.Length;
                byte[] imageBits = new byte[length];

                // read the digital bits of the image into the byte array
               // Image.InputStream.Read(imageBits, 0, length);

                var strUploadOriginalImages = System.Configuration.ConfigurationManager.AppSettings["UploadOriginalImages"];
                if (string.IsNullOrEmpty(strUploadOriginalImages))
                    strUploadOriginalImages = "0";

                // save the byte array as a Bitmap object
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(imageBits, 0, imageBits.Length);
                    using (Bitmap unrenderedImage = new Bitmap(ms))
                    {
                        RotateFlipType rft = RotateFlipType.RotateNoneFlipNone;
                        PropertyItem[] properties = unrenderedImage.PropertyItems;

                        foreach (var prop in properties)
                        {
                            if (prop.Id == 274)
                            {
                                var orientation = BitConverter.ToInt16(prop.Value, 0);
                                rft = orientation == 1 ? RotateFlipType.RotateNoneFlipNone :
                                        orientation == 3 ? RotateFlipType.Rotate180FlipNone :
                                        orientation == 6 ? RotateFlipType.Rotate90FlipNone :
                                        orientation == 8 ? RotateFlipType.Rotate270FlipNone :
                                        RotateFlipType.RotateNoneFlipNone;
                            }
                        }
                        if (rft != RotateFlipType.RotateNoneFlipNone)
                        {
                            unrenderedImage.RotateFlip(rft);
                        }
                        foreach (var ImageDimension in arrImageDimensions)
                        {
                            canvasWidth = Convert.ToInt32(ImageDimension.ToUpper().Split('X')[0]);
                            canvasHeight = Convert.ToInt32(ImageDimension.ToUpper().Split('X')[1]);

                            #region //Code to make the Physical FilePath's Length not more than 256 characters. Ex: D:\APPLICATION\wfx_Home\Company\22000\Pictures\Filename.jpg
                            Int32 FileLength = PhysicalPathLen + FileNameWithoutExtension.Length;
                            String PartialFileName = "_" + canvasWidth + "X" + canvasHeight + ".jpg";
                            Int32 AllowedMaxFileLength = 256 - PartialFileName.Length;
                            if (FileLength > AllowedMaxFileLength)
                                FileNameWithoutExtension = FileNameWithoutExtension.Substring(0, FileNameWithoutExtension.Length - (FileLength - AllowedMaxFileLength));
                            FileNameWithExtension = FileNameWithoutExtension + PartialFileName;
                            #endregion //Code to make the Physical FilePath's Length not more than 256 characters. Ex: D:\APPLICATION\wfx_Home\Company\22000\Pictures\Filename.jpg

                            if (DefaultFileNameWithExtension == "")
                            {
                                DefaultFileNameWithExtension = FileNameWithExtension;
                            }
                            ImageOptimization.ProcessImage(unrenderedImage, canvasWidth, canvasHeight, 75, PhysicalPath + FileNameWithExtension);
                        }

                        if (strUploadOriginalImages == "1")
                            ImageOptimization.ProcessImage(unrenderedImage, unrenderedImage.Width, unrenderedImage.Height, 75, PhysicalPath + FileNameWithoutExtension + ".jpg");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message.ToString());
            }

            return DefaultFileNameWithExtension;
        }

        //GetCompanyCode
        private String GetCompanyCode()
        {
            string pageParams = "", CompanyCode = "";
            var headers = Request.Headers;
            foreach (StringValues keys in headers.Keys)
            {
                if ((keys == "pageParams") || (keys == "pageparams"))
                {
                    pageParams = headers[keys];
                }
            }

            CompanyCode = commonf.GetParamValueFromPageParam(pageParams, "CompanyCode");
            return CompanyCode;
        }
    }
}
