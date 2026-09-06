using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlEcommerce")]
    public class ctlEcommerce : Controller
    {
        public class PlaceOrderRequest
        {
            public string Slug { get; set; }
            public string CustomerName { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
            public string Notes { get; set; }
            public List<PlaceOrderLine> Lines { get; set; }
        }

        public class PlaceOrderLine
        {
            public string ItemGuid { get; set; }
            public decimal Qty { get; set; }
            public string Size { get; set; }
            public string Color { get; set; }
            public string LineNote { get; set; }
        }

        [HttpGet]
        [Route("ShopBySlug")]
        public string ShopBySlug(string slug)
        {
            try
            {
                clsEcommerce svc = new clsEcommerce();
                var shop = svc.GetShopBySlug(slug);
                if (shop == null)
                {
                    return JsonConvert.SerializeObject(new { Ok = false, Error = "Shop not found" });
                }

                string logoB64 = shop.Logo != null && shop.Logo.Length > 0
                    ? Convert.ToBase64String(shop.Logo)
                    : "";

                return JsonConvert.SerializeObject(new
                {
                    Ok = true,
                    CompanyID = shop.CompanyID,
                    TradeName = shop.TradeName,
                    AName = shop.AName,
                    EName = shop.EName,
                    WebSlug = shop.WebSlug,
                    Email = shop.Email,
                    Address = shop.Address,
                    Tel1 = shop.Tel1,
                    Tel2 = shop.Tel2,
                    ContactPerson = shop.ContactPerson,
                    Logo = logoB64,
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Ok = false, Error = ex.Message });
            }
        }

        [HttpGet]
        [Route("Catalog")]
        public string Catalog(string slug)
        {
            try
            {
                clsEcommerce svc = new clsEcommerce();
                var shop = svc.GetShopBySlug(slug);
                if (shop == null)
                {
                    return JsonConvert.SerializeObject(new { Ok = false, Error = "Shop not found", Items = Array.Empty<object>() });
                }

                DataTable dt = svc.GetCatalog(shop.CompanyID);
                return JsonConvert.SerializeObject(new { Ok = true, Items = dt ?? new DataTable() });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Ok = false, Error = ex.Message, Items = Array.Empty<object>() });
            }
        }

        [HttpPost]
        [Route("PlaceOrder")]
        public string PlaceOrder([FromBody] PlaceOrderRequest body)
        {
            try
            {
                if (body == null)
                    return JsonConvert.SerializeObject(new { Ok = false, Error = "Invalid request" });

                clsEcommerce svc = new clsEcommerce();
                var lines = (body.Lines ?? new List<PlaceOrderLine>())
                    .Select(l => new clsEcommerce.OrderLineInput
                    {
                        ItemGuid = l?.ItemGuid ?? "",
                        Qty = l?.Qty ?? 0,
                        Size = l?.Size ?? "",
                        Color = l?.Color ?? "",
                        LineNote = l?.LineNote ?? "",
                    })
                    .ToList();

                var result = svc.PlaceOrder(
                    body.Slug,
                    body.CustomerName,
                    body.Phone,
                    body.Address,
                    body.Notes,
                    lines);

                try
                {
                    var configuration = HttpContext?.RequestServices?.GetService(typeof(IConfiguration)) as IConfiguration;
                    var environment = HttpContext?.RequestServices?.GetService(typeof(IHostEnvironment)) as IHostEnvironment;
                    var loggerFactory = HttpContext?.RequestServices?.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                    var logger = loggerFactory?.CreateLogger("Ecommerce");

                    var shop = svc.GetShopBySlug(body.Slug);
                    DataTable orderDt = svc.SelectOrderById(shop?.CompanyID ?? 0, result.OrderId);
                    decimal total = 0;
                    if (orderDt != null && orderDt.Rows.Count > 0)
                        total = Simulate.decimal_(orderDt.Rows[0]["Total"]);

                    clsEcommerceEmailSender.TrySendOrderNotification(
                        configuration,
                        environment,
                        logger,
                        result.CompanyEmail,
                        result.ShopName,
                        result.OrderNo,
                        body.CustomerName,
                        body.Phone,
                        body.Address ?? "",
                        total);
                }
                catch { /* email is best-effort */ }

                return JsonConvert.SerializeObject(new
                {
                    Ok = true,
                    OrderNo = result.OrderNo,
                    OrderId = result.OrderId,
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Ok = false, Error = ex.Message });
            }
        }

        [HttpGet]
        [Route("Orders")]
        public string Orders(int CompanyID, string Status = "")
        {
            try
            {
                clsEcommerce svc = new clsEcommerce();
                DataTable dt = svc.SelectOrders(CompanyID, Status ?? "");
                return JsonConvert.SerializeObject(dt ?? new DataTable());
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Error = ex.Message });
            }
        }

        [HttpGet]
        [Route("OrderById")]
        public string OrderById(int CompanyID, int OrderID)
        {
            try
            {
                clsEcommerce svc = new clsEcommerce();
                DataTable order = svc.SelectOrderById(CompanyID, OrderID);
                DataTable lines = svc.SelectOrderLines(CompanyID, OrderID);
                return JsonConvert.SerializeObject(new
                {
                    Order = order,
                    Lines = lines,
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Error = ex.Message });
            }
        }

        [HttpPost]
        [Route("MarkOrderSeen")]
        public string MarkOrderSeen(int CompanyID, int OrderID)
        {
            try
            {
                clsEcommerce svc = new clsEcommerce();
                int n = svc.MarkOrderSeen(CompanyID, OrderID);
                return JsonConvert.SerializeObject(new { Ok = n > 0 });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Ok = false, Error = ex.Message });
            }
        }

        [HttpPost]
        [Route("SetOrderProgressed")]
        public string SetOrderProgressed(int CompanyID, int OrderID, bool Progressed = true)
        {
            try
            {
                clsEcommerce svc = new clsEcommerce();
                int n = svc.SetOrderProgressed(CompanyID, OrderID, Progressed);
                return JsonConvert.SerializeObject(new { Ok = n > 0, Progressed });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Ok = false, Error = ex.Message });
            }
        }
    }
}
