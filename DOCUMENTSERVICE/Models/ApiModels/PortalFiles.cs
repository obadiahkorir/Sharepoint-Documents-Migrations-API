using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOCUMENTSERVICE.Models.ApiModels
{
    public class PortalFiles
    {
        public DateTime dateReceived { get; set; }
        public string docId { get; set; }
        public string docName { get; set; }
        public string docType { get; set; }
        public string fileName { get; set; }
        public string idNo { get; set; }
        public string insuredName { get; set; }
        public string link { get; set; }
        public string lob { get; set; }
        public string mimeType { get; set; }
        public string policyNo { get; set; }
        public string source { get; set; }
    }
}