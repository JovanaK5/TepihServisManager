using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using TepihServisManager.Models;

namespace TepihServisManager.Controllers
{
    public class AdminController : Controller
    {
        private TepihServisDBEntities1 db = new TepihServisDBEntities1();

        // Napredna pretraga i filtriranje narudžbina
        public ActionResult SveNarudzbine(string pojamPretrage, int? statusFilter)
        {
            var upit = db.Narudzbina
                         .Include(n => n.Klijent)
                         .Include(n => n.Klijent.Korisnik)
                         .Include(n => n.StatusNarudzbine);

            // Pretraga po imenu klijenta ili broju telefona
            if (!string.IsNullOrEmpty(pojamPretrage))
            {
                upit = upit.Where(n => n.Klijent.Korisnik.Ime.Contains(pojamPretrage) ||
                                       n.Klijent.Telefon.Contains(pojamPretrage));

                ViewBag.TrenutnaPretraga = pojamPretrage;
            }

            // Filtriranje po statusu 
            if (statusFilter.HasValue && statusFilter.Value > 0)
            {
                upit = upit.Where(n => n.StatusID == statusFilter.Value);
                ViewBag.TrenutniStatusFilter = statusFilter.Value;
            }

            ViewBag.Statusi = db.StatusNarudzbine.ToList();

            var sveNarudzbine = upit.OrderByDescending(n => n.Datum).ToList();
            return View(sveNarudzbine);
        }

        // Promjena statusa narudžbine
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PromijeniStatus(int narudzbinaId, int noviStatusId)
        {
            try
            {
                var narudzbina = db.Narudzbina.Find(narudzbinaId);

                if (narudzbina != null)
                {
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