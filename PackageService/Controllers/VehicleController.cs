using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackageService.Data;
using Microsoft.EntityFrameworkCore;
using PackageService.Models;
using System.Text;
using Telegram.Bot;

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
            List<VehicleItems> vehicleItems = new List<VehicleItems>();
            IQueryable<VehicleItems> vehicleItemsFiltered = dbContext.VehicleItems.Include(x => x.Item).Where(x => x.VehicleId == vehicle.Id);
            if (vehicle.Items != null)
            {
                ViewData["Items"] = vehicle.Items.Split("|")[0];
                vehicleItems.AddRange(vehicleItemsFiltered);

                List<Item> items = dbContext.Items.ToList();
                foreach (Item item in items)
                {
                    if (vehicleItemsFiltered.Any(vi => vi.ItemId == item.Id))
                        continue;
                    VehicleItems vehicleItem = new VehicleItems()
                    {
                        VehicleId = vehicle.Id,
                        ItemId = item.Id
                    };
                    vehicleItems.Add(vehicleItem);
                    dbContext.Add(vehicleItem);
                }
                dbContext.SaveChanges();
            } else
            {
                foreach (VehicleItems vi in vehicleItemsFiltered)
                {
                    dbContext.Remove(vi);
                }
                List<Item> items = dbContext.Items.ToList();
                foreach (Item item in items)
                {
                    VehicleItems vehicleItem = new VehicleItems()
                    {
                        VehicleId = vehicle.Id,
                        ItemId = item.Id
                    };
                    vehicleItems.Add(vehicleItem);
                    dbContext.Add(vehicleItem);
                }
                dbContext.SaveChanges();
            }
            return View(vehicleItems);
        }

        [HttpPost]
        public ActionResult LoadItem(List<VehicleItems> model, int[] ids, String address)
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
                    IQueryable<VehicleItems> vehicleItems = dbContext.VehicleItems.Include(x => x.Item).Where(x => x.VehicleId == vehicleId);
                    foreach (VehicleItems vehicleItem in vehicleItems)
                    {
                        int operationAmount = ids.Contains(vehicleItem.Id) ? model.Find(item => item.Id == vehicleItem.Id).Amount : 0;
                        operationAmount -= vehicleItem.Amount;
                        vehicleItem.Item.Stock -= operationAmount;
                        dbContext.Entry(vehicleItem.Item).State = EntityState.Modified;
                        dbContext.VehicleItems.Remove(vehicleItem);
                    }
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
                double total = 0;
                IDictionary<int, int> operationAmounts = new Dictionary<int, int>();
                IQueryable<VehicleItems> vehicleItems = dbContext.VehicleItems.Where(x => x.VehicleId == vehicleId);
                foreach (VehicleItems vehicleItem in vehicleItems)
                {
                    int operationAmount = ids.Contains(vehicleItem.Id) ? model.Find(item => item.Id == vehicleItem.Id).Amount : 0;
                    operationAmount -= vehicleItem.Amount;
                    operationAmounts.Add(vehicleItem.Id, operationAmount);
                }

                List<VehicleItems> selected = model.FindAll(model => ids.Contains(model.Id));

                foreach (VehicleItems vehicleItem in model)
                {
                    var local = dbContext.Set<VehicleItems>().Local.FirstOrDefault(entry => entry.Id.Equals(vehicleItem.Id));
                    if (local != null)
                    {
                        dbContext.Entry(local).State = EntityState.Detached;
                    }

                    Item item = dbContext.Items.Single(x => x.Id == vehicleItem.ItemId);
                    item.Stock -= operationAmounts[vehicleItem.Id];
                    if (ids.Contains(vehicleItem.Id))
                    {
                        if (vehicleItem.Amount < 1)
                        {
                            ViewData["Error"] = "Adet 1'den küçük olamaz.";
                            return View("Error");
                        }
                        else if (item.Stock < 0)
                        {
                            ViewData["Error"] = "Adet stok miktarından büyük olamaz.";
                            return View("Error");
                        }
                        total += vehicleItem.Amount * item.Price;

                        dbContext.Entry(vehicleItem).State = EntityState.Modified;
                        dbContext.Entry(item).State = EntityState.Modified;
                    }
                    else
                    {
                        vehicleItem.Vehicle = null;
                        vehicleItem.Item = null;
                        dbContext.VehicleItems.Remove(vehicleItem);
                    }
                    dbContext.SaveChanges();
                }


                string items = Join(selected, " - ", " adet ");

                vehicle.Items = items+"|"+total;
                vehicle.Address = address;

                dbContext.Entry(vehicle).State = EntityState.Modified;
                dbContext.SaveChanges();

                if(vehicle.Token != null && vehicle.ChatId != null)
                {
                    sendAlert(vehicle.Token, vehicle.ChatId.Value, "'" + vehicle.Address + "' adresine '" + items + "' eşyaları sipariş girilmiştir.");
                }

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

        public async void sendAlert(string token, long chatId, string message)
        {
            var botClient = new TelegramBotClient(token);

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: message);
        }

        public String Join(List<VehicleItems> selected, string delimiter, string amount)
        {
            StringBuilder result = new StringBuilder();
            using (IEnumerator<VehicleItems> en = selected.GetEnumerator())
            {
                if (!en.MoveNext())
                {
                    result.Append("Boş");
                }
                if (en.Current != null)
                {
                    result.Append(en.Current.Amount + amount + en.Current.Item.Name);
                }

                while (en.MoveNext())
                {
                    result.Append(delimiter);
                    if (en.Current != null)
                    {
                        result.Append(en.Current.Amount + amount + en.Current.Item.Name);
                    }
                }
            }
            return result.ToString();
        }

    }
}
