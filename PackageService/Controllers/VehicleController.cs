using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PackageService.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using PackageService.Models;
using System.Diagnostics.Contracts;
using System.Text;
using Telegram.Bot;
using System.Threading;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

namespace PackageService.Controllers
{
    [Authorize]
    public class VehicleController : Controller
    {
        private ApplicationDbContext dbContext;

        public VehicleController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            List<Vehicle> vehicles = dbContext.Vehicles.ToList();
            return View(vehicles);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                dbContext.Add(vehicle);
                dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vehicle);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            Vehicle vehicle = dbContext.Vehicles.Single(x => x.Id == id);
            return View(vehicle);
        }

        [HttpPost]
        public ActionResult Edit(Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                dbContext.Entry(vehicle).State = EntityState.Modified;
                dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vehicle);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            Vehicle vehicle = dbContext.Vehicles.Single(x => x.Id == id);
            return View(vehicle);
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Vehicle vehicle = dbContext.Vehicles.Single(x => x.Id == id);
            dbContext.Vehicles.Remove(vehicle);
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult LoadItem(int id)
        {
            Vehicle vehicle = dbContext.Vehicles.Single(x => x.Id == id);
            ViewData["Name"] = vehicle.Name;
            ViewData["Address"] = vehicle.Address;
            TempData["Name"] = vehicle.Name;
            TempData["Id"] = id;
            TempData["VehicleId"] = id;
            List<Item> items = dbContext.Items.ToList();
            if(vehicle.Items != null)
            {
                ViewData["Items"] = vehicle.Items.Split("|")[0];
            }
            return View(items);
        }

        [HttpPost]
        public ActionResult LoadItem(List<Item> model, int[] ids, String address)
        {

            int vehicleId = (int)TempData["VehicleId"];
            Vehicle vehicle = dbContext.Vehicles.Single(x => x.Id == vehicleId);

            if (ids.Length < 1)
            {
                if(vehicle.Items == null)
                {
                    ViewData["Error"] = "Hiç ürün seçmedin.";
                    return View("Error");
                }
                else
                {
                    vehicle.Items = null;
                    vehicle.Address = null;
                    dbContext.Entry(vehicle).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    return View("Complete", null);
                }
            }
            else if(String.IsNullOrEmpty(address))
            {
                    ViewData["Error"] = "Bir adres girmelisin.";
                    return View("Error");
            }
            else
            {
                List<Item> selected = model.FindAll(item => ids.Contains(item.Id));
                double total = 0;
                foreach(Item item in selected)
                {
                    if(item != null && item.Amount < 1)
                    {
                        ViewData["Error"] = "Adet 1'den küçük olamaz.";
                        return View("Error");
                    }
                    total += item.Amount * item.Price;
                }

                string items = Join(selected, " - ", " adet ");

                vehicle.Items = items+"|"+total;
                vehicle.Address = address;

                dbContext.Entry(vehicle).State = EntityState.Modified;
                dbContext.SaveChanges();

                sendAlert("'" + vehicle.Address + "' adresine '" + items + "' eşyaları sipariş girilmiştir.");

                return View("Complete", selected);
            }
        }

        public IActionResult SaveItem(int id)
        {
            Vehicle vehicle = dbContext.Vehicles.Single(x => x.Id == id);
            DateTime date = DateTime.Now;
            int hour = date.Minute;
            string hourAsString = hour.ToString().Length == 1 ? "0" + hour : hour.ToString();
            int minute = date.Minute;
            string minuteAsString = minute.ToString().Length == 1 ? "0" + minute : minute.ToString();
            string time = date.Hour + ":" + minuteAsString;

            string[] split = vehicle.Items.Split("|");

            double total = Double.Parse(split[1]);

            Report report = new Report()
            {
                Date = date,
                Time = time,
                VehicleId = vehicle.Id,
                Address = vehicle.Address,
                Items = split[0],
                Total = total
            };

            vehicle.Items = null;
            vehicle.Address = null;
            dbContext.Entry(vehicle).State = EntityState.Modified;
            dbContext.Add(report);
            dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public async void sendAlert(string message)
        {
            var botClient = new TelegramBotClient("YOUR_API_KEY");

            await botClient.SendTextMessageAsync(
                chatId: YOUR_CHAT_ID,
                text: message);
        }

        public String Join(List<Item> selected, string delimiter, string amount)
        {
            StringBuilder result = new StringBuilder();
            using (IEnumerator<Item> en = selected.GetEnumerator())
            {
                if (!en.MoveNext())
                {
                    result.Append("Boş");
                }
                if (en.Current != null)
                {
                    result.Append(en.Current.Amount + amount + en.Current.Name);
                }

                while (en.MoveNext())
                {
                    result.Append(delimiter);
                    if (en.Current != null)
                    {
                        result.Append(en.Current.Amount + amount + en.Current.Name);
                    }
                }
            }
            return result.ToString();
        }

    }
}
