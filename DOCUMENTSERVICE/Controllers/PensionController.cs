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
using System.Web;
using System.Web.Http;

namespace DOCUMENTSERVICE.Controllers
{
    public class PensionController : ApiController
    {
        /// <summary>
        ///  Get all Pension Documents for a given Policy.->Get document details associated with a particular pension number
        ///  pension must be sent with all client requests. Ensure that you replace any forward slashes(/) within the policy_number string with underscores(_) before sending your request e.g P/NRB/2011/2010/47429 should be sent as P_NRB_2011_2010_47429
        /// </summary>
        [HttpGet]
        [Route("api/pension/pension_documents/{file_name}")]
        public IHttpActionResult GetPensionDocument(string file_name)
        {
            List<PolicyFiles> response = new List<PolicyFiles>();
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
                                    var filedetails = new PolicyFiles();
                                    var fileName = resultRow["Title"].ToString();
                                    var filetype = resultRow["FileType"].ToString();
                                    var ParentLink = resultRow["ParentLink"].ToString();
                                    var fileLink = ParentLink + "/" + FileName;
                                    Uri fullfileUri = new Uri(fileLink);
                                    Microsoft.SharePoint.Client.File file = ctx.Web.GetFileByServerRelativeUrl(fullfileUri.AbsolutePath);
                                    ctx.Load(file);
                                    ctx.ExecuteQuery();

                                    ListItem item = file.ListItemAllFields;
                                    filedetails.dateReceived = file.TimeCreated;
                                    filedetails.docId = Convert.ToString(file.UniqueId);
                                    filedetails.docName = file.Name; ;
                                    filedetails.docType = "Pension";
                                    filedetails.fileName = file.Name;
                                    filedetails.idNo = "N/A";
                                    filedetails.link = fileLink;
                                    filedetails.mimeType = MimeMapping.GetMimeMapping(file.Name);
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
                var error = "File Name Cannot be Empty.Kindly provide a valid file Name";
                return Json(error);
            }
        }
        /// <summary>
        /// Get Document File from Sharepoint-> Provide only the File Name =>Download a specific document by the Document Name
        /// </summary>
        [HttpGet]
        [Route("api/pension/document/{file_name}")]
        public HttpResponseMessage GetDocument(string file_name)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            string FileString = string.Empty;
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
        [Route("api/pension/documentview/{file_name}")]
        public HttpResponseMessage GetDocumentView(string file_name)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            string FileString = string.Empty;
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
        [Route("api/pension/GetFileBase64String/{file_name}")]
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
        [Route("api/pension/upload")]
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
                            var sDocName = UploadPensionDocuments(filestream, filedetails.docName, sSpSiteRelativeUrl, Constants.SharepointLibrary, filedetails.policyNo, fileinfor, filedetails.docName, filesize);

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
                    error = "Pension Folder cannot be Empty.Kindly provide a valid pension folder and try again later";
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
        [Route("api/pension/UploadLifeDocumentString")]
        public HttpResponseMessage UploadLifeDocumentString([FromBody]JToken postData, HttpRequestMessage request)
        {
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
                        var fileName = filedetails.docName + "." + filedetails.fileExtension;
                        bool bbConnected = SharePointConnectionConfig.Connect(Constants.SharepointURL, Constants.SharepointUserName, Constants.SharepointPassword, Constants.SharepointDomain);

                        if (bbConnected)
                        {
                            Uri uri = new Uri(Constants.SharepointURL);
                            string sSpSiteRelativeUrl = uri.AbsolutePath;
                            byte[] bytes = Convert.FromBase64String(filedetails.file);
                            Stream filestream = new MemoryStream(bytes);
                            var sDocName = UploadPensionDocuments(filestream, fileName, sSpSiteRelativeUrl, Constants.SharepointLibrary, filedetails.policyNo, fileinfor, filedetails.fileExtension, 0);

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
                    error = "Pension Number cannot be Empty.Kindly provide a valid policy number and try again later";
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
        [Route("api/pension/GetDocumentDetails/{file_name}")]
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
        public string UploadPensionDocuments(Stream fs, string sFileName, string sSpSiteRelativeUrl, string sLibraryName, string sDirectoryName, FileUploadInformation Document, string fileExtention, long fileSize)
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
                Console.WriteLine(ex.Message);

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
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Delete the Document from Sharepoint Online
        /// </summary>
        [HttpDelete]
        [Route("api/pension/DeleteDocument/{file_name}")]
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
                        var parentFolderName = Constants.SharepointLibraryURI + "/" + filedetails.policyNo + "/";
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
    }
}
