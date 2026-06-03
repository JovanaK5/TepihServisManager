using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using TepihServisManager.Models;

namespace TepihServisManager.Controllers
{
    public class AdminController : Controller
    {
        private readonly TepihServisDBEntities1 db = new TepihServisDBEntities1();

        public ActionResult SveNarudzbine(string pojamPretrage, int? statusFilter)
        {
            if (Session["KorisnikID"] == null || Session["Uloga"]?.ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var upit = db.Narudzbina
                         .Include(n => n.Klijent)
                         .Include(n => n.Klijent.Korisnik)
                         .Include(n => n.StatusNarudzbine);

            if (!string.IsNullOrEmpty(pojamPretrage))
            {
                upit = upit.Where(n => n.Klijent.Korisnik.Ime.Contains(pojamPretrage) ||
                                       n.Klijent.Telefon.Contains(pojamPretrage));
                ViewBag.TrenutnaPretraga = pojamPretrage;
            }

            if (statusFilter.HasValue && statusFilter.Value > 0)
            {
                upit = upit.Where(n => n.StatusID == statusFilter.Value);
                ViewBag.TrenutniStatusFilter = statusFilter.Value;
            }

            ViewBag.Statusi = db.StatusNarudzbine.ToList();
            var sveNarudzbine = upit.OrderByDescending(n => n.Datum).ToList();
            return View(sveNarudzbine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PromijeniStatus(int narudzbinaId, int noviStatusId)
        {
            if (Session["KorisnikID"] == null || Session["Uloga"]?.ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

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
            if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}