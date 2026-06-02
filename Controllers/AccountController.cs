using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TepihServisManager.Models;

namespace TepihServisManager.Controllers
{
    public class AccountController : Controller
    {
        private TepihServisDBEntities1 db = new TepihServisDBEntities1();

        // Registracija - GET
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // Registracija - POST
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
                // Kreiramo korisnika
                Korisnik noviKorisnik = new Korisnik
                {
                    Ime = ime,
                    Email = email,
                    Lozinka = lozinka,
                    Uloga = "Korisnik" // Podrazumijevana uloga
                };

                db.Korisnik.Add(noviKorisnik);
                db.SaveChanges();

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

        // Login - GET
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // Login - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string lozinka)
        {
            var korisnik = db.Korisnik.FirstOrDefault(k => k.Email == email && k.Lozinka == lozinka);

            if (korisnik != null)
            {
                Session["KorisnikID"] = korisnik.KorisnikID;
                Session["KorisnikIme"] = korisnik.Ime;
                Session["Uloga"] = korisnik.Uloga;

                if (korisnik.Uloga == "Admin")
                {
                    return RedirectToAction("SveNarudzbine", "Admin");
                }
                return RedirectToAction("MojeNarudzbine", "Korisnik");
            }

            ViewBag.Error = "Pogrešan email ili lozinka!";
            return View();
        }

        // Metoda za odjavu
        public ActionResult Logout()
        {
            Session.Clear(); // Briše sve podatke iz sesije
            Session.Abandon();
            return RedirectToAction("Login", "Account"); // Vraća na login ekran
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