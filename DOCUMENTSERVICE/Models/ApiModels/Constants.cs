using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DOCUMENTSERVICE.Models.ApiModels
{
    public static class Constants
    {
        public static  string SharepointUserName { get; set; } = ConfigurationManager.AppSettings["S_USERNAME"];
        public static string SharepointPassword { get; set; } = ConfigurationManager.AppSettings["S_PWD"];
        public static string SharepointDomain { get; set; } = ConfigurationManager.AppSettings["S_DOMAIN"];
        public static string SharepointURL { get; set; } = ConfigurationManager.AppSettings["S_URL"];
        public static string SharepointLibrary { get; set; } = ConfigurationManager.AppSettings["S_DefaultLibrary"];
        public static  string SharepointLibraryURI { get; set; } = ConfigurationManager.AppSettings["S_DefaultLibraryURI"];
        public static string sMainFolder { get; set; } = ConfigurationManager.AppSettings["S_MainFolder"];
        public static  string sSubFolder { get; set; }  = ConfigurationManager.AppSettings["S_SubFolder"];
        public static string sPolicy { get; set; }  = ConfigurationManager.AppSettings["S_Policy"];
        public static  string FolderPath { get; set; } = ConfigurationManager.AppSettings["FolderPath"];
        public static string SPension { get; set; } = ConfigurationManager.AppSettings["S_Pension"];
        public static string sPortal { get; set; } = ConfigurationManager.AppSettings["S_Portal"];
        public static string sCorperate { get; set; } = ConfigurationManager.AppSettings["S_Corperate"];
        public static string sPersonal { get; set; } = ConfigurationManager.AppSettings["S_Personal"];
        public static string sLife { get; set; } = ConfigurationManager.AppSettings["S_Life"];
    }
}