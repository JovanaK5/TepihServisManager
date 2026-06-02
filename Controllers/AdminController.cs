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

        // Prikaz svih narudžbina sa mogućnošću pretrage po imenu klijenta
        public ActionResult SveNarudzbine(string pojamPretrage)
        {
          
            var upit = db.Narudzbina
                         .Include(n => n.Klijent)
                         .Include(n => n.Klijent.Korisnik) 
                         .Include(n => n.StatusNarudzbine);

            // Ako je admin unio tekst u pretragu, filtriramo po imenu iz tabele Korisnik
            if (!string.IsNullOrEmpty(pojamPretrage))
            {
                upit = upit.Where(n => n.Klijent.Korisnik.Ime.Contains(pojamPretrage));
                ViewBag.TrenutnaPretraga = pojamPretrage; 
            }

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