using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GorevYoneticisi.Models;

namespace GorevYoneticisi.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Authorize(KULLANICI kullanici)
        {
            using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
            {

                var userDetails = db.KULLANICI.Where(x => x.Kullanici_Adi == kullanici.Kullanici_Adi && x.Sifre == kullanici.Sifre).FirstOrDefault();

                if (userDetails == null)
                {
                    KULLANICI usr = new KULLANICI();
                    ViewBag.LoginErrorMessage = "Kullanıcı Adı veya Şifre Hatalı !";
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    Session["userID"] = userDetails.ID;
                    Session["UserName"] = userDetails.Ad;
                    Session["UserSurName"] = userDetails.Soyad;
                    Session["UserNickName"] = userDetails.Kullanici_Adi;
                    Session["UserPassword"] = userDetails.Sifre;

                    return RedirectToAction("Index", "KullaniciBilgileri");
                }
            }
        }

        public ActionResult LogOut()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Login");
        }
    }
}