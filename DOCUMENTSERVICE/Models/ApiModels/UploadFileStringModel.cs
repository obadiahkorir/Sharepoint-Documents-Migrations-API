using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOCUMENTSERVICE.Models.ApiModels
{
    public class UploadFileStringModel
    {
        public string base64String { get; set; }
        public string claimNo { get; set; }
        public string dept { get; set; }
        public string docName { get; set; }
        public string docSource { get; set; }
        public string docType { get; set; }
        public string fileExtension { get; set; }
        public string idNo { get; set; }
        public string insuredName { get; set; }
        public string policyNo { get; set; }
    }
}