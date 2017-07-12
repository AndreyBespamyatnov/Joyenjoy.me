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
    public class PhotoBoothEntitiesController : ApiController
    {
        private PhotoBoothContext db = new PhotoBoothContext();

        public PhotoBoothEntitiesController()
        {
            db.Configuration.LazyLoadingEnabled = false;
        }

        // GET: api/PhotoBoothEntities
        public IQueryable<PhotoBoothEntity> GetPhotoBooths()
        {
            return db.PhotoBooths;
        }

        // GET: api/PhotoBoothEntities/5
        [ResponseType(typeof(PhotoBoothEntity))]
        public async Task<IHttpActionResult> GetPhotoBoothEntity(Guid id)
        {
            PhotoBoothEntity photoBoothEntity = await db.PhotoBooths.FindAsync(id);
            if (photoBoothEntity == null)
            {
                return NotFound();
            }

            return Ok(photoBoothEntity);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PhotoBoothEntityExists(Guid id)
        {
            return db.PhotoBooths.Count(e => e.Id == id) > 0;
        }
    }
}