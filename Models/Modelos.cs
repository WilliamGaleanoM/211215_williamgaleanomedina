using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace _211215_williamgaleanomedina.Models
{

    public class IMEIDataServices
    {
        [Display(Name = "Imei")]
        public string IMEI { get; set; }

        [Display(Name = "Vehicle ID")]
        public string VehicleID { get; set; }

        [Display(Name = "Company ID")]
        public int CompanyID { get; set; }
    }

    public class IMEIRequest
    {
        public string IMEI { get; set; }
        public int CompanyID { get; set; }
    }


}