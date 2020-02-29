using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GorevYoneticisi.Models;

namespace GorevYoneticisi.Controllers
{
    public class DBController : Controller
    {
        // GET: DB
        public ActionResult Index()
        {
            return View();
        }

        //Kullanıcının ID ve Sifresinin Geçerli olup olmadığını kontrol eder
        bool isUserValid(string kullaniciAdi, string sifre)
        {
            using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
            {
                var userlist = db.KULLANICI.Where(x => x.Kullanici_Adi == kullaniciAdi).ToList<KULLANICI>();
                if (userlist.Count == 0){return true;}
                else{return false;}
            }
        }
        //Kullanıcı Kayıt edilir
        public ActionResult UserRegister(KULLANICI kullanici)
        {
            using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
            {
                if (isUserValid(kullanici.Kullanici_Adi,kullanici.Sifre))
                {
                    if (db.KULLANICI.Count() != 0)
                    {
                        kullanici.ID = db.KULLANICI.Max(item => item.ID) + 1;
                    }
                    else
                    {
                        kullanici.ID = 1;
                    }
                    db.KULLANICI.Add(kullanici);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Kayıt Başarılı" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, message = "Kullanıcı Adı veya Şifre Kullanılıyor" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //Sisteme girilen aylık görevleri, DataTable(ajax) tablolar ile görüntülenmek üzere json formatında döndürür
        public ActionResult getMonthlyMissions()
        {
            DateTime today = DateTime.Today;
            int userID = Int16.Parse(Session["userID"].ToString());
            using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                List<GOREV> gorevList = db.GOREV.Where(x => x.Gorev_Tipi == "Aylik" && x.Tamamlandi_Mi != true && x.Son_Tarih > today && x.UserID == userID).ToList<GOREV>();
                return Json(new { data = gorevList }, JsonRequestBehavior.AllowGet);
            }
        }
        //Sisteme girilen haftalık görevleri, DataTable(ajax) tablolar ile görüntülenmek üzere json formatında döndürür
        public ActionResult getWeeklyMissions()
        {
            DateTime today = DateTime.Today;
            int userID = Int16.Parse(Session["userID"].ToString());
            using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                List<GOREV> gorevList = db.GOREV.Where(x => x.Gorev_Tipi == "Haftalik" && x.Tamamlandi_Mi != true && x.Son_Tarih > today && x.UserID == userID).ToList<GOREV>();
                return Json(new { data = gorevList }, JsonRequestBehavior.AllowGet);
            }
        }
        //Sisteme girilen Günlük görevleri, DataTable(ajax) tablolar ile görüntülenmek üzere json formatında döndürür
        public ActionResult getDailyMissions()
        {
            DateTime today = DateTime.Today;
            int userID = Int16.Parse(Session["userID"].ToString());
            using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                List<GOREV> gorevList = db.GOREV.Where(x => x.Gorev_Tipi == "Gunluk" && x.Tamamlandi_Mi != true && x.Son_Tarih>=today && x.UserID == userID).ToList<GOREV>();
                return Json(new { data = gorevList }, JsonRequestBehavior.AllowGet);
            }
        }
        //Tamamlanmış olan veya deadline süresi geçmiş olan görevleri döndürür
        public ActionResult getCompletedOrFailedMissions()
        {
            DateTime today = DateTime.Today;
            int userID = Int16.Parse(Session["userID"].ToString());
            using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                List<GOREV> gorevList = db.GOREV.Where(x => (x.Tamamlandi_Mi != true && x.Son_Tarih < today) || x.Tamamlandi_Mi == true && x.UserID == userID).ToList<GOREV>();
                return Json(new { data = gorevList }, JsonRequestBehavior.AllowGet);
            }
        }

        //Tabloda "düzenle" butonuna tıklandığında popupta görüntülenecek görev bilgilerini getirir 
        [HttpGet]
        public ActionResult addOrEditDailyMission(int id = 0)
        {
            if (id == 0)
            {
                return View(new GOREV());
            }
            else
            {
                using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
                {
                    return View(db.GOREV.Where(x => x.GorevID == id).FirstOrDefault<GOREV>());
                }
            }
        }
        //Veritabanına yeni görev girer. Eğer düzenle popup'ından geliniyorsa görev bilgilerini günceller
        [HttpPost]
        public ActionResult addOrEditDailyMission(GOREV gorev)
        {
            using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
            {
                if (gorev.GorevID == 0)
                {
                    if (db.GOREV.Count() != 0)
                    {
                        gorev.GorevID = db.GOREV.Max(item => item.GorevID) + 1;
                    }
                    else
                    {
                        gorev.GorevID = 1;
                    }
                    gorev.UserID = Int16.Parse(Session["userID"].ToString());

                    gorev.Gorev_Tipi = "Gunluk";
                    gorev.Son_Tarih = gorev.Baslangic_Tarihi;

                    db.GOREV.Add(gorev);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Başarıyla Kaydedildi" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    gorev.UserID = Int16.Parse(Session["userID"].ToString());
                    gorev.Gorev_Tipi = "Gunluk";

                    gorev.Son_Tarih = gorev.Baslangic_Tarihi;

                    db.Entry(gorev).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { success = true, message = "Başarıyla Güncellendi" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        //Tabloda "düzenle" butonuna tıklandığında popupta görüntülenecek görev bilgilerini getirir 
        [HttpGet]
        public ActionResult addOrEditWeeklyMission(int id = 0)
        {
            if (id == 0)
            {
                return View(new GOREV());
            }
            else
            {
                using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
                {
                    return View(db.GOREV.Where(x => x.GorevID == id).FirstOrDefault<GOREV>());
                }
            }
        }
        //Veritabanına yeni görev girer. Eğer düzenle popup'ından geliniyorsa görev bilgilerini günceller
        [HttpPost]
        public ActionResult addOrEditWeeklyMission(GOREV gorev)
        {
            using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
            {
                if (gorev.GorevID == 0)
                {
                    if (db.GOREV.Count() != 0)
                    {
                        gorev.GorevID = db.GOREV.Max(item => item.GorevID) + 1;
                    }
                    else
                    {
                        gorev.GorevID = 1;
                    }
                    gorev.UserID = Int16.Parse(Session["userID"].ToString());

                    gorev.Gorev_Tipi = "Haftalik";
                    gorev.Son_Tarih = gorev.Baslangic_Tarihi.Value.AddDays(7);

                    db.GOREV.Add(gorev);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Başarıyla Kaydedildi" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    gorev.UserID = Int16.Parse(Session["userID"].ToString());
                    gorev.Gorev_Tipi = "Haftalik";

                    gorev.Son_Tarih = gorev.Baslangic_Tarihi.Value.AddDays(7);

                    db.Entry(gorev).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { success = true, message = "Başarıyla Güncellendi" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        //Tabloda "düzenle" butonuna tıklandığında popupta görüntülenecek görev bilgilerini getirir 
        [HttpGet]
        public ActionResult addOrEditMonthlyMission(int id = 0)
        {
            if (id == 0)
            {
                return View(new GOREV());
            }
            else
            {
                using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
                {
                    return View(db.GOREV.Where(x => x.GorevID == id).FirstOrDefault<GOREV>());
                }
            }
        }
        //Veritabanına yeni görev girer. Eğer düzenle popup'ından geliniyorsa görev bilgilerini günceller
        [HttpPost]
        public ActionResult addOrEditMonthlyMission(GOREV gorev)
        {
            using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
            {
                if (gorev.GorevID == 0)
                {
                    if (db.GOREV.Count() != 0)
                    {
                        gorev.GorevID = db.GOREV.Max(item => item.GorevID) + 1;
                    }
                    else
                    {
                        gorev.GorevID = 1;
                    }
                    gorev.UserID = Int16.Parse(Session["userID"].ToString());

                    gorev.Gorev_Tipi = "Aylik";
                    gorev.Son_Tarih = gorev.Baslangic_Tarihi.Value.AddMonths(1);

                    db.GOREV.Add(gorev);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Başarıyla Kaydedildi" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    gorev.UserID = Int16.Parse(Session["userID"].ToString());
                    gorev.Gorev_Tipi = "Aylik";

                    gorev.Son_Tarih = gorev.Baslangic_Tarihi.Value.AddMonths(1);

                    db.Entry(gorev).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { success = true, message = "Başarıyla Güncellendi" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        //ID'si verilen görevi siler
        [HttpPost]
        public ActionResult deleteMission(int id)
        {
            using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
            {
                GOREV gorev = db.GOREV.Where(x => x.GorevID == id).FirstOrDefault<GOREV>();
                db.GOREV.Remove(gorev);
                db.SaveChanges();
                return Json(new { success = true, message = "Başarıyla Silindi" }, JsonRequestBehavior.AllowGet);
            }
        }
        //ID'si verilen görevi tamamlandı olarak işaretler
        public ActionResult CompleteMission(int id)
        {
            using (GorevYoneticisiEntities db = new GorevYoneticisiEntities())
            {
                GOREV gorev = db.GOREV.Where(x => x.GorevID == id).FirstOrDefault<GOREV>();
                gorev.Tamamlandi_Mi = true;
                db.Entry(gorev).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, message = "Gorev Tamamlandı!" }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}