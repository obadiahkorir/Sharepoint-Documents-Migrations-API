using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOCUMENTSERVICE.Models.ApiModels
{
    public class PolicyFiles
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
    public class ClaimsFiles
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
        public string claimNo { get; set; }
        public string source { get; set; }
    }
    public class FileInfor 
    {

        public string claimNo { get; set; }
        public string policyNo { get; set; }
        public string docType { get; set; }
        public string docName { get; set; }
        public string docSource { get; set; }
        public string dept { get; set; }
        public string idNo { get; set; }
        public string insuredName { get; set; }
        public HttpPostedFile file { get; set; }
    }
    public class FileInfor2
    {

        public string claimNo { get; set; }
        public string policyNo { get; set; }
        public string docType { get; set; }
        public string docName { get; set; }
        public string docSource { get; set; }
        public string dept { get; set; }
        public string idNo { get; set; }
        public string insuredName { get; set; }
        public string file { get; set; }
        public string fileExtension { get; set; }
    }
    public class Rootdetails
    {

        public string fileName { get; set; }
        public string policyNo { get; set; }
    }
}