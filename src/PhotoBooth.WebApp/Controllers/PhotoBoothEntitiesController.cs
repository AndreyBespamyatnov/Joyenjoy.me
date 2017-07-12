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
    public class PhotoBoothEntitiesController : Controller
    {
        private PhotoBoothContext db = new PhotoBoothContext();

        // GET: PhotoBoothEntities
        public async Task<ActionResult> Index()
        {
            return this.View(await this.db.PhotoBooths.ToListAsync());
        }

        // GET: PhotoBoothEntities/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PhotoBoothEntity photoBoothEntity = await this.db.PhotoBooths.FindAsync(id);
            if (photoBoothEntity == null)
            {
                return this.HttpNotFound();
            }
            return this.View(photoBoothEntity);
        }

        // GET: PhotoBoothEntities/Create
        public ActionResult Create()
        {
            return this.View();
        }

        // POST: PhotoBoothEntities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,PathToDSLRSettings,IPadDeviceId,LastAvailableDate")] PhotoBoothEntity photoBoothEntity)
        {
            if (this.ModelState.IsValid)
            {
                photoBoothEntity.Id = Guid.NewGuid();
                this.db.PhotoBooths.Add(photoBoothEntity);
                await this.db.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }

            return this.View(photoBoothEntity);
        }

        // GET: PhotoBoothEntities/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PhotoBoothEntity photoBoothEntity = await this.db.PhotoBooths.FindAsync(id);
            if (photoBoothEntity == null)
            {
                return this.HttpNotFound();
            }
            return this.View(photoBoothEntity);
        }

        // POST: PhotoBoothEntities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,PathToDSLRSettings,IPadDeviceId,LastAvailableDate")] PhotoBoothEntity photoBoothEntity)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(photoBoothEntity).State = EntityState.Modified;
                await this.db.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }
            return this.View(photoBoothEntity);
        }

        // GET: PhotoBoothEntities/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PhotoBoothEntity photoBoothEntity = await this.db.PhotoBooths.FindAsync(id);
            if (photoBoothEntity == null)
            {
                return this.HttpNotFound();
            }
            return this.View(photoBoothEntity);
        }

        // POST: PhotoBoothEntities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            PhotoBoothEntity photoBoothEntity = await this.db.PhotoBooths.FindAsync(id);
            this.db.PhotoBooths.Remove(photoBoothEntity);
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
    }
}
