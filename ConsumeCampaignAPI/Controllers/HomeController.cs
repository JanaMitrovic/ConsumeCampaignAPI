using ConsumeCampaignAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public async Task<ActionResult> Login(LoginModel loginModel)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseURL + "/authorization/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Set login credentials in the request body
                HttpResponseMessage loginResponse = await client.PostAsJsonAsync("login", loginModel);

                if (loginResponse.IsSuccessStatusCode)
                {
                    //Get token data from the response
                    var responseContent = await loginResponse.Content.ReadAsStringAsync();
                    var responseObj = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                    if (responseObj != null)
                    {
                        //Get Token value
                        string tokenValue = responseObj.token;
                        //Save token value in the Session
                        HttpContext.Session.SetString("Token", tokenValue);
                        Console.WriteLine("login: " + tokenValue);

                        return RedirectToAction("createPurchase");
                    }
                }
                else if (loginResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError(string.Empty, "Login failed. Please check your credentials.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Login failed.");
                }
            }

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
                //Get token from Session
                string token = HttpContext.Session.GetString("Token");
                
                if (!string.IsNullOrEmpty(token))
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(baseURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
                            ModelState.AddModelError(string.Empty, "Error communicating to API!");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "There is no token in Session!");
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