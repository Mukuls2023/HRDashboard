using System.Collections.Generic;
using System;
using System.Linq;
using System.Web;


namespace gNxt_HR_Dashboard.Models
{
    public class CelebrationModels
    {
        public string EmpCode { get; set; } = "";
        public string EmpName { get; set; } = "";
        public string CelebrationType { get; set; } = "";
        public string MessageText { get; set; } = "";
        public string Image { get; set; }
    }

    public class slideListData
    {
        public List<CelebrationModels> lstCelebration { get; set; } 
    }

}
