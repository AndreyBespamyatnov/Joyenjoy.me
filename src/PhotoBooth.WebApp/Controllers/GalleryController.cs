using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PhotoBooth.DAL;
using PhotoBooth.Models;

namespace PhotoBooth.WebApp.Controllers
{
    [AllowAnonymous]
    public class GalleryController : Controller
    {
        private readonly PhotoBoothContext _db = new PhotoBoothContext();

        // GET: Gallery
        public async Task<ActionResult> Index()
        {
            var photoEvents = this._db.PhotoEvents.Where(p => p.ShowOnGallery).Include(p => p.PhotoBoothEntity);
            return this.View(await photoEvents.ToListAsync());
        }

        public ActionResult ShowError(string errorText)
        {
            return this.View(model: errorText);
        }

        //[HttpPost]
        public async Task<ActionResult> GetGallery(Guid? id, string password)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PhotoEvent photoEvent = await this._db.PhotoEvents.FindAsync(id);
            if (photoEvent == null)
            {
                return this.HttpNotFound();
            }

            if (photoEvent.IsPublic)
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    return this.RedirectToAction("ShowError", new { errorText = "Вы должны ввести пароль!" });
                }

                if (photoEvent.Password != password)
                {
                    return this.RedirectToAction("ShowError", new { errorText = "Не верный пароль!" });
                }
            }

            var photos = this._db.Photos.Where(p => p.PhotoEventId == id).Include(p => p.PhotoEvent);
            return this.View(await photos.ToListAsync());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}