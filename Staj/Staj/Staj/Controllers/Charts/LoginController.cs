using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Staj.Data;
using Staj.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Staj.Controllers.Charts
{
   
    public class LoginController : Controller
    {
        private AppDbContext _context2;
        public LoginController(AppDbContext context2)
        {
            _context2 = context2;
        }
        public IActionResult Index()
        {
            var kullanıcılar = _context2.Logins.ToList();
            return View(kullanıcılar);
        }
        public IActionResult Add() //KAYIT EKLEMEK İŞLEMİ YAPAR
        {
            return View();
        }
        [HttpPost]
        public IActionResult Add(IFormCollection col)
        {
            Login login = new Login();

            login.Adı = col["Adı"].ToString();
            login.Sifre = col["Sifre"].ToString();
            login.Rol = "B";
            ////DB YE MANUEL GİRİLECEK
            //login.Departman = _context2.Tutanaklar.Where(x => x.TeslimAlanId == Convert.ToInt32(col["Id"])).Select(x => x.TeslimAlanDep).FirstOrDefault().ToString();
            //login.Gorev = _context2.Tutanaklar.Where(x => x.TeslimAlanId == Convert.ToInt32(col["Id"])).Select(x => x.TeslimAlanGorev).FirstOrDefault().ToString();
            //if (login.Departman.Equals(null))
            //{
            //    login.Departman = "null";
            //}else if (login.Gorev.Equals(null))
            //{
            //    login.Gorev = "null";
            //}
            if (!_context2.Logins.Where(x => x.Adı == login.Adı).Any())//aynı isimden kayıt oluşturmuyor
            {
                _context2.Logins.Add(login);
                _context2.SaveChanges();
                return RedirectToAction("Giris");
            }
            else
            {
                return RedirectToAction("Add");
            }
        }
        public IActionResult Giris() //GİRİŞ YAPAR
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Giris(Login log,string ReturnUrl)  
        {
            if (_context2.Logins.Any(l => l.Adı == log.Adı && l.Sifre == log.Sifre))
            {
                string istenenDeger = _context2.Logins //giriş yapan kullanıcının rolünü istenenDeger attım
                    .Where(r => r.Adı == log.Adı && r.Sifre == log.Sifre)
                    .Select(r => r.Rol)
                    .FirstOrDefault(); // İlk eşleşen değeri alın

                string sonKullaniciId = _context2.Logins //giriş yapan kullanıcının Id sinisonKullaniciId attım
                    .Where(r => r.Adı == log.Adı && r.Sifre == log.Sifre)
                    .Select(r => r.Id)
                    .FirstOrDefault().ToString(); 

                var claims = new List<Claim>
                {
                       new Claim(ClaimTypes.Name, log.Adı),
                       new Claim(ClaimTypes.Role, istenenDeger),
                       new Claim(ClaimTypes.Sid, sonKullaniciId)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                // Çerez oluşturulup ve eklendi
                Response.Cookies.Append("username", claims[0].Value);
                Response.Cookies.Append("userid", claims[2].Value);
                Response.Cookies.Append("userrole", claims[1].Value);
                TempData["bir"] = "Giriş Başarılı";
                if (!string.IsNullOrEmpty(ReturnUrl)) //ONAY İCİN KONTROL YAPILACAK:: TeslimAlanId İLE SİSTEMDEKİ KİŞİ ID Sİ KARŞILAŞTIRILACAK
                {
                    return Redirect(ReturnUrl);
                }
                else
                {
                    return RedirectToAction("Urunler", "Products");
                }
            }
            else
            {
                TempData["iki"] = "Hatalı Giriş, Tekrar Deneyiniz ";
                return RedirectToAction("Giris", "Login");
            }
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            Response.Cookies.Delete("username");
            Response.Cookies.Delete("userid");
            Response.Cookies.Delete("userrole");
            return RedirectToAction("Giris", "Login");
        }
    }
}
