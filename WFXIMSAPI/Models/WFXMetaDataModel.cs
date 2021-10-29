using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WFXIMSAPI.Models
{
    public class WFXMetaDataModel
    {
        public string Code { get; set; }
        public string Text { get; set; }
        public string icon { get; set; }
        public int? ID { get; set; }
        public int? SortOrder { get; set; }
        public bool Active { get; set; }
    }
    public class WFXMetaDataResultModel
    {
        public int ResponseID { get; set; }
        public List<WFXMetaDataModel> ResponseData { get; set; }
        public string ErrorMsg { get; set; }
        public string Status { get; set; }
    }
}
