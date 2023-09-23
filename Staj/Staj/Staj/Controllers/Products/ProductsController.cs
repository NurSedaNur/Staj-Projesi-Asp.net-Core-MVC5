using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Staj.Data;
using Staj.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Rotativa;

namespace Staj.Controllers.Products
{
    [Authorize]
    public class ProductsController : Controller
    {
        private AppDbContext _context;
        private readonly TutanakRepository _tutanakRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductsController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _tutanakRepository = new TutanakRepository();
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var tutanaklar = _context.Tutanaklar.ToList();
            return View(tutanaklar);
        }
       //View
        public IActionResult Add()
        {
            ViewBag.allusers = _context.Logins.ToList();
            ViewBag.adminName = _context.Logins.Where(x => x.Id == Convert.ToInt32(Request.Cookies["userid"])).Select(x => x.Adı).FirstOrDefault().ToString();
            ViewBag.adminDep = _context.Logins.Where(x => x.Id == Convert.ToInt32(Request.Cookies["userid"])).Select(x => x.Departman).FirstOrDefault().ToString();
            ViewBag.adminGorev = _context.Logins.Where(x => x.Id == Convert.ToInt32(Request.Cookies["userid"])).Select(x => x.Gorev).FirstOrDefault().ToString();
            ViewBag.sonTutanakId = (_context.Tutanaklar.OrderByDescending(t => t.OlusturulmaTarihi).Select(t => t.Id).FirstOrDefault())+1;
            ViewBag.teslimedenId= _context.Logins.Where(x => x.Id == Convert.ToInt32(Request.Cookies["userid"])).Select(x => x.Id).FirstOrDefault();
            return View();
        }
        //personel infosunu almak için ol
        [Authorize(Roles = "A")]
        [HttpGet]
        public IActionResult GetPersInfo(string userName)
        {
            // userName'e göre veritabanından kullanıcı bilgilerini alın
            var kullanici = _context.Logins.Where(x=>x.Adı==userName).FirstOrDefault();
            if (kullanici != null) 
            {
                ViewBag.depAdi = kullanici.Departman;
                ViewBag.kulId = kullanici.Id;
                return Json(new { kullanici = kullanici });
            }
            else
            {
                return View();
            }           
        }
        Tutanak aa = new Tutanak();
        //yeni kayıt fonksiyonun
        [HttpPost]
        public IActionResult CreateNewForm(IFormCollection col ,IFormFile img,Tutanak tutanakMail)
        {
            int seri_no = _context.Tutanaklar.Count() + 1;
            string ttno = "TT" + (DateTime.Now.Year % 100).ToString("00") + DateTime.Now.Month.ToString("00") + seri_no.ToString("00000");
            aa.TTNO =ttno;
            aa.OlusturulmaTarihi = DateTime.Now;
            aa.IsYeri = col["IsYeri"].ToString();
            aa.Malzeme = col["Malzeme"].ToString();
            aa.Diger = col["Diger"].ToString();
            aa.TeslimTarihi = Convert.ToDateTime(col["birthdate"]);
            aa.TeslimEdenAd = col["TeslimEdenAd"].ToString();
            aa.TeslimEdenDep = col["TeslimEdenDep"];
            aa.TeslimEdenGorev = col["TeslimEdenGorev"];
            aa.TeslimAlanAd = col["TeslimAlanAd"];
            aa.TeslimAlanDep = col["TeslimAlanDep"];
            aa.TeslimAlanGorev = col["TeslimAlanGorev"];

            var teslimedenid = _context.Logins.Where(x => x.Id == Convert.ToInt32(Request.Cookies["userid"])).Select(x => x.Id).FirstOrDefault();
            aa.TeslimEdenId = teslimedenid;
            var teslimalanid = _context.Logins.Where(x => x.Adı == col["TeslimAlanAd"].ToString()).Select(x => x.Id).FirstOrDefault();
            aa.TeslimAlanId = teslimalanid;

            var wwwRootPath = _webHostEnvironment.WebRootPath;
            if (img != null && img.Length > 0)
            {
                // Dosyanın adın ve uzantısı alındı
                var fileName = Path.GetFileName(img.FileName);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                // Dosyanın yükleneceği klasör belirlendi
                var uploadPath = Path.Combine(wwwRootPath, "Image");
                // Klasör yoksa oluşturuldu
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                // Dosya klasöre kaydedildi
                var filePath = Path.Combine(uploadPath, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    img.CopyTo(fileStream);
                }
                aa.Resim = filePath; // Veritabanında dosyanın yolu tutulabilir
            }
            if (ModelState.IsValid)
            {
                SendEmail(tutanakMail);
                aa.PersMailGonDurumu = true;
                TempData["status"] = "Mail Gönderildi.";
                _context.Tutanaklar.Add(aa);
                _context.SaveChanges();
            }                       
            return RedirectToAction("Urunler");
        }
        private void SendEmail(Tutanak tutanakMail)//teslim eden formu doldurunca teslim alan kişiye sayfaya yonlendirecek linki de bulunduran bir mail atıyor.
        {
            var fromAddress = new MailAddress("bilgi@technocast.com.tr",tutanakMail.TeslimEdenAd);
            const string fromPassword = "b.22.b-b.22";
            var toAddress = new MailAddress("sedanurekici.technocast@gmail.com",tutanakMail.TeslimAlanAd);
            const string subject = "Teslim ve Tesellüm Tutanak Formu";
            string link = "https://localhost:44309/Products/Onay?id="+tutanakMail.Id+ "&teslimalanid=" + tutanakMail.TeslimAlanId; /*//https://localhost:44309/Products/Onay/95?teslimalanid=1*/
            var body = "Merhaba " + tutanakMail.TeslimAlanAd + " , \nMülkiyeti " + tutanakMail.IsYeri +
                "\'e ait olan " + tutanakMail.Malzeme + " demirbaşı " + tutanakMail.Diger + " ile birlikte çalışır durumda eksiksiz olarak teslim edilmiştir." +
                " Demirbaş olarak aldığım bu malzemeyi emeklilik, istifa veya görevden ayrılma gibi durumlarda eksiksiz ve çalışır durumda görevlilere teslim edilecektir." +
                "(Garanti kapsamına girmeyen ve kullanıcı hatasından kaynaklanan arızaların tamirinden kendisinin sorumlu olduğu imza sahibine hatırlatılmıştır.)\n" +
                "\nOnaylamak için linke gidiniz.\n" +
                " <a href='" + link+ "'>LINKK</a> ";//linke tıklayınca products->Onay açılacak.

            using (var smtp = new SmtpClient
            {
                Host = "smtp.office365.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            })
            {
            using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body, IsBodyHtml = true })
            {
                smtp.Send(message);
            }
            }
        }
        private void SendEmail2(Tutanak tutanakMail2)//teslim alan formu onaylayınca mail atmak için
        {   
            var fromAddress = new MailAddress("bilgi@technocast.com.tr", tutanakMail2.TeslimEdenAd);
            const string fromPassword = "b.22.b-b.22";
            var toAddress = new MailAddress("sedanurekici.technocast@gmail.com", tutanakMail2.TeslimAlanAd);
            const string subject = "Teslim ve Tesellüm Tutanağı Onay Formu";
            var body = "Merhaba " + tutanakMail2.TeslimEdenAd + " , \nMülkiyeti " + tutanakMail2.IsYeri +
                "\'e ait olan " + tutanakMail2.Malzeme + " demirbaşı " + tutanakMail2.Diger + "" +
                " ile birlikte çalışır durumda eksiksiz olarak teslim edilmiştir." +
               tutanakMail2.TeslimAlanAd + " aldığını onaylamıştır.\n İyi Günler.";



            ////Set Aspose license before HTML to PDF conversion
            //Aspose.Words.License AsposeWordsLicense = new Aspose.Words.License();
            //AsposeWordsLicense.SetLicense(@"Aspose.Words.lic");
            //// Import the HTML into Aspose.Words DOM. 
            //Document doc = new Document("\\Onay.cshtml");
            //// Save document to PDF file format
            //doc.Save("convert html to pdf using c#.pdf", SaveFormat.Pdf);

            //body += SaveFormat.Pdf;

            using (var smtp = new SmtpClient
            {
                Host = "smtp.office365.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            })
            {
                using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body, IsBodyHtml = true })
                {
                    smtp.Send(message);
                }
            }
        }
        //public ActionResult ConvertToPDF()
        //{

        //    //// Razor sayfanızın view adını ve modeli belirtin
        //    //var pdfView = new ViewAsPdf("Onay",Tutanak);

        //    //// PDF'e dönüştürmek için Rotativa'yı kullanın
        //    //var pdfBytes = pdfView.BuildPdf(ControllerContext);

        //    //// PDF dosyasını kullanıcıya sunmak için FileResult döndürün
        //    //return File(pdfBytes, "application/pdf", "YourPDFFileName.pdf");



        //    var printpdf = new ActionAsPdf("Onay");
        //    //return printpdf;
        //    return RedirectToAction("Urunlerim");
        //}

        //ÜRÜNLERİN ALINDIĞINI ONAYLAMA SAYFASI İÇİN-----------------------Product/Onay/id--url
        public IActionResult Onay(int id,int teslimalanid)
        {
            var tutanak = _context.Tutanaklar.Where(x=>x.Id == id && x.TeslimAlanId == teslimalanid).ToList();
            return View(tutanak);
        }
        [HttpPost]
        public IActionResult Onay(IFormCollection col, Tutanak tutanakMail2) 
        {
            tutanakMail2 =_context.Tutanaklar.Where(x=>x.Id==tutanakMail2.Id ).FirstOrDefault();//burada teslim edilen id sini alıp işlem yaptım
            if (ModelState.IsValid)
            {
                SendEmail2(tutanakMail2);
                var product = _context.Tutanaklar.Find(tutanakMail2.Id);
                product.MailOnayDurumu = true;
                product.OnaylamaTarihi= DateTime.Now;
                _context.SaveChanges();
            }
            TempData["status2"] = "Form onaylandı.";
            return RedirectToAction("Urunlerim");
        }
        //ÜRÜNLERİN LİSTELENMESİ İÇİN
        public IActionResult Urunler(string p)
        {
            var tutanaklar = from d in _context.Tutanaklar select d;
            if (!string.IsNullOrEmpty(p))
            {
                tutanaklar = _context.Tutanaklar.Where(x => x.TeslimAlanAd.Contains(p) || x.Diger.Contains(p)||x.IsYeri.Contains(p)||x.Malzeme.Contains(p));
            }
            return View(tutanaklar.ToList());
            //var tutanaklar = _context.Tutanaklar.ToList();
            //return View(tutanaklar);
        }
        [HttpPost]
        public IActionResult Urunler(Tutanak tutanakMail3)  
        {
            return RedirectToAction("Index");
        }
        //KENDİ ÜRÜNLERİNİN LİSTELENMESİ İÇİN
        public IActionResult Urunlerim(int id) 
        {
            var tutanaklar = _context.Tutanaklar.Where(x=>x.TeslimAlanId == Convert.ToInt32(Request.Cookies["userid"])).ToList();
            return View(tutanaklar);
        }
        [HttpPost]
        public IActionResult Urunlerim(Tutanak tutanakMail3)
        {
            return RedirectToAction("Index");
        }
    }
}
