using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PhotoBooth.WebApp.Routers
{
    public class ImageRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var filename = requestContext.RouteData.Values["filename"] as string;
            var containerKey = requestContext.RouteData.Values["container"] as string;

            if (string.IsNullOrEmpty(filename))
            {
                requestContext.HttpContext.Response.ClearHeaders();
                requestContext.HttpContext.Response.Clear();

                requestContext.HttpContext.Response.StatusCode = 404;
                requestContext.HttpContext.Response.SuppressContent = true;
                requestContext.HttpContext.Response.End();
            }
            else
            {
                string contentType = GetContentType(requestContext.HttpContext.Request.Url.ToString());

                string account = CloudConfigurationManager.GetSetting("StorageAccountName");
                string key = CloudConfigurationManager.GetSetting("StorageAccountAccessKey");
                string connectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", account, key);
                //_blockBlob.FetchAttributes();

                //Important to set buffer to false. IIS will download entire blob before passing it on to user if this is not set to false
                //requestContext.HttpContext.Response.AddHeader("Content-Disposition", "attachment; filename=" + _blockBlob.Name);
                requestContext.HttpContext.Response.Buffer = false;
                requestContext.HttpContext.Response.ContentType = contentType;
                requestContext.HttpContext.Response.Flush();

                //Use the Azure API to stream the blob to the user instantly.
                // *SNIP*
                CloudStorageAccount.Parse(connectionString)
                    .CreateCloudBlobClient()
                    .GetContainerReference(containerKey)
                    .GetBlockBlobReference(filename)
                    .DownloadToStream(requestContext.HttpContext.Response.OutputStream);
                requestContext.HttpContext.Response.End();
            }

            return null;
        }

        public static string GetContentType(String path)
        {
            var extension = Path.GetExtension(path);
            if (extension == null) return "";
            switch (extension.ToLowerInvariant())
            {
                case ".bmp": return "Image/bmp";
                case ".gif": return "Image/gif";
                case ".jpg": return "Image/jpeg";
                case ".png": return "Image/png";
            }
            return "";
        }
    }
}
