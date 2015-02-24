using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace SageMan
{
    public enum BuildStatus
    {
        Create,
        Queued,
        Building,
        Success,
        Error,
        Cancelling,
        Cancelled
    }
    public class Model
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public string HasActiveBuild { get; set; }
        public string BuildId { get; set; }
        public string Mpr { get; set; }
        public string UsageFileNames { get; set; }
        public string CatalogId { get; set; }
        public string Description { get; set; }
        public string CatalogFileName { get; set; }
        public override string ToString()
        {
            return string.Format("{0} - {1}", Name,  Status);
        }
        public Model()
        {
            Name = Id = Date = Status = HasActiveBuild = BuildId = Mpr = "";
            UsageFileNames = CatalogId = Description = CatalogFileName = "";
        }
    }
    public class CatalogItem
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0}, Name: {1}", Id, Name);
        }
        public CatalogItem()
        {
            Id = Name = "";
        }
    }

    /// <summary>
    /// Utility class holding  build status information.
    /// </summary>
    public class Notification
    {
        public string UserName { get; set; }
        public string ModelName { get; set; }
        public string ModelId { get; set; }
        public string Message { get; set; }
        public string DateCreated { get; set; }
        public string NotificationType { get; set; }
       
        public override string ToString()
        {
            return string.Format("ModelName: {0}, Message: {1}, Date: {2}, NotificationType: {3}", ModelName, Message, DateCreated, NotificationType);
        }
        public Notification()
        {
            UserName =  ModelName = ModelId = DateCreated = Message = NotificationType = "";
        }
    }

    /// <summary>
    /// Utility class holding  build status information.
    /// </summary>
    public class BuildStatusItem
    {

        public string UserName { get; set; }
        public string ModelName { get; set; }
        public string ModelId { get; set; }
        public string IsDeployed { get; set; }
        public string BuildId { get; set; }
        public string BuildType { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
        public string Progress { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ExecutionTime { get; set; }
        public string ProgressStep { get; set; }
        public override string ToString()
        {
            return string.Format("ModelName: {0}, BuildId: {1}, Status: {2}, Progress: {3}, Exec:{4}", 
                                   ModelName, BuildId, Status, Progress, ExecutionTime);
        }
        public BuildStatusItem()
        {
            UserName = ModelName = ModelId = IsDeployed = BuildId = BuildType = "";
            Status = StatusMessage = StartTime = EndTime = ExecutionTime = ProgressStep = "";
        }
    }
    /// <summary>
    /// Utility class holding a recommended item information.
    /// </summary>
    public class CatItem
    {
        public string ExtId { get; set; }
        public string IntId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string MetaData { get; set; }
        public override string ToString()
        {
            return string.Format("Name: {0}, ExtId: {1}, IntId: {2}, Cat: {3}", Name, ExtId, IntId, Category);
        }
        public CatItem()
        {
            ExtId = IntId = Name = Category = Description = MetaData = "";
        }
    }
    /// <summary>
    /// Utility class holding a recommended item information.
    /// </summary>
    public class RecommendedItem
    {
        public string Name { get; set; }
        public string Rating { get; set; }
        public string Reasoning { get; set; }
        public string Id { get; set; }

        public override string ToString()
        {
            //return string.Format("Name: {0}, Id: {1}, Rating: {2}, Reasoning: {3}", Name, Id, Rating, Reasoning);
            return string.Format("Name: {0}, Id: {1}, Rating: {2}", Name, Id, Rating);
        }
        public RecommendedItem()
        {
            Name = Rating = Reasoning = Id = "";
        }
    }
    public class SageService
    {
        private  string RootUri { get; set; }
        private  string Pass { get; set; }
        private  HttpClient HttpClient { get; set; }

        public string  User { get; set; }
        public string AccountKey { get; set; }

        public SageService()
        {
            User = (string)Properties.Settings.Default.User;
            AccountKey = (string)Properties.Settings.Default.AccountKey;
        }

        public void SetUser( string user )
        {
            Properties.Settings.Default.User = user;
            Properties.Settings.Default.Save();
            User = user;
            Console.WriteLine("User:" + user);

        }

        public void SetAccountKey(string key)
        {
            Properties.Settings.Default.AccountKey = key;
            Properties.Settings.Default.Save();
            Console.WriteLine("Akey:" + key);
            AccountKey = key;
        }

        public bool SetCredentials(bool verbose)
        {
            RootUri = Uris.RootUri;
            HttpClient = new HttpClient();
            if (User == "" || AccountKey == "") return false;
            var byteArray = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", User, AccountKey));
            Pass = Convert.ToBase64String(byteArray);
            if (verbose)
            {
                Console.WriteLine("Login User: {0}", User);
            }
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Pass);
            HttpClient.BaseAddress = new Uri(RootUri);
            return true;
        }

        public string CreateModel(string modelName)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, String.Format(Uris.CreateModel, modelName));
            var response = HttpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(String.Format("Error {0}: Failed to create model {1}, \n reason {2}",
                    response.StatusCode, modelName, response.ReasonPhrase));
            }

            //process response if success
            string modelId = null;

            var node = XmlUtils.ExtractXmlElement(response.Content.ReadAsStreamAsync().Result, "//a:entry/a:content/m:properties/d:Id");
            if (node != null)
                modelId = node.InnerText;

            return modelId;
        }
        public string GetModelId(string name)
        {
            string id = "";
            if (MidMap.ContainsKey(name))
            {
                id = MidMap[name];
            }
            return id;
        }
        public string Name { get; set; }
 

        private string popStringOut(XmlDocument xmlDoc, XmlNamespaceManager nsmgr, string xmlpath)
        {
            string rv = "";
            XmlNode node = xmlDoc.SelectSingleNode(xmlpath, nsmgr);
            if (node != null)
            {
                rv = node.InnerText;
            }
            return rv;
        }
        private string popStringOutNode(XmlNode xmlNode, XmlNamespaceManager nsmgr, string xmlpath)
        {
            string rv = "";
            var nodes = xmlNode.SelectNodes(xmlpath);

            //if (node != null)
            //{
            //    rv = node.InnerText;
            //}
            return rv;
        }
        public class ImportReport
        {
            public string Info { get; set; }
            public int ErrorCount { get; set; }
            public int LineCount { get; set; }

            public override string ToString()
            {
                return string.Format("successfully imported {0}/{1} lines for {2}", LineCount - ErrorCount, LineCount,
                    Info);
            }
        }
        public string ImportCatalog(string modelId, string filePath)
        {
            var rv = ImportFile(modelId, filePath, Uris.ImportCatalog);
            return rv;
        }
        public string ImportUsage(string modelId, string filePath)
        {
            var rv = ImportFile(modelId, filePath, Uris.ImportUsage);
            return rv;
        }
        public string ImportFile(string modelId, string filePath, string importUri)
        {
            if (!File.Exists(filePath))
            {
                throw new Exception(
                    String.Format("File {0} does not exist",filePath));
            }
            var filestream = new FileStream(filePath, FileMode.Open);
            var fileName = Path.GetFileName(filePath);
            var imfuri = String.Format(importUri, modelId, fileName);
            var request = new HttpRequestMessage(HttpMethod.Post, imfuri );

            request.Content = new StreamContent(filestream);
            var response = HttpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    String.Format("Error {0}: Failed to import file {1}, for model {2} \n reason {3}",
                        response.StatusCode, filePath, modelId, response.ReasonPhrase));
            }

            //process response if success
            var nodeList = XmlUtils.ExtractXmlElementList(response.Content.ReadAsStreamAsync().Result,
                "//a:entry/a:content/m:properties/*");

            var report = new ImportReport { Info = fileName };
            foreach (XmlNode node in nodeList)
            {
                if ("LineCount".Equals(node.LocalName))
                {
                    report.LineCount = int.Parse(node.InnerText);
                }
                if ("ErrorCount".Equals(node.LocalName))
                {
                    report.ErrorCount = int.Parse(node.InnerText);
                }
            }
            var reportString = report.ToString();
            return reportString;
        }
        public string BuildModel(string modelId, string buildDescription)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, String.Format(Uris.BuildModel, modelId, buildDescription));
            var response = HttpClient.SendAsync(request).Result;


            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(String.Format("Error {0}: Failed to start build for model {1}, \n reason {2}",
                    response.StatusCode, modelId, response.ReasonPhrase));
            }
            string buildId = null;
            //process response if success
            var node = XmlUtils.ExtractXmlElement(response.Content.ReadAsStreamAsync().Result, "//a:entry/a:content/m:properties/d:Id");
            if (node != null)
                buildId = node.InnerText;

            return buildId;

        }

        public BuildStatus GetBuidStatus(string modelId, string buildId)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, String.Format(Uris.BuildStatuses, modelId, false));
            var response = HttpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(String.Format("Error {0}: Failed to retrieve build for status for model {1} and build id {2}, \n reason {3}",
                    response.StatusCode, modelId, buildId, response.ReasonPhrase));
            }
            var xmlStream = response.Content.ReadAsStreamAsync().Result;
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlStream);
            var nsmgr = XmlUtils.CreateNamespaceManager(xmlDoc);
            var str = string.Format("//a:entry/a:content/m:properties[d:BuildId='{0}']/d:Status", buildId);
            var buildStatusStr = popStringOut(xmlDoc, nsmgr, str);

            //string buildStatusStr = null;
            //var node = XmlUtils.ExtractXmlElement(response.Content.ReadAsStreamAsync().Result, string.Format("//a:entry/a:content/m:properties[d:BuildId='{0}']/d:Status",buildId));
            //if (node != null)
            //    buildStatusStr = node.InnerText;

            BuildStatus buildStatus;
            if (!Enum.TryParse(buildStatusStr, true, out buildStatus))
            {
                throw new Exception(string.Format("Failed to parse build status for value {0} of build {1} for model {2}", buildStatusStr, buildId, modelId));
            }

            return buildStatus;
        }

        private string GetModelName(string id)
        {
            //foreach( var entry in MidMap.)
            //if (MidMap.ContainsKey(id))
            //{
            //    return MidMap[id];
            //}
            return "";
        }
        public List<Notification> GetNotifications(string modelId)
        {
            var getnoteuri = String.Format(Uris.GetNotifications, modelId);
            if (modelId == "")
            {
                getnoteuri = String.Format(Uris.GetNotificationsAll);
            }
            var request = new HttpRequestMessage(HttpMethod.Get, getnoteuri );
            var response = HttpClient.SendAsync(request).Result;


            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(String.Format("Error {0}: Failed to get notifications for model {1}, \n reason {2}",
                    response.StatusCode, modelId, response.ReasonPhrase));
            }
            var noteList = new List<Notification>();


            var nodeList = XmlUtils.ExtractXmlElementList(response.Content.ReadAsStreamAsync().Result, "//a:entry/a:content/m:properties");

            foreach (var node in (nodeList))
            {
                var note = new Notification();
                //cycle through the recommended items
                foreach (var child in ((XmlElement)node).ChildNodes)
                {
                    //cycle through properties
                    var nodeName = ((XmlNode)child).LocalName;
                    switch (nodeName)
                    {
                        case "UserName":
                            note.UserName = ((XmlNode)child).InnerText;
                            break;
                        case "ModelId":
                            note.ModelId = ((XmlNode)child).InnerText;
                            note.ModelName = GetModelName(note.ModelId);
                            break;
                        case "Message":
                            note.Message = ((XmlNode)child).InnerText;
                            break;
                        case "DateCreated":
                            note.DateCreated = ((XmlNode)child).InnerText;
                            break;
                        case "NotificationType":
                            note.NotificationType = ((XmlNode)child).InnerText;
                            break;
                    }
                    noteList.Add(note);
                }
            }
            return noteList;
        }

        public List<CatItem> GetCatalog(string modelId)
        {
            var getcaturi = String.Format(Uris.GetCatalog, modelId);
            var request = new HttpRequestMessage(HttpMethod.Get, getcaturi);
            var response = HttpClient.SendAsync(request).Result;


            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(String.Format("Error {0}: Failed to get catalog for model {1}, \n reason {2}",
                    response.StatusCode, modelId, response.ReasonPhrase));
            }
            var catList = new List<CatItem>();


            var nodeList = XmlUtils.ExtractXmlElementList(response.Content.ReadAsStreamAsync().Result, "//a:entry/a:content/m:properties");

            foreach (var node in (nodeList))
            {
                var catInst = new CatItem();
                //cycle through the recommended items
                foreach (var child in ((XmlElement)node).ChildNodes)
                {
                    //cycle through properties
                    var nodeName = ((XmlNode)child).LocalName;
                    switch (nodeName)
                    {
                        case "ExternalId":
                            catInst.ExtId = ((XmlNode)child).InnerText;
                            break;
                        case "InternalId":
                            catInst.IntId = ((XmlNode)child).InnerText;
                            break;
                        case "Name":
                            catInst.Name = ((XmlNode)child).InnerText;
                            break;
                        case "Category":
                            catInst.Category = ((XmlNode)child).InnerText;
                            break;
                        case "Description":
                            catInst.Description = ((XmlNode)child).InnerText;
                            break;
                        case "Metadata":
                            catInst.MetaData = ((XmlNode)child).InnerText;
                            break;
                    }
                }
                catList.Add(catInst);
            }
            return catList;
        }
           

        public List<BuildStatusItem> GetUserBuildsStatus(bool allbuilds)
        {
            var gubstr =  String.Format(Uris.GetUserBuildsStatus, !allbuilds);
            // GetUserBuildsStatus?onlyLastBuilds=true&apiVersion=%271.0%27
            var request = new HttpRequestMessage(HttpMethod.Get,gubstr);
            var response = HttpClient.SendAsync(request).Result;


            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(String.Format("Error {0}: Failed to get user builds status, \n reason {1}",
                    response.StatusCode, response.ReasonPhrase));
            }

            var buildList = new List<BuildStatusItem>();


            var nodeList = XmlUtils.ExtractXmlElementList(response.Content.ReadAsStreamAsync().Result, "//a:entry/a:content/m:properties");

            foreach (var node in (nodeList))
            {
                var buildInst = new BuildStatusItem();
                //cycle through the recommended items
                foreach (var child in ((XmlElement)node).ChildNodes)
                {
                    //cycle through properties
                    var nodeName = ((XmlNode)child).LocalName;
                    switch (nodeName)
                    {
                        case "UserName":
                            buildInst.UserName = ((XmlNode)child).InnerText;
                            break;
                        case "ModelName":
                            buildInst.ModelName = ((XmlNode)child).InnerText;
                            break;
                        case "BuildId":
                            buildInst.BuildId = ((XmlNode)child).InnerText;
                            break;
                        case "Status":
                            buildInst.Status = ((XmlNode)child).InnerText;
                            break;
                        case "Progress":
                            buildInst.Progress = ((XmlNode)child).InnerText;
                            break;
                        case "IsDeployed":
                            buildInst.IsDeployed = ((XmlNode)child).InnerText;
                            break;
                        case "EndTime":
                            buildInst.EndTime = ((XmlNode)child).InnerText;
                            break;
                        case "ExecutionTime":
                            buildInst.ExecutionTime = ((XmlNode)child).InnerText;
                            break;
                    }

                }
                buildList.Add(buildInst);
            }
            return buildList;

        }

        public string DeleteModel(string modelId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, String.Format(Uris.DeleteModel, modelId));
            var response = HttpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(String.Format("Error {0}: Failed to get model {1}, \n reason {2}",
                    response.StatusCode, modelId, response.ReasonPhrase));
            }

            //process response if success

            return Name;
        }
        private void ErrorOut(string emsg)
        {
            System.Console.WriteLine(emsg);
        }
        public bool chkmname(string mname)
        {
            if (mname == "")
            {
                ErrorOut("No model name specified");
                return false;
            }
            if (!MidMap.ContainsKey(mname))
            {
                ErrorOut("Model Name:"+mname+" not defined");
                return false;
            }
            return true;
        }

        public Dictionary<string, string> MidMap = null;
        public Dictionary<string, string> MnmMap = null;
        public Dictionary<string, Model> ModelMidMap = null;
        public Dictionary<string, Model> ModelMnmMap = null;

        public List<string> Names = null;
        public List<Model> GetInventory()
        {

            var request = new HttpRequestMessage(HttpMethod.Get, String.Format(Uris.GetAllModels));
            var response = HttpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(String.Format("Error {0}: Failed to get all models, \n reason {2}",
                    response.StatusCode, response.ReasonPhrase));
            }

            //process response if success


            var xmlStream = response.Content.ReadAsStreamAsync().Result;
            var xmlDoc = new XmlDocument();
            //xmlDoc.Load(xmlStream);
            var nsmgr = XmlUtils.CreateNamespaceManager(xmlDoc);

            var modelList = new List<Model>();


            var nodeList = XmlUtils.ExtractXmlElementList(xmlStream, "//a:entry/a:content/m:properties");

            foreach (var node in (nodeList))
            {
                var model = new Model();
                //cycle through the recommended items
                foreach (var child in ((XmlElement)node).ChildNodes)
                {
                    //cycle through properties
                    var nodeName = ((XmlNode)child).LocalName;
                    switch (nodeName)
                    {
                        case "Name":
                            model.Name = ((XmlNode)child).InnerText;
                            break;
                        case "Id":
                            model.Id = ((XmlNode)child).InnerText;

                            break;
                        case "Date":
                            model.Date = ((XmlNode)child).InnerText;
                            break;
                        case "Status":
                            model.Status = ((XmlNode)child).InnerText;
                            break;
                        case "HasActiveBuild":
                            model.HasActiveBuild = ((XmlNode)child).InnerText;
                            break;
                        case "BuildId":
                            model.BuildId = ((XmlNode)child).InnerText;
                            break;
                        case "UsageFileNames":
                            model.UsageFileNames = ((XmlNode)child).InnerText;
                            break;
                        case "CatalogId":
                            model.CatalogId = ((XmlNode)child).InnerText;
                            break;
                        case "Description":
                            model.Description = ((XmlNode)child).InnerText;
                            break;
                        case "CatalogFileName":
                            model.CatalogFileName = ((XmlNode)child).InnerText;
                            break;
                    }
                }
                modelList.Add(model);
            }

            MidMap = new Dictionary<string, string>();
            MnmMap = new Dictionary<string, string>();
            ModelMidMap = new Dictionary<string, Model>();
            ModelMnmMap = new Dictionary<string, Model>();

            foreach (var model in modelList)
            {
                MidMap[model.Name] = model.Id;
                MnmMap[model.Id] = model.Name;
                ModelMidMap[model.Id] = model;
                ModelMnmMap[model.Name] = model;
            }

            return modelList;
        }

        public List<RecommendedItem> GetRecommendation(string modelId, List<string> itemIdList, int numResults,
                             bool incMetadata = false)
        {
            var joinstr = string.Join(",", itemIdList);
            var recuri = String.Format(Uris.GetRecommendation, modelId, joinstr, numResults, incMetadata);
            var request = new HttpRequestMessage(HttpMethod.Get,recuri );
            var response = HttpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    String.Format(
                        "Error {0}: Failed to retrieve recommendation for item list {1} and model {2}, \n reason {3}",
                        response.StatusCode, string.Join(",", itemIdList), modelId, response.ReasonPhrase));
            }
            var recoList = new List<RecommendedItem>();


            var nodeList = XmlUtils.ExtractXmlElementList(response.Content.ReadAsStreamAsync().Result, "//a:entry/a:content/m:properties");

            foreach (var node in (nodeList))
            {
                var item = new RecommendedItem();
                //cycle through the recommended items
                foreach (var child in ((XmlElement)node).ChildNodes)
                {
                    //cycle through properties
                    var nodeName = ((XmlNode)child).LocalName;
                    switch (nodeName)
                    {
                        case "Id":
                            item.Id = ((XmlNode)child).InnerText;
                            break;
                        case "Name":
                            item.Name = ((XmlNode)child).InnerText;
                            break;
                        case "Rating":
                            item.Rating = ((XmlNode)child).InnerText;
                            break;
                        case "Reasoning":
                            item.Reasoning = ((XmlNode)child).InnerText;
                            break;
                    }

                }
                recoList.Add(item);
            }
            return recoList;
        }
    }
    public static class Uris
    {
        // http://azure.microsoft.com/en-us/documentation/articles/machine-learning-recommendation-api-documentation/

        //public const string RootUri = "https://api.datamarket.azure.com/amla/recommendations/v1";
        public const string RootUri = "https://api.datamarket.azure.com/amla/recommendations/v2/";

        public const string CreateModel = "CreateModel?modelName=%27{0}%27&apiVersion=%271.0%27";

        public const string GetModel = "GetModel?id=%27{0}%27&apiVersion=%271.0%27";

        public const string GetAllModels = "GetAllModels?apiVersion=%271.0%27";

        public const string DeleteModel = "DeleteModel?id=%27{0}%27&apiVersion=%271.0%27";

        public const string ImportCatalog = "ImportCatalogFile?modelId=%27{0}%27&filename=%27{1}%27&apiVersion=%271.0%27";

        public const string ImportUsage = "ImportUsageFile?modelId=%27{0}%27&filename=%27{1}%27&apiVersion=%271.0%27";

        public const string BuildModel = "BuildModel?modelId=%27{0}%27&userDescription=%27{1}%27&apiVersion=%271.0%27";

        public const string BuildStatuses = "GetModelBuildsStatus?modelId=%27{0}%27&onlyLastBuild={1}&apiVersion=%271.0%27";

        public const string GetRecommendation = "ItemRecommend?modelId=%27{0}%27&itemIds=%27{1}%27&numberOfResults={2}&includeMetadata={3}&apiVersion=%271.0%27";

        public const string UpdateModel = "UpdateModel?id=%27{0}%27&apiVersion=%271.0%27";

        public const string GetNotifications = "GetNotifications?modelId=%27{0}%27&apiVersion=%271.0%27";

        public const string GetNotificationsAll = "GetNotifications?apiVersion=%271.0%27";

        public const string GetCatalog = "GetCatalog?modelId=%27{0}%27&apiVersion=%271.0%27";

        public const string GetUserBuildsStatus = "GetUserBuildsStatus?onlyLastBuilds={0}&apiVersion=%271.0%27";

    }

    public class XmlUtils
    {
        /// <summary>
        /// extract a single xml node from the given stream, given by the xPath
        /// </summary>
        /// <param name="xmlStream"></param>
        /// <param name="xPath"></param>
        /// <returns></returns>
        internal static XmlNode ExtractXmlElement(Stream xmlStream, string xPath)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlStream);
            //Create namespace manager
            var nsmgr = CreateNamespaceManager(xmlDoc);

            var node = xmlDoc.SelectSingleNode(xPath, nsmgr);
            return node;
        }

   
        public static XmlNamespaceManager CreateNamespaceManager(XmlDocument xmlDoc)
        {
            var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("a", "http://www.w3.org/2005/Atom");
            nsmgr.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            nsmgr.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
            return nsmgr;
        }

        /// <summary>
        /// extract the xml nodes from the given stream, given by the xPath
        /// </summary>
        /// <param name="xmlStream"></param>
        /// <param name="xPath"></param>
        /// <returns></returns>
        internal static XmlNodeList ExtractXmlElementList(Stream xmlStream, string xPath)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlStream);
            var nsmgr = CreateNamespaceManager(xmlDoc);
            var nodeList = xmlDoc.SelectNodes(xPath, nsmgr);
            return nodeList;
        }
    }


 
}
