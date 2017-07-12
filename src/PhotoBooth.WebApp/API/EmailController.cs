using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using PhotoBooth.DAL;
using PhotoBooth.Models;
using PhotoBooth.WebApp.Routers;

namespace PhotoBooth.WebApp.API
{
    public class EmailController : ApiController
    {
        private PhotoBoothContext db = new PhotoBoothContext();

        public EmailController()
        {
            db.Configuration.LazyLoadingEnabled = false;
        }

        // POST: api/PrintQueues
        [ResponseType(typeof(Email))]
        public async Task<IHttpActionResult> PostEmail(Email email)
        {
            if (email.PhotoId == Guid.Empty)
            {
                email.PhotoId = Guid.NewGuid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var photo = db.Photos.FirstOrDefault(p=>p.Id == email.PhotoId);
            if (photo == null)
            {
                return NotFound();
            }

            string account = CloudConfigurationManager.GetSetting("StorageAccountName");
            string key = CloudConfigurationManager.GetSetting("StorageAccountAccessKey");
            string connectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", account, key);

            Attachment attachment;
            var stream = new MemoryStream();
            CloudStorageAccount.Parse(connectionString)
                .CreateCloudBlobClient()
                .GetContainerReference(photo.PhotoEventId.ToString())
                .GetBlockBlobReference(photo.ImageName)
                .DownloadToStream(stream);

            stream.Seek(0, SeekOrigin.Begin);
            attachment = new Attachment(stream, "joyenjoy" + Path.GetExtension(photo.ImageName));

            var message = new MailMessage();
            message.To.Add(new MailAddress(email.To)); //replace with valid value
            message.Subject = "joyenjoy.me";
            message.Body = "joyenjoy.me";
            message.Attachments.Add(attachment);
            message.IsBodyHtml = true;
            message.SubjectEncoding = Encoding.Default;
            message.BodyEncoding = Encoding.Default;
            message.Headers["Content-type"] = "text/plain; charset=windows-1251";

            using (var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(message);
            }

            return CreatedAtRoute("DefaultApi", new { id = email.PhotoId }, email);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PrintQueueExists(Guid id)
        {
            return db.PrintQueue.Count(e => e.Id == id) > 0;
        }
    }
}