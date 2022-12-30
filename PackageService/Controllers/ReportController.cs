using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackageService.Data;
using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PackageService.Models;

namespace PackageService.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private ApplicationDbContext dbContext;

        public ReportController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber)
        {
            if (pageNumber.HasValue && pageNumber.Value < 1) pageNumber = 1;
            if (sortOrder == null) sortOrder = "id_desc";
            ViewData["CurrentSort"] = sortOrder;
            ViewData["VehicleSortParm"] = sortOrder == "vehicle" ? "vehicle_desc" : "vehicle";
            ViewData["AddressSortParm"] = sortOrder == "address" ? "address_desc" : "address";
            ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["IdSortParm"] = sortOrder == "Id" ? "id_desc" : "Id";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var reports = from s in dbContext.Reports.Include(s => s.Vehicle)
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                reports = reports.Where(s => s.Vehicle.Name.Contains(searchString)
                                       || s.Date.ToString().Contains(searchString)
                                       || s.Items.Contains(searchString)
                                       || s.Address.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "Id":
                    reports = reports.OrderBy(s => s.Id);
                    break;
                case "id_desc":
                    reports = reports.OrderByDescending(s => s.Id);
                    break;
                case "price":
                    reports = reports.OrderBy(s => s.Total);
                    break;
                case "price_desc":
                    reports = reports.OrderByDescending(s => s.Total);
                    break;
                case "address":
                    reports = reports.OrderBy(s => s.Address);
                    break;
                case "address_desc":
                    reports = reports.OrderByDescending(s => s.Address);
                    break;
                case "vehicle_desc":
                    reports = reports.OrderByDescending(s => s.Vehicle.Name);
                    break;
                case "Date":
                    reports = reports.OrderBy(s => s.Date);
                    break;
                case "date_desc":
                    reports = reports.OrderByDescending(s => s.Date);
                    break;
                default:
                    reports = reports.OrderBy(s => s.Vehicle.Name);
                    break;
            }
            double total = 0;
            foreach (Report report in reports)
            {
                total += report.Total;
            }
            ViewData["Total"] = total;
            int pageSize = 10;
            return View(await PaginatedList<Report>.CreateAsync(reports.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public ActionResult Delete(List<Report> reports, int[] ids)
        {
            if (ids.Length < 1)
            {
                ViewData["Error"] = "Hiç rapor seçmedin.";
                return View("Error");
            }
            else
            {
                List<Report> selected = reports.FindAll(report => ids.Contains(report.Id));
                if (selected.Count < 1)
                {
                    ViewData["Error"] = "Hiç rapor bulunamadı.";
                    return View("Error");
                }

                foreach (Report report in selected)
                {
                    dbContext.Remove(report);
                }
                dbContext.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        public IActionResult ZReport()
        {
            DateTime current = DateTime.Now;
            IEnumerable<Report> list = dbContext.Reports.ToList().Where(i => i.Date.Date.Equals(current.Date));
            return View("ZReport", list);
        }
    }
}
