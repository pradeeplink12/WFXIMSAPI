using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WFXIMSAPI.Models
{
    public class WFXResultModel
    {
        public int ResponseID { get; set; }
        public dynamic ResponseData { get; set; }
        public string ErrorMsg { get; set; }
        public string Status { get; set; }
    }
}
