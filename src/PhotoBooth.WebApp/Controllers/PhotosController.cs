using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PhotoBooth.DAL;
using PhotoBooth.Models;

namespace PhotoBooth.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PhotosController : Controller
    {
        private PhotoBoothContext db = new PhotoBoothContext();

        // GET: Photos
        public async Task<ActionResult> Index()
        {
            var photos = db.Photos.Include(p => p.PhotoEvent);
            return View(await photos.OrderByDescending(p => p.Created).ToListAsync());
        }

        public async Task<ActionResult> EventPhotos(Guid photoEventId)
        {
            var photos = db.Photos.Include(p => p.PhotoEvent).Where(p => p.PhotoEventId == photoEventId);
            return View("Index", await photos.OrderByDescending(p => p.Created).ToListAsync());
        }

        // GET: Photos/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Photo photo = await db.Photos.FindAsync(id);
            if (photo == null)
            {
                return HttpNotFound();
            }
            return View(photo);
        }

        // GET: Photos/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Photo photo = await db.Photos.FindAsync(id);
            if (photo == null)
            {
                return HttpNotFound();
            }
            ViewBag.PhotoEventId = new SelectList(db.PhotoEvents, "Id", "Name", photo.PhotoEventId);
            return View(photo);
        }

        // POST: Photos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Created,LocalPathToImage,BlobPathToImage,BlobPathToPreviewImage,Md5Hash,PhotoEventId,ImageWidth,ImageHeight,IsDeleted")] Photo photo)
        {
            if (ModelState.IsValid)
            {
                string blobPathToImage = photo.BlobPathToImage;
                if (!blobPathToImage.Contains(photo.PhotoEventId.ToString()))
                {
                    string originalGuid = blobPathToImage.Substring(40, 36);
                    Guid oldEventId;
                    if (Guid.TryParse(originalGuid, out oldEventId))
                    {
                        photo.OriginalPhotoEventId = new Guid(originalGuid);
                    }
                }
                else
                {
                    photo.OriginalPhotoEventId = Guid.Empty;
                }

                db.Entry(photo).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.PhotoEventId = new SelectList(db.PhotoEvents, "Id", "Name", photo.PhotoEventId);
            return View(photo);
        }

        // GET: Photos/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Photo photo = await db.Photos.FindAsync(id);
            if (photo == null)
            {
                return HttpNotFound();
            }
            return View(photo);
        }

        // POST: Photos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Photo photo = await db.Photos.FindAsync(id);
            db.Photos.Remove(photo);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
