using System.Drawing;
using System.Globalization;
using AspWebProgram.Models;
//using AspWebProgramming.Data;
using AspWebProgramming.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Controllers
{
    public class RandevuController : Controller
    {
        private DataContext dbcontext;
        public RandevuController(DataContext context)
        {
            dbcontext = context;
        }
        public async Task<IActionResult> IndexRandevu()
        {
            var randevular = await dbcontext.Randevular
            .Include(x => x.Doktor)
            .Include(x => x.Hasta)
            .ToListAsync();
            return View(randevular);
        }
        [HttpGet]
        public async Task<IActionResult> RandevuOlustur()
        {
            ViewBag.Hastalar = new SelectList(await dbcontext.Hastalar.ToListAsync(), "HastaId", "HastaAd");
            ViewBag.Doktorlar = new SelectList(await dbcontext.Doktorlar.ToListAsync(), "DoktorId", "DoktorAdSoyad");

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetRandevuSaatleriJson(DateTime secilenTarih)
        {
            var saatler = await GetRandevuSaatleri(secilenTarih);
            return Json(new SelectList(saatler, "Value", "Text"));
        }
        private async Task<List<SelectListItem>> GetRandevuSaatleri(DateTime randevuTarihi)
        {

            var alinanSaatler = await dbcontext.Randevular
                .Where(r => r.RandevuTarih.Date == randevuTarihi.Date)
                .Select(r => r.RandevuSaati)
                .ToListAsync();

            var saatler = new List<SelectListItem>();
            var baslangic = new TimeSpan(9, 0, 0);
            var bitis = new TimeSpan(17, 0, 0);
            var aralik = TimeSpan.FromMinutes(30);

            for (var saat = baslangic; saat < bitis; saat += aralik)
            {
                if (!alinanSaatler.Contains(saat))
                {
                    saatler.Add(new SelectListItem
                    {
                        Value = saat.ToString(),
                        Text = saat.ToString(@"hh\:mm")
                    });
                }
            }

            return saatler;
        }

        // private async Task<List<SelectListItem>> GetRandevuSaatleri(DateTime? secilenTarih)
        // {
        //     var mevcutRandevular = new HashSet<TimeSpan>();

        //     if (secilenTarih.HasValue)
        //     {
        //         var randevuTarihleri = await dbcontext.Randevular
        //             .Where(r => r.RandevuTarih.Date == secilenTarih.Value.Date)
        //             .Select(r => r.RandevuTarih.TimeOfDay)
        //             .ToListAsync();

        //         mevcutRandevular = new HashSet<TimeSpan>(randevuTarihleri);
        //     }

        //     var saatler = new List<SelectListItem>();
        //     var baslangic = new TimeSpan(9, 0, 0); // Sabah 9:00
        //     var bitis = new TimeSpan(17, 0, 0);    // Akşam 5:00

        //     for (var saat = baslangic; saat < bitis; saat = saat.Add(TimeSpan.FromMinutes(30)))
        //     {
        //         if (!mevcutRandevular.Contains(saat))
        //         {
        //             saatler.Add(new SelectListItem { Value = saat.ToString(), Text = saat.ToString(@"hh\:mm") });
        //         }
        //     }

        //     return saatler;
        // }
        [HttpPost]
        public async Task<IActionResult> RandevuOlustur(Randevu model)
        {
            dbcontext.Randevular.Add(model);
            await dbcontext.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var randevu = dbcontext.Randevular.SingleOrDefault(r => r.RandevuId == id);
            if (randevu == null)
            {
                return NotFound();
            }

            var viewModel = new RandevuEditViewModel
            {
                RandevuId = randevu.RandevuId,
                HastaId = randevu.HastaId,
                DoktorId = randevu.DoktorId,
                RandevuTarih = randevu.RandevuTarih,
                RandevuSaati = randevu.RandevuSaati
            };

            ViewBag.Hastalar = new SelectList(dbcontext.Hastalar, "HastaId", "HastaAd", randevu.HastaId);
            ViewBag.Doktorlar = new SelectList(dbcontext.Doktorlar, "DoktorId", "DoktorAd", randevu.DoktorId);
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(int id, RandevuEditViewModel viewModel)
        {
            if (id != viewModel.RandevuId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var randevuToUpdate = dbcontext.Randevular.Find(id);
                if (randevuToUpdate == null)
                {
                    return NotFound();
                }

                randevuToUpdate.HastaId = viewModel.HastaId;
                randevuToUpdate.DoktorId = viewModel.DoktorId;
                randevuToUpdate.RandevuTarih = viewModel.RandevuTarih;
                randevuToUpdate.RandevuSaati = viewModel.RandevuSaati;

                dbcontext.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Hastalar = new SelectList(dbcontext.Hastalar, "HastaId", "HastaAd", viewModel.HastaId);
            ViewBag.Doktorlar = new SelectList(dbcontext.Doktorlar, "DoktorId", "DoktorAd", viewModel.DoktorId);

            return View(viewModel);
        }
        public IActionResult Delete(int id)
        {
            var randevu = dbcontext.Randevular.Find(id);
            if (randevu != null)
            {
                dbcontext.Randevular.Remove(randevu);
                dbcontext.SaveChanges();
            }
            return RedirectToAction("Index", "Home");
        }
    }



}