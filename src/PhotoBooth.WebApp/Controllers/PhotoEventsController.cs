using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PhotoBooth.DAL;
using PhotoBooth.Models;

namespace PhotoBooth.WebApp.Controllers
{
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Routers;

    [Authorize(Roles = "Admin")]
    public class PhotoEventsController : Controller
    {
        private PhotoBoothContext db = new PhotoBoothContext();

        // GET: PhotoEvents
        public async Task<ActionResult> Index()
        {
            var photoEvents = this.db.PhotoEvents.Include(p => p.PhotoBoothEntity);
            return this.View(await photoEvents.ToListAsync());
        }

        // GET: PhotoEvents/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PhotoEvent photoEvent = await this.db.PhotoEvents.FindAsync(id);
            if (photoEvent == null)
            {
                return this.HttpNotFound();
            }
            return this.View(photoEvent);
        }

        // GET: PhotoEvents/Create
        public ActionResult Create()
        {
            this.ViewBag.PhotoBoothEntityId = new SelectList(this.db.PhotoBooths, "Id", "Name");
            return this.View();
        }

        // POST: PhotoEvents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Description,HashTag,StartDateTime,EndDateTime,ShowOnGallery,IsPublic,Password,LinkToLastZip,InstagrammBrandingFile,LinkToGalleryPreviewImage,PhotoBoothEntityId")] PhotoEvent photoEvent)
        {
            if (this.ModelState.IsValid)
            {
                if (photoEvent.StartDateTime != null)
                {
                    var nowDateTime = TimeZoneInfo.ConvertTime(photoEvent.StartDateTime.Value, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
                    var existEvent = await db.PhotoEvents.Where(e=>e.PhotoBoothEntityId == photoEvent.PhotoBoothEntityId)
                        .FirstOrDefaultAsync(currentBoothEvent => currentBoothEvent.StartDateTime <= nowDateTime && currentBoothEvent.EndDateTime >= nowDateTime);

                    if (existEvent != null)
                    {
                        this.ViewBag.ExistIsTrue = true;
                        this.ViewBag.PhotoBoothEntityId = new SelectList(this.db.PhotoBooths, "Id", "Name", photoEvent.PhotoBoothEntityId);
                        return this.View(photoEvent);
                    }
                }

                if (photoEvent.InstagrammBrandingFile != null)
                {
                    using (Image image = Image.FromStream(photoEvent.InstagrammBrandingFile.InputStream))
                    {
                        photoEvent.InstagrammBrandingImage = ImageToBase64(image);
                    }
                }

                photoEvent.Id = Guid.NewGuid();
                this.db.PhotoEvents.Add(photoEvent);
                await this.db.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.PhotoBoothEntityId = new SelectList(this.db.PhotoBooths, "Id", "Name", photoEvent.PhotoBoothEntityId);
            return this.View(photoEvent);
        }

        // GET: PhotoEvents/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PhotoEvent photoEvent = await this.db.PhotoEvents.FindAsync(id);
            if (photoEvent == null)
            {
                return this.HttpNotFound();
            }
            this.ViewBag.PhotoBoothEntityId = new SelectList(this.db.PhotoBooths, "Id", "Name", photoEvent.PhotoBoothEntityId);
            return this.View(photoEvent);
        }

        // POST: PhotoEvents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,HashTag,StartDateTime,EndDateTime,ShowOnGallery,IsPublic,Password,LinkToLastZip,InstagrammBrandingImage,InstagrammBrandingFile,LinkToGalleryPreviewImage,PhotoBoothEntityId")] PhotoEvent photoEvent)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(photoEvent).State = EntityState.Modified;
                if (photoEvent.InstagrammBrandingFile != null)
                {
                    using (Image image = Image.FromStream(photoEvent.InstagrammBrandingFile.InputStream))
                    {
                        photoEvent.InstagrammBrandingImage = ImageToBase64(image);
                    }
                }
                await this.db.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }
            this.ViewBag.PhotoBoothEntityId = new SelectList(this.db.PhotoBooths, "Id", "Name", photoEvent.PhotoBoothEntityId);
            return this.View(photoEvent);
        }

        // GET: PhotoEvents/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PhotoEvent photoEvent = await this.db.PhotoEvents.FindAsync(id);
            if (photoEvent == null)
            {
                return this.HttpNotFound();
            }
            return this.View(photoEvent);
        }

        // POST: PhotoEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            PhotoEvent photoEvent = await this.db.PhotoEvents.FindAsync(id);
            this.db.PhotoEvents.Remove(photoEvent);
            await this.db.SaveChangesAsync();
            return this.RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.db.Dispose();
            }
            base.Dispose(disposing);
        }

        public async Task<ActionResult> ZipFiles(Guid? id)
        {
            return await DownloadZip(id);
        }

        public async Task<ActionResult> DownloadZip(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PhotoEvent photoEvent = await this.db.PhotoEvents.FindAsync(id);
            if (photoEvent == null)
            {
                return this.HttpNotFound();
            }

            //string applicationZip = "application/zip";
            string account = CloudConfigurationManager.GetSetting("StorageAccountName");
            string key = CloudConfigurationManager.GetSetting("StorageAccountAccessKey");
            string connectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", account, key);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(id.ToString());
            container.CreateIfNotExists();

            CloudBlockBlob blockZipDownloadBlob = container.GetBlockBlobReference(id + ".zip");
            if (!blockZipDownloadBlob.Exists())
            {
                return this.HttpNotFound();
            }

            blockZipDownloadBlob.FetchAttributes();

            //Important to set buffer to false. IIS will download entire blob before passing it on to user if this is not set to false
            this.Response.Buffer = false;
            this.Response.AddHeader("Content-Disposition", "attachment; filename=" + id + ".zip");
            this.Response.AddHeader("Content-Length", blockZipDownloadBlob.Properties.Length.ToString()); //Set the length the file
            this.Response.ContentType = "application/octet-stream";
            this.Response.Flush();

            //Use the Azure API to stream the blob to the user instantly.
            // *SNIP*
            blockZipDownloadBlob.DownloadToStream(this.Response.OutputStream);
            return new EmptyResult();
        }

        public static string ImageToBase64(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, image.RawFormat);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        public static Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }
    }
}
