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
    public class PrintQueuesController : ApiController
    {
        private PhotoBoothContext db = new PhotoBoothContext();

        public PrintQueuesController()
        {
            db.Configuration.LazyLoadingEnabled = false;
        }

        // GET: api/PrintQueues
        public List<PrintQueue> GetPrintQueue()
        {
            return db.PrintQueue.ToList();
        }

        // POST: api/PrintQueues
        [ResponseType(typeof(PrintQueue))]
        public async Task<IHttpActionResult> PostPrintQueue(PrintQueue printQueue)
        {
            if (printQueue.Id == Guid.Empty)
            {
                printQueue.Id = Guid.NewGuid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.PrintQueue.Add(printQueue);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PrintQueueExists(printQueue.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = printQueue.Id }, printQueue);
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