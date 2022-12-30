using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackageService.Data;
using PackageService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;

namespace PackageService.Controllers
{
    [Authorize]
    public class ItemController : Controller
    {
        private ApplicationDbContext dbContext = null;

        public ItemController(ApplicationDbContext dbContext)
        {
            //To fix double issue.
            CultureInfo culture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
			
            this.dbContext = dbContext;
        }

        public async Task<IActionResult> Index(int? pageNumber)
        {
            if (pageNumber.HasValue && pageNumber.Value < 1) pageNumber = 1;

            var reports = from s in dbContext.Items
                          select s;
            int pageSize = 5;
            return View(await PaginatedList<Item>.CreateAsync(reports.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Item item)
        {
            if (ModelState.IsValid)
            {
                dbContext.Add(item);
                dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            Item item = dbContext.Items.Single(x => x.Id == id);
            return View(item);
        }

        [HttpPost]
        public ActionResult Edit(Item item)
        {
            if (ModelState.IsValid)
            {
                dbContext.Entry(item).State = EntityState.Modified;
                dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            Item item = dbContext.Items.Single(x => x.Id == id);
            return View(item);
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Item item = dbContext.Items.Single(x => x.Id == id);
            dbContext.Items.Remove(item);
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
