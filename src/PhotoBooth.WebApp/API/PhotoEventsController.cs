using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using PhotoBooth.DAL;
using PhotoBooth.Models;

namespace PhotoBooth.WebApp.API
{
    public class PhotoEventsController : ApiController
    {
        private PhotoBoothContext db = new PhotoBoothContext();

        public PhotoEventsController()
        {
            db.Configuration.LazyLoadingEnabled = false;
        }

        // GET: api/PhotoEvents/5
        [ResponseType(typeof(PhotoEvent))]
        public async Task<IHttpActionResult> GetPhotoEvent(Guid id)
        {
            var nowDateTime  = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
            var currentBoothEvents = await db.PhotoEvents.Where(pe => pe.PhotoBoothEntityId == id).Include(x => x.Photos).ToListAsync();
            PhotoEvent photoEvent = currentBoothEvents.FirstOrDefault(currentBoothEvent => currentBoothEvent.StartDateTime <= nowDateTime && currentBoothEvent.EndDateTime >= nowDateTime);
            //PhotoEvent photoEvent = currentBoothEvents.FirstOrDefault();

            if (photoEvent == null)
            {
                return NotFound();
            }

            return Ok(photoEvent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PhotoEventExists(Guid id)
        {
            return db.PhotoEvents.Count(e => e.Id == id) > 0;
        }
    }
}