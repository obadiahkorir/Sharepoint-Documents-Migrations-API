using DOCUMENTSERVICE.Connections;
using DOCUMENTSERVICE.Models;
using DOCUMENTSERVICE.Models.ApiModels;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace DOCUMENTSERVICE.Controllers
{
    public class LifeController : ApiController
    {
        /// <summary>
        ///  Get all Policy Documents for a given Policy.->Get document details associated with a particular policy number
        ///  policy_number must be sent with all client requests. Ensure that you replace any forward slashes(/) within the policy_number string with underscores(_) before sending your request e.g P/NRB/2011/2010/47429 should be sent as P_NRB_2011_2010_47429
        /// </summary>
        [HttpGet]
        [Route("api/Life/policy_documents/{policy_number}")]
        public IHttpActionResult GetPolicyDocuments(string policy_number)
        {
            //HttpRequestMessage request
            List<PolicyFiles> response = new List<PolicyFiles>();
            string PolicyNumber = policy_number;
            if (!string.IsNullOrEmpty(PolicyNumber))
            {

                PolicyNumber = PolicyNumber.Replace('/', '_');
                FileUploadInformation fileinfor = new FileUploadInformation();
                try
                {
                    bool bbConnected = SharePointConnectionConfig.Connect(Constants.SharepointURL, Constants.SharepointUserName, Constants.SharepointPassword, Constants.SharepointDomain);
                    if (bbConnected)
                    {
                        using (ClientContext ctx = new ClientContext(Constants.SharepointURL))
                        {
                            var secret = new SecureString();
                            var parentFolderName = Constants.SharepointLibraryURI + "/" + Constants.sMainFolder + "/" + Constants.sSubFolder + "/" + Constants.sPolicy + "/" + PolicyNumber + "/";
                            foreach (char c in Constants.SharepointPassword)
                            {
                                secret.AppendChar(c);
                            }
                            try
                            {
                                ctx.Credentials = new SharePointOnlineCredentials(Constants.SharepointUserName, secret);
                                ctx.Load(ctx.Web);
                                ctx.ExecuteQuery();

                                Uri uri = new Uri(Constants.SharepointURL);
                                string sSpSiteRelativeUrl = uri.AbsolutePath;

                                var FolderRelativeURL = Constants.SharepointURL + parentFolderName;
                                Uri fileUri = new Uri(FolderRelativeURL);

                                Folder folder = ctx.Web.GetFolderByServerRelativeUrl(fileUri.AbsolutePath);
                                ctx.Load(folder);
                                ctx.ExecuteQuery();

                                ctx.Load(folder.Files,
                                items => items.Include(
                                item => item.Name,
                                item => item.Author,
                                item => item.UniqueId,
                                item => item.TimeCreated,
                                item => item.ModifiedBy,
                                item => item.ListItemAllFields["DocumentName"],
                                item => item.ListItemAllFields["Department"],
                                item => item.ListItemAllFields["IDNumber"],
                                item => item.ListItemAllFields["MemberNames"],
                                item => item.ListItemAllFields["SchemeName"],
                                item => item.ListItemAllFields["Source"],
                                item => item.Length));
                                ctx.ExecuteQuery();

                                FileCollection PolicyFiles = folder.Files;
                                foreach (Microsoft.SharePoint.Client.File file in PolicyFiles)
                                {
                                    var Filelink = Constants.SharepointURL + Constants.SharepointLibraryURI + "/" + Constants.sMainFolder + "/" + Constants.sSubFolder + "/" + Constants.sPolicy + "/" + PolicyNumber + "/" + file.Name;
                                    var filedetails = new PolicyFiles();
                                    ListItem item = file.ListItemAllFields;
                                    string someFieldValue = item["DocumentName"] == null ? "" : item["DocumentName"].ToString();
                                    filedetails.dateReceived = file.TimeCreated;
                                    filedetails.docId = Convert.ToString(file.UniqueId);
                                    filedetails.docName = "";
                                    filedetails.docType = "Policy";
                                    filedetails.fileName = file.Name;
                                    filedetails.idNo = "N/A";
                                    filedetails.insuredName = item["MemberNames"] == null ? "" : item["MemberNames"].ToString();
                                    filedetails.link = Filelink;
                                    filedetails.lob = item["Department"] == null ? "" : item["Department"].ToString();
                                    filedetails.mimeType = MimeMapping.GetMimeMapping(file.Name);
                                    filedetails.policyNo = PolicyNumber;
                                    filedetails.source = item["Source"] == null ? "" : item["Source"].ToString();
                                    response.Add(filedetails);
                                }
                                return Json(response);
                            }
                            catch (Exception ex)
                            {
                                var error = ex.Message.ToString();
                                return Json(error);
                            }
                        }
                    }
                    else
                    {
                        var error = "Sharepoint EDMS could not be connected. Kindly try again later";
                        return Json(error);
                    }
                }
                catch (Exception ex)
                {
                    var error = ex.Message.ToString();
                    return Json(error);
                }
            }
            else
            {
                var error = "Policy Number Cannot be Empty. Kindly Provide a valid policy Number and try again later";
                return Json(error);
            }
        }
        /// <summary>
        ///  Get all Claim Documents for a given Claim =>Get documents belonging to a claim number.The claim number of documents requested. Ensure you replace all forward slashes (/) in the claim_number with underscores(_) before sending the request e.g C/KSM/20I0/20I8/6832 should  be sent as C_KSM_20I0_20I8_6832
        /// </summary>
        [HttpPost]
        [Route("api/Life/claim_documents /{claim_number}")]
        public IHttpActionResult GetClaimsDocuments(string claim_number)
        {

            List<ClaimsFiles> response = new List<ClaimsFiles>();
            string ClaimNumber = claim_number;
            if (!string.IsNullOrEmpty(ClaimNumber))
            {
                ClaimNumber = ClaimNumber.Replace('/', '_');
                FileUploadInformation fileinfor = new FileUploadInformation();
                string FolderPath = ConfigurationManager.AppSettings["FolderPath"];
                try
                {
                    bool bbConnected = SharePointConnectionConfig.Connect(Constants.SharepointURL, Constants.SharepointUserName, Constants.SharepointPassword, Constants.SharepointDomain);
                    if (bbConnected)
                    {
                        using (ClientContext ctx = new ClientContext(Constants.SharepointURL))
                        {
                            var secret = new SecureString();
                            var parentFolderName = Constants.SharepointLibraryURI + "/" + Constants.sMainFolder + "/" + Constants.sSubFolder + "/" + Constants.sPolicy + "/" + ClaimNumber + "/";
                            foreach (char c in Constants.SharepointPassword)
                            {
                                secret.AppendChar(c);
                            }
                            try
                            {
                                ctx.Credentials = new SharePointOnlineCredentials(Constants.SharepointUserName, secret);
                                ctx.Load(ctx.Web);
                                ctx.ExecuteQuery();

                                Uri uri = new Uri(Constants.SharepointURL);
                                string sSpSiteRelativeUrl = uri.AbsolutePath;

                                var FolderRelativeURL = Constants.SharepointURL + parentFolderName;
                                Uri fileUri = new Uri(FolderRelativeURL);

                                Folder folder = ctx.Web.GetFolderByServerRelativeUrl(fileUri.AbsolutePath);
                                ctx.Load(folder);
                                ctx.ExecuteQuery();

                                ctx.Load(folder.Files,
                                items => items.Include(
                                item => item.Name,
                                item => item.Author,
                                item => item.UniqueId,
                                item => item.TimeCreated,
                                item => item.ModifiedBy,
                                item => item.ListItemAllFields["DocumentName"],
                                item => item.ListItemAllFields["Department"],
                                item => item.ListItemAllFields["IDNumber"],
                                item => item.ListItemAllFields["MemberNames"],
                                item => item.ListItemAllFields["SchemeName"],
                                item => item.ListItemAllFields["Source"],
                                item => item.Length));
                                ctx.ExecuteQuery();
                                FileCollection PolicyFiles = folder.Files;
                                foreach (Microsoft.SharePoint.Client.File file in PolicyFiles)
                                {
                                    var Filelink = Constants.SharepointURL + Constants.SharepointLibraryURI + "/" + Constants.sMainFolder + "/" + Constants.sSubFolder + "/" + Constants.sPolicy + "/" + ClaimNumber + "/" + file.Name;
                                    var filedetails = new ClaimsFiles();
                                    ListItem item = file.ListItemAllFields;
                                    string someFieldValue = item["DocumentName"] == null ? "" : item["DocumentName"].ToString();
                                    filedetails.dateReceived = file.TimeCreated;
                                    filedetails.docId = Convert.ToString(file.UniqueId);
                                    filedetails.docName = "";
                                    filedetails.docType = "Claim";
                                    filedetails.fileName = file.Name;
                                    filedetails.idNo = "N/A";
                                    filedetails.insuredName = item["MemberNames"] == null ? "" : item["MemberNames"].ToString();
                                    filedetails.link = Filelink;
                                    filedetails.lob = item["Department"] == null ? "" : item["Department"].ToString();
                                    filedetails.mimeType = MimeMapping.GetMimeMapping(file.Name);
                                    filedetails.claimNo = ClaimNumber;
                                    filedetails.source = item["Source"] == null ? "" : item["Source"].ToString();
                                    response.Add(filedetails);
                                }
                                return Json(response);
                            }
                            catch (Exception ex)
                            {
                                var error = ex.Message.ToString();
                                return Json(error);
                            }
                        }
                    }
                    else
                    {
                        var error = "Sharepoint EDMS could not be connected. Kindly try again later";
                        return Json(error);
                    }

                }
                catch (Exception ex)
                {
                    var error = ex.Message.ToString();
                    return Json(error);
                }
            }
            else
            {
                var error = "Claim Number Cannot be Empty.Kindly Provide a  valid claim number and try again later";
                return Json(error);
            }
        }
        /// <summary>
        /// Get Document File from Sharepoint-> Provide only the File Name =>Download a specific document by the Document Name
        /// </summary>
        [HttpGet]
        [Route("api/Life/document/{file_name}")]
        public HttpResponseMessage GetDocument(string file_name)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            string FileName = file_name;
            if (!string.IsNullOrEmpty(FileName))
            {
                string FileString = string.Empty;
                FileName = FileName.Replace('/', '_');
                FileUploadInformation fileinfor = new FileUploadInformation();
                try
                {
                    bool bbConnected = SharePointConnectionConfig.Connect(Constants.SharepointURL, Constants.SharepointUserName, Constants.SharepointPassword, Constants.SharepointDomain);
                    if (bbConnected)
                    {
                        using (ClientContext ctx = new ClientContext(Constants.SharepointURL))
                        {
                            var secret = new SecureString();
                            foreach (char c in Constants.SharepointPassword)
                            {
                                secret.AppendChar(c);
                            }
                            try
                            {
                                ctx.Credentials = new SharePointOnlineCredentials(Constants.SharepointUserName, secret);
                                ctx.Load(ctx.Web);
                                ctx.ExecuteQuery();

                                Uri uri = new Uri(Constants.SharepointURL);
                                string sSpSiteRelativeUrl = uri.AbsolutePath;

                                List list = ctx.Web.Lists.GetByTitle("Documents");
                                KeywordQuery keywordQuery = new KeywordQuery(ctx);
                                keywordQuery.QueryText = FileName;

                                SearchExecutor searchExecutor = new SearchExecutor(ctx);

                                ClientResult<ResultTableCollection> results = searchExecutor.ExecuteQuery(keywordQuery);

                                ctx.ExecuteQuery();
                                foreach (var resultRow in results.Value[0].ResultRows)
                                {
                                    var fileName = resultRow["Title"].ToString();
                                    var filetype = resultRow["FileType"].ToString();
                                    var ParentLink = resultRow["ParentLink"].ToString();
                                    var fileLink = ParentLink + "/" + FileName;
                                    Uri fullfileUri = new Uri(fileLink);
                                    Microsoft.SharePoint.Client.File file = ctx.Web.GetFileByServerRelativeUrl(fullfileUri.AbsolutePath);
                                    ctx.Load(file);
                                    ctx.ExecuteQuery();
                                    using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                                    {
                                        ClientResult<System.IO.Stream> data = file.OpenBinaryStream();
                                        ctx.Load(file);
                                        ctx.ExecuteQuery();
                                        if (data != null)
                                        {
                                            data.Value.CopyTo(mStream);
                                            byte[] fileArray = mStream.ToArray();
                                            string b64String = Convert.ToBase64String(fileArray);
                                            FileString = b64String;
                                            var dataStream = new MemoryStream(fileArray);

                                            httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                                            httpResponseMessage.Content = new StreamContent(dataStream);
                                            httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                                            httpResponseMessage.Content.Headers.ContentDisposition.FileName = FileName;
                                            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                                            return httpResponseMessage;
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.OK, "File details could not be found on Sharepoint EDMS. Kindly provide a valid file name");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, ex.Message.ToString());
                            }
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Sharepoint EDMS could not be connected.Kindly try again later");
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, ex.Message.ToString());
                }
            }
            else
            {
                var error = "File Name Cannot be Empty.Kindly provide a valid file Name";
                return Request.CreateResponse(HttpStatusCode.OK, error);
            }

            return httpResponseMessage;
        }

        /// <summary>
        /// Get Document File from Sharepoint-> Provide only the File Name =>Download a specific document by the Document Name
        /// </summary>
        [HttpGet]
        [Route("api/Life/documentview/{file_name}")]
        public HttpResponseMessage GetDocumentView(string file_name)
        {
            MemoryStream workStream = new MemoryStream();
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            string FileName = file_name;
            if (!string.IsNullOrEmpty(FileName))
            {
                string FileString = string.Empty;
                FileName = FileName.Replace('/', '_');
                FileUploadInformation fileinfor = new FileUploadInformation();
                try
                {
                    bool bbConnected = SharePointConnectionConfig.Connect(Constants.SharepointURL, Constants.SharepointUserName, Constants.SharepointPassword, Constants.SharepointDomain);
                    if (bbConnected)
                    {
                        using (ClientContext ctx = new ClientContext(Constants.SharepointURL))
                        {
                            var secret = new SecureString();
                            foreach (char c in Constants.SharepointPassword)
                            {
                                secret.AppendChar(c);
                            }
                            try
                            {
                                ctx.Credentials = new SharePointOnlineCredentials(Constants.SharepointUserName, secret);
                                ctx.Load(ctx.Web);
                                ctx.ExecuteQuery();

                                Uri uri = new Uri(Constants.SharepointURL);
                                string sSpSiteRelativeUrl = uri.AbsolutePath;

                                List list = ctx.Web.Lists.GetByTitle("Documents");
                                KeywordQuery keywordQuery = new KeywordQuery(ctx);
                                keywordQuery.QueryText = FileName;

                                SearchExecutor searchExecutor = new SearchExecutor(ctx);

                                ClientResult<ResultTableCollection> results = searchExecutor.ExecuteQuery(keywordQuery);

                                ctx.ExecuteQuery();
                                foreach (var resultRow in results.Value[0].ResultRows)
                                {
                                    var fileName = resultRow["Title"].ToString();
                                    var filetype = resultRow["FileType"].ToString();
                                    var ParentLink = resultRow["ParentLink"].ToString();
                                    var fileLink = ParentLink + "/" + FileName;
                                    Uri fullfileUri = new Uri(fileLink);
                                    Microsoft.SharePoint.Client.File file = ctx.Web.GetFileByServerRelativeUrl(fullfileUri.AbsolutePath);
                                    ctx.Load(file);
                                    ctx.ExecuteQuery();
                                    using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                                    {
                                        ClientResult<System.IO.Stream> data = file.OpenBinaryStream();
                                        ctx.Load(file);
                                        ctx.ExecuteQuery();
                                        if (data != null)
                                        {
                                            data.Value.CopyTo(mStream);
                                            byte[] fileArray = mStream.ToArray();
                                            string b64String = Convert.ToBase64String(fileArray);
                                            FileString = b64String;
                                            var dataStream = new MemoryStream(fileArray);
                                            httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                                            httpResponseMessage.Content = new StreamContent(dataStream);
                                            httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("inline");
                                            httpResponseMessage.Content.Headers.ContentDisposition.FileName = FileName;
                                            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                                            return httpResponseMessage;
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.OK, "File details could not be found on Sharepoint EDMS. Kindly provide a valid file name");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, ex.Message.ToString());
                            }
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Sharepoint EDMS could not be connected.Kindly try again later");
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, ex.Message.ToString());
                }
            }
            else
            {
                var error = "File Name Cannot be Empty.Kindly provide a valid file Name";
                return Request.CreateResponse(HttpStatusCode.OK, error);
            }

            return httpResponseMessage;
        }
        [HttpGet]
        [Route("api/Life/GetFileBase64String/file_name")]
        public IHttpActionResult GetFileDetails(string file_name)
        {
            string FileString = string.Empty;
            string error = string.Empty;
            string FileName = file_name;
            if (!string.IsNullOrEmpty(FileName))
            {

                FileName = FileName.Replace('/', '_');
                FileUploadInformation fileinfor = new FileUploadInformation();
                try
                {
                    bool bbConnected = SharePointConnectionConfig.Connect(Constants.SharepointURL, Constants.SharepointUserName, Constants.SharepointPassword, Constants.SharepointDomain);
                    if (bbConnected)
                    {
                        using (ClientContext ctx = new ClientContext(Constants.SharepointURL))
                        {
                            var secret = new SecureString();
                            foreach (char c in Constants.SharepointPassword)
                            {
                                secret.AppendChar(c);
                            }
                            try
                            {
                                ctx.Credentials = new SharePointOnlineCredentials(Constants.SharepointUserName, secret);
                                ctx.Load(ctx.Web);
                                ctx.ExecuteQuery();

                                Uri uri = new Uri(Constants.SharepointURL);
                                string sSpSiteRelativeUrl = uri.AbsolutePath;

                                List list = ctx.Web.Lists.GetByTitle("Documents");
                                KeywordQuery keywordQuery = new KeywordQuery(ctx);
                                keywordQuery.QueryText = FileName;

                                SearchExecutor searchExecutor = new SearchExecutor(ctx);

                                ClientResult<ResultTableCollection> results = searchExecutor.ExecuteQuery(keywordQuery);

                                ctx.ExecuteQuery();
                                foreach (var resultRow in results.Value[0].ResultRows)
                                {
                                    var fileName = resultRow["Title"].ToString();
                                    var filetype = resultRow["FileType"].ToString();
                                    var ParentLink = resultRow["ParentLink"].ToString();
                                    var fileLink = ParentLink + "/" + FileName;
                                    Uri fullfileUri = new Uri(fileLink);
                                    Microsoft.SharePoint.Client.File file = ctx.Web.GetFileByServerRelativeUrl(fullfileUri.AbsolutePath);
                                    ctx.Load(file);
                                    ctx.ExecuteQuery();

                                    using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                                    {
                                        ClientResult<System.IO.Stream> data = file.OpenBinaryStream();
                                        ctx.Load(file);
                                        ctx.ExecuteQuery();
                                        if (data != null)
                                        {
                                            data.Value.CopyTo(mStream);
                                            byte[] imageArray = mStream.ToArray();
                                            string b64String = Convert.ToBase64String(imageArray);
                                            FileString = b64String;
                                            return Json(FileString);
                                        }
                                        else
                                        {
                                            error = "File Not Found ,File to be downloaded could not be found. Kindly provide a valid file name";
                                            return Json(error);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                error = ex.Message.ToString();
                                return Json(error);
                            }
                        }
                    }
                    else
                    {
                        error = "Sharepoint EDMS Could not be Connected.Kindly try again later";
                        return Json(error);
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message.ToString();
                    return Json(error);
                }
            }
            else
            {
                error = "File Name Cannot be Empty.Kindly provide a valid file Name";
                return Json(error);
            }
            return Json(FileString);
        }
        /// <summary>
        /// Upload Document to Sharepoint Online - Pass Document File
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Life/upload")]
        public IHttpActionResult UploadLifeDocument()
        {
            string error = string.Empty;
            HttpRequestMessage request = this.Request;
            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            UploadApiResponse apiresponse = new UploadApiResponse();
            FileUploadInformation fileinfor = new FileUploadInformation();
            try
            {
                FileInfor filedetails = new FileInfor();
                var content = request.Content;
                var jsonContent = content.ReadAsStringAsync().Result;
                filedetails.claimNo = HttpContext.Current.Request.Params["claimNo"];
                filedetails.policyNo = HttpContext.Current.Request.Params["policyNo"];
                filedetails.docType = HttpContext.Current.Request.Params["docType"];
                filedetails.docName = HttpContext.Current.Request.Params["docName"];
                filedetails.docSource = HttpContext.Current.Request.Params["docSource"];
                filedetails.dept = HttpContext.Current.Request.Params["dept"];
                filedetails.idNo = HttpContext.Current.Request.Params["idNo"];
                filedetails.insuredName = HttpContext.Current.Request.Params["insuredName"];
                filedetails.file = HttpContext.Current.Request.Files["file"];
                if (filedetails.policyNo != null)
                {
                    if (filedetails != null && filedetails.file.ContentLength > 0)
                    {

                        bool bbConnected = SharePointConnectionConfig.Connect(Constants.SharepointURL, Constants.SharepointUserName, Constants.SharepointPassword, Constants.SharepointDomain);

                        if (bbConnected)
                        {
                            Uri uri = new Uri(Constants.SharepointURL);
                            string sSpSiteRelativeUrl = uri.AbsolutePath;
                            long filesize = filedetails.file.ContentLength;
                            long fileSizeGBS = filesize / (1024);
                            Stream filestream = new MemoryStream(filedetails.file.ContentLength);
                            var sDocName = UploadLifeDocuments(filestream, filedetails.docName, sSpSiteRelativeUrl, Constants.SharepointLibrary, filedetails.policyNo, fileinfor, filedetails.docName, filesize);

                            using (ClientContext ctx = new ClientContext(Constants.SharepointURL))
                            {
                                var secret = new SecureString();
                                foreach (char c in Constants.SharepointPassword)
                                {
                                    secret.AppendChar(c);
                                }
                                try
                                {
                                    ctx.Credentials = new SharePointOnlineCredentials(Constants.SharepointUserName, secret);
                                    ctx.Load(ctx.Web);
                                    ctx.ExecuteQuery();
                                    var FolderRelativeURL = Constants.SharepointURL + Constants.SharepointLibraryURI + "/" + Constants.sMainFolder + "/" + Constants.sSubFolder + "/" + Constants.sPolicy + "/" + filedetails.policyNo + "/" + filedetails.docName;
                                    Uri fileUri = new Uri(FolderRelativeURL);
                                    Microsoft.SharePoint.Client.File file = ctx.Web.GetFileByServerRelativeUrl(fileUri.AbsolutePath);
                                    ctx.Load(file);
                                    ctx.ExecuteQuery();
                                    var fileUniqueId = "Created new document:" + file.UniqueId;
                                    return Json(fileUniqueId);

                                }
                                catch (Exception ex)
                                {
                                    error = ex.Message.ToString();
                                    return Json(error);
                                }
                            }
                        }
                        else
                        {
                            error = "Sharepoint site could not be connected successfully.Kindly try again later";
                            return Json(error);
                        }
                    }
                    else
                    {
                        error = "File to be uploaded could not be found. Kindly attach the file to be uploaded and try again later";
                        return Json(error);
                    }
                }
                else
                {
                    error = "Policy Number cannot be Empty.Kindly provide a valid policy number and try again later";
                    return Json(error);
                }
            }
            catch (Exception ex)
            {
                error = "File could not be Uploaded Successfully" + ex.Message.ToString();
                return Json(error);
            }
        }
        /// <summary>
        /// Upload Document to Sharepoint Online - Pass File as Base64
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Life/UploadLifeDocumentString")]
        public HttpResponseMessage UploadLifeDocumentString([FromBody]JToken postData, HttpRequestMessage request)
        {
            // Initialization  
            HttpResponseMessage response = null;
            UploadFileStringModel requestObj = JsonConvert.DeserializeObject<UploadFileStringModel>(postData.ToString());
            string error = string.Empty;
            UploadApiResponse apiresponse = new UploadApiResponse();
            FileUploadInformation fileinfor = new FileUploadInformation();
            try
            {
                FileInfor2 filedetails = new FileInfor2();
                var content = request.Content;
                var jsonContent = content.ReadAsStringAsync().Result;
                filedetails.claimNo = requestObj.claimNo;
                filedetails.policyNo = requestObj.policyNo;
                filedetails.docType = requestObj.fileExtension;
                filedetails.docName = requestObj.docName;
                filedetails.docSource = requestObj.docSource;
                filedetails.dept = requestObj.dept;
                filedetails.idNo = requestObj.idNo;
                filedetails.insuredName = requestObj.insuredName;
                filedetails.file = requestObj.base64String;
                filedetails.fileExtension = requestObj.fileExtension;
                
                if (filedetails.policyNo != null)
                {
                    if (filedetails != null && filedetails.file.Length > 0)
                    {
                        var fileName = filedetails.docName+"."+ filedetails.fileExtension;
                        bool bbConnected = SharePointConnectionConfig.Connect(Constants.SharepointURL, Constants.SharepointUserName, Constants.SharepointPassword, Constants.SharepointDomain);

                        if (bbConnected)
                        {
                            Uri uri = new Uri(Constants.SharepointURL);
                            string sSpSiteRelativeUrl = uri.AbsolutePath;
                            byte[] bytes = Convert.FromBase64String(filedetails.file);
                           // Encoding.ASCII.GetBytes(filedetails.file);
                            Stream filestream = new MemoryStream(bytes);
                            var sDocName = UploadLifeDocuments(filestream, fileName, sSpSiteRelativeUrl, Constants.SharepointLibrary, filedetails.policyNo, fileinfor, filedetails.fileExtension, 0);

                            using (ClientContext ctx = new ClientContext(Constants.SharepointURL))
                            {
                                var secret = new SecureString();
                                foreach (char c in Constants.SharepointPassword)
                                {
                                    secret.AppendChar(c);
                                }
                                try
                                {
                                    ctx.Credentials = new SharePointOnlineCredentials(Constants.SharepointUserName, secret);
                                    ctx.Load(ctx.Web);
                                    ctx.ExecuteQuery();
                                    var FolderRelativeURL = Constants.SharepointURL + Constants.SharepointLibraryURI + "/" + Constants.sMainFolder + "/" + Constants.sSubFolder + "/" + Constants.sPolicy + "/" + filedetails.policyNo + "/" + fileName;
                                    Uri fileUri = new Uri(FolderRelativeURL);
                                    Microsoft.SharePoint.Client.File file = ctx.Web.GetFileByServerRelativeUrl(fileUri.AbsolutePath);
                                    ctx.Load(file);
                                    ctx.ExecuteQuery();
                                    var fileUniqueId = "Created new document:" + file.UniqueId;
                                    response = Request.CreateResponse(HttpStatusCode.OK);
                                    response.Content = new StringContent(fileUniqueId, Encoding.UTF8, "application/json");
                                    return response;

                                }
                                catch (Exception ex)
                                {
                                    error = ex.Message.ToString();
                                    response = Request.CreateResponse(HttpStatusCode.BadRequest);
                                    response.Content = new StringContent(error, Encoding.UTF8, "application/json");
                                    return response;
                                }
                            }
                        }
                        else
                        {
                            error = "Sharepoint site could not be connected successfully.Kindly try again later";
                            response = Request.CreateResponse(HttpStatusCode.BadRequest);
                            response.Content = new StringContent(error, Encoding.UTF8, "application/json");
                            return response;
                        }
                    }
                    else
                    {
                        error = "File could not be found on Sharepoint EDMS.Kindly try to upload a valid file name";
                        response = Request.CreateResponse(HttpStatusCode.BadRequest);
                        response.Content = new StringContent(error, Encoding.UTF8, "application/json");
                        return response;
                    }

                }
                else
                {
                    error = "Policy Number cannot be Empty.Kindly provide a valid policy number and try again later";
                    response = Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.Content = new StringContent(error, Encoding.UTF8, "application/json");
                    return response;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                response = Request.CreateResponse(HttpStatusCode.BadRequest);
                response.Content = new StringContent(error, Encoding.UTF8, "application/json");
                return response;
            }
        }
        /// <summary>
        /// Get Decuments Details =>get Document Infomation from Sharepoint Online DMS
        /// </summary>
        [HttpGet]
        [Route("api/Life/GetDocumentDetails/file_name")]
        public IHttpActionResult GetDocumentDetails(string file_name)
        {
            DocumentDetails filedetails = new DocumentDetails();
            string error = string.Empty;
            string FileName = file_name;
            if (!string.IsNullOrEmpty(FileName))
            {
                FileUploadInformation fileinfor = new FileUploadInformation();
                try
                {
                    using (ClientContext ctx = new ClientContext(Constants.SharepointURL))
                    {
                        var secret = new SecureString();
                        foreach (char c in Constants.SharepointPassword)
                        {
                            secret.AppendChar(c);
                        }
                        try
                        {
                            ctx.Credentials = new SharePointOnlineCredentials(Constants.SharepointUserName, secret);
                            ctx.Load(ctx.Web);
                            ctx.ExecuteQuery();

                            Uri uri = new Uri(Constants.SharepointURL);
                            string sSpSiteRelativeUrl = uri.AbsolutePath;

                            List list = ctx.Web.Lists.GetByTitle("Documents");
                            KeywordQuery keywordQuery = new KeywordQuery(ctx);
                            keywordQuery.QueryText = FileName;

                            SearchExecutor searchExecutor = new SearchExecutor(ctx);

                            ClientResult<ResultTableCollection> results = searchExecutor.ExecuteQuery(keywordQuery);

                            ctx.ExecuteQuery();
                            foreach (var resultRow in results.Value[0].ResultRows)
                            {
                                var fileName = resultRow["Title"].ToString();
                                var filetype = resultRow["FileType"].ToString();
                                var ParentLink = resultRow["ParentLink"].ToString();
                                var fileLink = ParentLink + "/" + FileName;
                                Uri fullfileUri = new Uri(fileLink);
                                Microsoft.SharePoint.Client.File file = ctx.Web.GetFileByServerRelativeUrl(fullfileUri.AbsolutePath);
                                ctx.Load(file);
                                ctx.ExecuteQuery();

                                //Get File Owner Information
                                User SharepointUser = file.Author;
                                ctx.Load(SharepointUser);
                                ctx.ExecuteQuery();
                                if (!file.Exists)
                                {
                                    filedetails.Description = "File Details could not be found";
                                    filedetails.Status = "File Details could not be found";
                                }
                                else
                                {
                                    filedetails.file_name = file.Name;
                                    filedetails.VersionLabel = file.UIVersionLabel;
                                    filedetails.file_AutherName = SharepointUser.Title;
                                    filedetails.file_AutherEmail = SharepointUser.UserPrincipalName;
                                    filedetails.fileUniqueID = file.UniqueId.ToString();
                                    filedetails.file_UploadedOn = file.TimeCreated.ToString();
                                    filedetails.fileRelativeURL = file.ServerRelativeUrl;
                                    filedetails.filePath = file.LinkingUrl;
                                    filedetails.ID = file.UniqueId.ToString();
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            error = ex.Message.ToString();
                            return Json(error);
                        }
                    }

                }
                catch (Exception ex)
                {
                    error = ex.Message.ToString();
                    return Json(error);
                }
            }
            else
            {
                error = "File Name cannot be empty.Kindly provide a valid file Name";
                return Json(error);
            }
            return Json(filedetails);
        }
        public string UploadLifeDocuments(Stream fs, string sFileName, string sSpSiteRelativeUrl, string sLibraryName, string sDirectoryName, FileUploadInformation Document, string fileExtention, long fileSize)
        {
            string CorrectDirectoryName = sDirectoryName.Replace("\\", "/");
            var FinalDestination = Constants.sMainFolder + "/" + Constants.sSubFolder + "/" + Constants.sPolicy + "/" + CorrectDirectoryName;
            string parent_folderName = FinalDestination;
            var SharepointFileLink = Constants.SharepointURL + "/" + Constants.SharepointLibraryURI + "/" + parent_folderName + "/" + sFileName;
            var folderRelativeURL = sSpSiteRelativeUrl + "/" + parent_folderName;

            var status = string.Empty;
            try
            {
                // if a folder doesn't exists, create it
                if (!FolderExists(SharePointConnectionConfig.SPClientContext, folderRelativeURL))
                    CreateFolder(SharePointConnectionConfig.SPClientContext.Web, Constants.SharepointLibrary, parent_folderName, sSpSiteRelativeUrl);
                if (SharePointConnectionConfig.SPWeb != null)
                {

                    // Sharepoint Online
                    List documentsList = SharePointConnectionConfig.SPClientContext.Web.Lists.GetByTitle(Constants.SharepointLibrary);
                    var fileCreationInformation = new FileCreationInformation();
                    fileCreationInformation.ContentStream = fs;
                    fileCreationInformation.Overwrite = true;
                    fileCreationInformation.Url = Constants.SharepointURL + Constants.SharepointLibraryURI + "/" + parent_folderName + "/" + sFileName;
                    Microsoft.SharePoint.Client.File uploadFile = documentsList.RootFolder.Files.Add(fileCreationInformation);

                    //Metadata Updates
                    //Update the metadata for a field having name"
                    uploadFile.ListItemAllFields["DocumentName"] = sFileName;
                    uploadFile.ListItemAllFields["DateReceived"] = DateTime.UtcNow;
                    uploadFile.ListItemAllFields["DateScanned"] = DateTime.UtcNow;
                    uploadFile.ListItemAllFields.Update();

                    SharePointConnectionConfig.SPClientContext.ExecuteQuery();
                    var FolderRelativeURL = Constants.SharepointURL + Constants.SharepointLibraryURI + "/" + parent_folderName + "/" + sFileName;
                    Uri fileUri = new Uri(FolderRelativeURL);

                    Microsoft.SharePoint.Client.File file = SharePointConnectionConfig.SPClientContext.Web.GetFileByServerRelativeUrl(fileUri.AbsolutePath);
                    SharePointConnectionConfig.SPClientContext.Load(file);
                    SharePointConnectionConfig.SPClientContext.ExecuteQuery();
                    string result = file.UniqueId.ToString();

                }
            }

            catch (Exception ex)
            {
                status = ex.Message;
            }
            return status;
        }

        public string UploadDocuments(Stream fs, string sFileName, string sSpSiteRelativeUrl, string sLibraryName, string sDirectoryName, ActivityFeed Document, string fileExtention, long fileSize)
        {

            var status = string.Empty;
            try
            {
                string CorrectDirectoryName = sDirectoryName.Replace("\\", "/");
                var FinalDestination = Constants.sMainFolder + "/" + Constants.sSubFolder + "/" + CorrectDirectoryName;
                string parent_folderName = FinalDestination;
                var SharepointFileLink = Constants.SharepointURL + "/" + Constants.SharepointLibraryURI + "/" + parent_folderName + "/" + sFileName;
                var folderRelativeURL = sSpSiteRelativeUrl + "/" + parent_folderName;
                // if a folder doesn't exists, create it
                if (!FolderExists(SharePointConnectionConfig.SPClientContext, folderRelativeURL))
                    CreateFolder(SharePointConnectionConfig.SPClientContext.Web, Constants.SharepointLibrary, parent_folderName, sSpSiteRelativeUrl);

                if (SharePointConnectionConfig.SPWeb != null)
                {

                    // Sharepoint Online
                    List documentsList = SharePointConnectionConfig.SPClientContext.Web.Lists.GetByTitle(Constants.SharepointLibrary);
                    var fileCreationInformation = new FileCreationInformation();
                    fileCreationInformation.ContentStream = fs;
                    fileCreationInformation.Overwrite = true;
                    fileCreationInformation.Url = Constants.SharepointURL + Constants.SharepointLibraryURI + "/" + parent_folderName + "/" + sFileName;
                    Microsoft.SharePoint.Client.File uploadFile = documentsList.RootFolder.Files.Add(fileCreationInformation);
                    //Metadata Updates
                    uploadFile.ListItemAllFields["DocumentName"] = Document.filename;
                    uploadFile.ListItemAllFields["DateReceived"] = DateTime.UtcNow;
                    uploadFile.ListItemAllFields["DateScanned"] = DateTime.UtcNow;
                    uploadFile.ListItemAllFields.Update();
                    SharePointConnectionConfig.SPClientContext.ExecuteQuery();
                    status = "success";
                    var FolderRelativeURL = Constants.SharepointURL + Constants.SharepointLibraryURI + "/" + parent_folderName + "/" + sFileName;
                    Uri fileUri = new Uri(FolderRelativeURL);
                    Microsoft.SharePoint.Client.File file = SharePointConnectionConfig.SPClientContext.Web.GetFileByServerRelativeUrl(fileUri.AbsolutePath);
                    SharePointConnectionConfig.SPClientContext.Load(file);
                    SharePointConnectionConfig.SPClientContext.ExecuteQuery();
                    string result = "";
                    result += "File Name:" + file.Name + ",";
                    result += "File UniqeId:" + file.UniqueId.ToString() + ",";
                    result += "File ServerRelativeUrl:" + file.ServerRelativeUrl + ",";

                }
            }
            catch (Exception ex)
            {


            }
            return status;
        }
        private static bool FolderExists(ClientContext context, string url)
        {
            var folder = context.Web.GetFolderByServerRelativeUrl(url);
            context.Load(folder, f => f.Exists);
            try
            {
                context.ExecuteQuery();

                if (folder.Exists)
                {
                    return true;
                }
                return false;
            }
            catch (ServerUnauthorizedAccessException uae)
            {
                Trace.WriteLine($"You are not allowed to access this folder: " + uae.Message);
                throw;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Could not find folder {url}" + ex.Message);
                return false;
            }
        }
        private static void CreateFolder(Web web, string listTitle, string folderName, string sSpSiteRelativeUrl)
        {
            try
            {
                var list = web.Lists.GetByTitle(listTitle);
                var mainfolder = list.RootFolder;

                web.Context.Load(mainfolder);
                web.Context.ExecuteQuery();
                string[] Folders = folderName.Split('/');
                string SharepointLibrary = ConfigurationManager.AppSettings["S_DefaultLibrary"];
                var TotalFolders = (Folders.Count() - 1);
                var FolderPath = Folders[0];
                int FolderIndex = 0;
                var folderRelativeURL = sSpSiteRelativeUrl + "/" + FolderPath;
                foreach (var folder in Folders)
                {
                    bool exists = FolderExists(SharePointConnectionConfig.SPClientContext, folderRelativeURL);
                    if (!exists)
                    {
                        mainfolder = mainfolder.Folders.Add(folder);
                        web.Context.ExecuteQuery();
                    }
                    if (TotalFolders > FolderIndex)
                    {
                        FolderIndex += 1;
                        FolderPath = FolderPath + "/" + Folders[FolderIndex];
                    }
                    else
                    {
                        FolderPath = FolderPath + "/" + Folders[TotalFolders];
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// Delete the Document from Sharepoint Online
        /// </summary>
        [HttpDelete]
        [Route("api/Life/DeletePolicyDocument/{policy_number}/{file_name}")]
        public IHttpActionResult DeleteDocumentByPolicy(string policy_number, string file_name)
        {

            string error = string.Empty;
            Rootdetails filedetails = new Rootdetails();
            DeleteApiResponse apiresponse = new DeleteApiResponse();
            FileUploadInformation fileinfor = new FileUploadInformation();
            filedetails.policyNo = policy_number;
            filedetails.fileName = file_name;
            if (!string.IsNullOrEmpty(filedetails.policyNo) && !string.IsNullOrEmpty(filedetails.fileName))
            {
                try
                {
                    using (ClientContext ctx = new ClientContext(Constants.SharepointURL))
                    {
                        var secret = new SecureString();
                        var parentFolderName = Constants.SharepointLibraryURI + "/" + Constants.sMainFolder + "/" + Constants.sSubFolder + "/" + filedetails.policyNo + "/";
                        foreach (char c in Constants.SharepointPassword)
                        {
                            secret.AppendChar(c);
                        }
                        try
                        {
                            ctx.Credentials = new SharePointOnlineCredentials(Constants.SharepointUserName, secret);
                            ctx.Load(ctx.Web);
                            ctx.ExecuteQuery();
                            Uri uri = new Uri(Constants.SharepointURL);
                            string sSpSiteRelativeUrl = uri.AbsolutePath;
                            string filePath = sSpSiteRelativeUrl + parentFolderName + filedetails.fileName;

                            var file = ctx.Web.GetFileByServerRelativeUrl(filePath);
                            ctx.Load(file, f => f.Exists);
                            file.DeleteObject();
                            ctx.ExecuteQuery();

                            if (!file.Exists)
                            {
                                var status = "File" + " " + filedetails.fileName + "  " + "Deleted Successfully ";
                                return Json(status);
                            }
                            else
                            {
                                error = "File" + " " + filedetails.fileName + "  " + "Could not be Deleted Successfully";
                                return Json(error);
                            }
                        }
                        catch (Exception ex)
                        {
                            error = ex.Message.ToString();
                            return Json(error);
                        }
                    }

                }
                catch (Exception ex)
                {
                    error = ex.Message.ToString();
                    return Json(error);
                }
            }
            else
            {
                error = "File Name and Policy No Cannot be Empty.Kindly provide a valid file Name";
                return Json(error);
            }
        }
        /// <summary>
        /// Delete the Document from Sharepoint Online
        /// </summary>
        [HttpDelete]
        [Route("api/Life/DeleteDocument/{file_name}")]
        public IHttpActionResult DeleteDocument(string file_name)
        {
            Rootdetails filedetails = new Rootdetails();
            string error = string.Empty;
            DeleteApiResponse apiresponse = new DeleteApiResponse();
            FileUploadInformation fileinfor = new FileUploadInformation();
            string FileName = file_name;
            if (string.IsNullOrEmpty(FileName))
            {
                FileName = FileName.Replace('/', '_');
                try
                {
                    using (ClientContext ctx = new ClientContext(Constants.SharepointURL))
                    {
                        var secret = new SecureString();
                        var parentFolderName = Constants.SharepointLibraryURI + "/" + Constants.sMainFolder + "/" + Constants.sSubFolder + "/" + filedetails.policyNo + "/";
                        foreach (char c in Constants.SharepointPassword)
                        {
                            secret.AppendChar(c);
                        }
                        try
                        {
                            ctx.Credentials = new SharePointOnlineCredentials(Constants.SharepointUserName, secret);
                            ctx.Load(ctx.Web);
                            ctx.ExecuteQuery();

                            Uri uri = new Uri(Constants.SharepointURL);
                            string sSpSiteRelativeUrl = uri.AbsolutePath;

                            List list = ctx.Web.Lists.GetByTitle("Documents");
                            KeywordQuery keywordQuery = new KeywordQuery(ctx);
                            keywordQuery.QueryText = FileName;

                            SearchExecutor searchExecutor = new SearchExecutor(ctx);

                            ClientResult<ResultTableCollection> results = searchExecutor.ExecuteQuery(keywordQuery);

                            ctx.ExecuteQuery();
                            foreach (var resultRow in results.Value[0].ResultRows)
                            {
                                var fileName = resultRow["Title"].ToString();
                                var filetype = resultRow["FileType"].ToString();
                                var ParentLink = resultRow["ParentLink"].ToString();
                                var fileLink = ParentLink + "/" + FileName;
                                Uri fullfileUri = new Uri(fileLink);
                                Microsoft.SharePoint.Client.File file = ctx.Web.GetFileByServerRelativeUrl(fullfileUri.AbsolutePath);
                                ctx.Load(file);
                                ctx.Load(file, f => f.Exists);
                                file.DeleteObject();
                                if (!file.Exists)
                                {
                                    var status = "File Deleted Successfully";
                                    return Json(status);
                                }
                                else
                                {
                                    error = "File Details could not be deleted on Sharepoint EDMS. Kindly provide a valid file Name";
                                    return Json(error);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            error = ex.Message.ToString();
                            return Json(error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message.ToString();
                    return Json(error);
                }
            }
            else
            {
                error = "File Details could not be found. Please provide a valid File Name";
                return Json(error);
            }
            return Json(apiresponse);
        }
        /// <summary>
        /// 
        /// </summary>
        public static bool FolderExists(Web web, string listTitle, string folderUrl)
        {
            var list = web.Lists.GetByTitle(listTitle);
            var folders = list.GetItems(CamlQuery.CreateAllFoldersQuery());
            web.Context.Load(list.RootFolder);
            web.Context.Load(folders);
            web.Context.ExecuteQuery();
            var folderRelativeUrl = string.Format("{0}/{1}", list.RootFolder.ServerRelativeUrl, folderUrl);
            return Enumerable.Any(folders, folderItem => (string)folderItem["FileRef"] == folderRelativeUrl);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="web"></param>
        /// <param name="listTitle"></param>
        /// <param name="folderName"></param>
        private static void CreateFolder(Web web, string listTitle, string folderName)
        {
            var list = web.Lists.GetByTitle(listTitle);
            string[] Folders = folderName.Split('/');
            string SharepointLibrary = ConfigurationManager.AppSettings["S_DefaultLibrary"];
            var TotalFolders = (Folders.Count() - 1);
            var FolderPath = Folders[0];
            int FolderIndex = 0;
            foreach (var folder in Folders)
            {
                if (!FolderExists(SharePointConnectionConfig.SPClientContext.Web, SharepointLibrary, FolderPath))
                {
                    var folderCreateInfo = new ListItemCreationInformation
                    {
                        UnderlyingObjectType = FileSystemObjectType.Folder,
                        LeafName = FolderPath
                    };
                    var folderItem = list.AddItem(folderCreateInfo);
                    folderItem.Update();
                    web.Context.ExecuteQuery();
                }
                if (TotalFolders > FolderIndex)
                {
                    FolderIndex += 1;
                    FolderPath = FolderPath + "/" + Folders[FolderIndex];
                }
                else
                {
                    FolderPath = FolderPath + "/" + Folders[TotalFolders];
                }
            }

        }
        /// <summary>
        ///  Download Sharepoint Online Document
        /// </summary>
        [HttpGet]
        [Route("api/Life/DownloadPolicyDocument/{policy_number}/{file_name}")]
        public HttpResponseMessage DownloadLifePolicyDocument(string policy_number, string file_name)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            DownloadFile filedetails = new DownloadFile();
            string FileName = file_name;
            string PolicyNo = policy_number;
            if (!string.IsNullOrEmpty(FileName) && !string.IsNullOrEmpty(PolicyNo))
            {
                FileUploadInformation fileinfor = new FileUploadInformation();

                try
                {
                    using (ClientContext ctx = new ClientContext(Constants.SharepointURL))
                    {
                        var secret = new SecureString();
                        var parentFolderName = Constants.SharepointLibraryURI + "/" + Constants.sMainFolder + "/" + Constants.sSubFolder + "/" + Constants.sPolicy + "/" + PolicyNo + " / ";
                        foreach (char c in Constants.SharepointPassword)
                        {
                            secret.AppendChar(c);
                        }
                        try
                        {
                            Uri uri = new Uri(Constants.SharepointURL);
                            string sSpSiteRelativeUrl = uri.AbsolutePath;
                            string filePath = sSpSiteRelativeUrl + parentFolderName + FileName;
                            ctx.Credentials = new SharePointOnlineCredentials(Constants.SharepointUserName, secret);
                            ctx.Load(ctx.Web);
                            ctx.ExecuteQuery();

                            string folderultimatepath = Constants.SharepointURL + parentFolderName;
                            Uri folderuri = new Uri(folderultimatepath);
                            string folder = folderuri.AbsolutePath;
                            var files = ctx.Web.GetFolderByServerRelativeUrl(folder).Files;
                            ctx.Load(files);
                            ctx.ExecuteQuery();
                            if (files.Count > 0)
                            {
                                foreach (Microsoft.SharePoint.Client.File file in files)
                                {
                                    ClientResult<System.IO.Stream> data = file.OpenBinaryStream();
                                    ctx.Load(file);
                                    ctx.ExecuteQuery();
                                    using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                                    {
                                        if (data != null)
                                        {
                                            data.Value.CopyTo(mStream);
                                            byte[] fileArray = mStream.ToArray();
                                            string b64String = Convert.ToBase64String(fileArray);
                                            var dataStream = new MemoryStream(fileArray);

                                            httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                                            httpResponseMessage.Content = new StreamContent(dataStream);
                                            httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                                            httpResponseMessage.Content.Headers.ContentDisposition.FileName = FileName;
                                            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                                            return httpResponseMessage;
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.OK, "File to be downloaded could not be found. Kindly provide a valid file name");

                                        }
                                    }
                                }
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, "Folder does not have files. Kindly provide a valid file name");

                            }
                        }
                        catch (Exception ex)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, ex.Message.ToString());
                        }
                    }

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "File Could not be downloaded. Kindly try again later" + " " + ex.Message.ToString());

                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, "File Name and Policy No Cannot be Empty.Kindly provide a valid file Name");

            }

            return httpResponseMessage;
        }

    }
}
