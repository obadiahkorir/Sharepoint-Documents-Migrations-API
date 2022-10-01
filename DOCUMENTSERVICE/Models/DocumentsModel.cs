using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOCUMENTSERVICE.Models
{
    public class DocumentsModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string alfresco_doc_id { get; set; }
        public string claim_no { get; set; }
        public string date_received { get; set; }
        public string dept { get; set; }
        public string doc_name { get; set; }
        public string doc_source { get; set; }
        public string doc_type { get; set; }
        public string document_name { get; set; }
        public string document_type { get; set; }
        public string exception { get; set; }
        public string id_no { get; set; }
        public string insured_name { get; set; }
        public string member_name { get; set; }
        public string member_no { get; set; }
        public string mime_type { get; set; }
        public string moved_to_dms { get; set; }
        public string policy_no { get; set; }
        public string scan_date { get; set; }
        public string scheme_name { get; set; }
        public string scheme_no { get; set; }
        public string scheme_type { get; set; }
        public string upload_date { get; set; }
        public string date_updated { get; set; }
        public string application_name { get; set; }
        public string application_resp { get; set; }
        public string sent_to_application { get; set; }
        public string document_owner { get; set; }
        public string downloaded { get; set; }
        public string retry_count { get; set; }
        public string db_error_message { get; set; }
        public string accident_date { get; set; }
        public string desc_of_loss { get; set; }
        public string page_no { get; set; }
        public string reg_no { get; set; }
        public string document_sub_type { get; set; }
        public string link { get; set; }
        public string doc_year { get; set; }
        public string file_name { get; set; }
        public string fileExtension { get; set; }
        public string fileSize { get; set; }
        public string fileDirectory { get; set; }
        public string fileUniqueID { get; set; }
        public string SharepointStatus { get; set; }
        public string SharepointDocLink { get; set; }

    }
    public class DocumentDetails
    {
        public string file_AutherName { get; set; }
        public string file_AutherEmail { get; set; }
        public string file_UploadedOn { get; set; }
        public string file_name { get; set; }
        public string fileDirectory { get; set; }
        public string fileUniqueID { get; set; }
        public string fileRelativeURL { get; set; }
        public string filePath { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string VersionLabel { get; set; }
        public string ID { get; set; }
    }
}