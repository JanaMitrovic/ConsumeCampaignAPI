using ConsumeCampaignAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;

namespace ConsumeCampaignAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        string baseURL = "http://localhost:5163";
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<ActionResult<string>> createPurchase(PurchaseModel purchase)
        {
            PurchaseModel purchaseObject = new PurchaseModel()
            {
                AgentId = purchase.AgentId,
                CustomerId = purchase.CustomerId,
                CampaignId = purchase.CampaignId,
                Price = purchase.Price,
                Discount = purchase.Discount,
                PurchaseDate = purchase.PurchaseDate
            };

            if(purchase.AgentId != null && purchase.CustomerId != null && purchase.CampaignId != null && purchase.Price != 0 && purchase.Discount != 0 && purchase.PurchaseDate != null)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseURL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage getData = await client.PostAsJsonAsync<PurchaseModel>("createPurchase", purchaseObject);

                    if (getData.IsSuccessStatusCode)
                    {
                        return RedirectToAction("createPurchase");
                    }
                    else if (getData.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var responseContent = await getData.Content.ReadAsStringAsync();
                        dynamic responseObject = JsonConvert.DeserializeObject(responseContent);
                        string errorMessage = responseObject.CustomError[0].ToString();

                        ModelState.AddModelError(string.Empty, errorMessage);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred while communicating with the API.");
                    }
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}