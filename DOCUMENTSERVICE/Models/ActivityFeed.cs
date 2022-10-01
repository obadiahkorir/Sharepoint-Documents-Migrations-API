using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOCUMENTSERVICE.Models
{
    public class ActivityFeed
    {
        public string id { get; set; }
        public string childnodeid { get; set; }
        public string filename { get; set; }
        public string parentnodeid { get; set; }
        public string uuid { get; set; }
        public string audit_creator { get; set; }
        public string audit_modifier { get; set; }
        public string file_path { get; set; }
        public string post_id { get; set; }
        public string post_date { get; set; }
        public string activity_summary { get; set; }
        public string feed_user_id { get; set; }
        public string activity_type { get; set; }
        public string activity_format { get; set; }
        public string site_network { get; set; }
        public string app_tool { get; set; }
        public string post_user_id { get; set; }
        public string feed_date { get; set; }
        public string parentNodeRef { get; set; }
        public string lastName { get; set; }
        public string title { get; set; }
        public string page { get; set; }
        public string nodeRef { get; set; }
        public string firstName { get; set; }
        public string scanned_date { get; set; }
        public string document_type { get; set; }
        public string department { get; set; }
        public string creation_date { get; set; }
        public string modification_date { get; set; }
        public string filesize { get; set; }
    }
}