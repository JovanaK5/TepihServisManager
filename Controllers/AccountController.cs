using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TepihServisManager.Models;

namespace TepihServisManager.Controllers
{
    public class AccountController : Controller
    {
        // Ako ti i dalje crveni "TepihServisDBEntities", samo dodaj broj 1 na kraj: TepihServisDBEntities1
        private TepihServisDBEntities1 db = new TepihServisDBEntities1();

        // 1. REGISTRACIJA - GET
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // 2. REGISTRACIJA - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string ime, string email, string lozinka, string telefon, string adresa)
        {
            // Provjera da li email već postoji
            if (db.Korisnik.Any(k => k.Email == email))
            {
                ViewBag.Error = "Korisnik sa ovim email-om već postoji!";
                return View();
            }

            try
            {
                // Kreiramo korisnika (Usklađeno tačno sa tvojom bazom gdje ima samo Ime)
                Korisnik noviKorisnik = new Korisnik
                {
                    Ime = ime,
                    Email = email,
                    Lozinka = lozinka,
                    Uloga = "Korisnik" // Postavljamo podrazumijevanu ulogu iz baze
                };

                db.Korisnik.Add(noviKorisnik);
                db.SaveChanges(); // Čuvamo da baza generiše KorisnikID

                // Kreiramo klijenta sa istim ID-jem (Veza 1:1)
                Klijent noviKlijent = new Klijent
                {
                    KlijentID = noviKorisnik.KorisnikID,
                    Telefon = telefon,
                    Adresa = adresa
                };

                db.Klijent.Add(noviKlijent);
                db.SaveChanges();

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Greška na serveru: " + ex.Message;
                return View();
            }
        }

        // 3. LOGIN - GET
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // 4. LOGIN - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string lozinka)
        {
            var korisnik = db.Korisnik.FirstOrDefault(k => k.Email == email && k.Lozinka == lozinka);

            if (korisnik != null)
            {
                Session["KorisnikID"] = korisnik.KorisnikID;
                Session["Ime"] = korisnik.Ime;
                Session["Uloga"] = korisnik.Uloga;

                if (korisnik.Uloga == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                return RedirectToAction("MojeNarudzbine", "Korisnik");
            }

            ViewBag.Error = "Pogrešan email ili lozinka!";
            return View();
        }

        // 5. LOGOUT
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
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