using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOCUMENTSERVICE.Models
{
    public class UploadApiResponse
    {
        public string Status { get; set; }

        public string ErrorDescription { get; set; }

        public bool Uploaded { get; set; }
    }
    public class DeleteApiResponse
    {
        public string Status { get; set; }

        public string ErrorDescription { get; set; }

        public bool Deleted { get; set; }
    }
}