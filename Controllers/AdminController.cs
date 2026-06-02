using System;
using System.Linq;
using System.Web.Mvc;
using TepihServisManager.Models;

namespace TepihServisManager.Controllers
{
    public class AdminController : Controller
    {
        private TepihServisDBEntities1 db = new TepihServisDBEntities1();

        // Pregled svih narudžbina 
        public ActionResult SveNarudzbine()
        {
            // Izvlačimo sve narudžbine iz baze, sortirane od najnovijih
            // Uključujemo povezane tabele Klijent i StatusNarudzbine da bismo imali njihove podatke
            var sveNarudzbine = db.Narudzbina
                                  .Include("Klijent")
                                  .Include("StatusNarudzbine")
                                  .OrderByDescending(n => n.Datum)
                                  .ToList();

            return View(sveNarudzbine);
        }

        // Promjena statusa narudžbine - POST metoda koja prima ID narudžbine i novi ID statusa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PromijeniStatus(int narudzbinaId, int noviStatusId)
        {
            try
            {
                // Pronalazimo tačnu narudžbinu
                var narudzbina = db.Narudzbina.Find(narudzbinaId);

                if (narudzbina != null)
                {
                    // Dodjeljujemo joj novi StatusID koji je radnik izabrao
                    narudzbina.StatusID = noviStatusId;
                    db.SaveChanges();
                }

                return RedirectToAction("SveNarudzbine");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Greška pri promjeni statusa: " + ex.Message;
                return RedirectToAction("SveNarudzbine");
            }
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