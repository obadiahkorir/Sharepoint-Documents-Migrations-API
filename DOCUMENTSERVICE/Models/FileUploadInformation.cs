using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOCUMENTSERVICE.Models
{
    public class FileUploadInformation
    {
        public HttpPostedFileBase File { get; set; }
        public string FileName { get; set; }
        public string FileDirectory { get; set; }
        public string DocumentType { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string FileType { get; set; }
    }
}