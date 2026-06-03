using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TepihServisManager.Models;

namespace TepihServisManager.Controllers
{
    public class KorisnikController : Controller
    {
        private TepihServisDBEntities1 db = new TepihServisDBEntities1();

        private bool IsKorisnikUlogovan()
        {
            return Session["KorisnikID"] != null;
        }

        // Prikaz svih narudžbina trenutnog korisnika
        public ActionResult MojeNarudzbine()
        {
            if (!IsKorisnikUlogovan())
            {
                return RedirectToAction("Login", "Account");
            }

            int trenutniKorisnikId = Convert.ToInt32(Session["KorisnikID"]);

            var narudzbine = db.Narudzbina
                               .Where(n => n.KlijentID == trenutniKorisnikId)
                               .OrderByDescending(n => n.Datum)
                               .ToList();

            return View(narudzbine);
        }

        // Kreiranje nove narudžbine - GET (Prikaz prazne stranice)
        [HttpGet]
        public ActionResult NovaNarudzbina()
        {
            if (!IsKorisnikUlogovan())
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // Kreiranje nove narudžbine - POST (Snimanje u bazu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NovaNarudzbina(FormCollection forma)
        {
            if (!IsKorisnikUlogovan())
            {
                return RedirectToAction("Login", "Account");
            }

            int trenutniKorisnikId = Convert.ToInt32(Session["KorisnikID"]);

            try
            {
                Narudzbina novaNarudzbina = new Narudzbina
                {
                    KlijentID = trenutniKorisnikId,
                    Datum = DateTime.Now,
                    StatusID = 1 //Početni status iz baze
                };

                db.Narudzbina.Add(novaNarudzbina);
                db.SaveChanges();

                return RedirectToAction("MojeNarudzbine");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Greška pri kreiranju narudžbine: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DodajOcjenuIKomentar(int ocjena, string tekstKomentara)
        {
            if (!IsKorisnikUlogovan()) return RedirectToAction("Login", "Account");

            int id = Convert.ToInt32(Session["KorisnikID"]);
            try
            {
                // Dodavanje komentara
                Komentar noviKomentar = new Komentar
                {
                    Tekst = tekstKomentara,
                    Datum = DateTime.Now,
                    KlijentID = id
                };
                db.Komentar.Add(noviKomentar);

                // Dodavanje ocjene
                Ocjena novaOcjena = new Ocjena
                {
                    Vrijednost = ocjena,
                    KlijentID = id
                };
                db.Ocjena.Add(novaOcjena);

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Greška pri slanju: " + ex.Message;
            }
            return RedirectToAction("MojeNarudzbine");
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