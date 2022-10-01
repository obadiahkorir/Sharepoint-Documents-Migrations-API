using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOCUMENTSERVICE.Models
{
    public class DownloadFile
    {
        public string FileName { get; set; }
        public string FileString { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
    }
}