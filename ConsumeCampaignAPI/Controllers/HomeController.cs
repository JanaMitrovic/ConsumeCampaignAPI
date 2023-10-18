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

                        return RedirectToAction("startCampaign");
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
        public async Task<ActionResult<string>> startCampaign(CampaignModel campaign)
        {
            CampaignModel campaignObject = new CampaignModel()
            {
                Company = campaign.Company,
                CampaignName = campaign.CampaignName,
                StartDate = campaign.StartDate
            };

            if (campaign.Company != null && campaign.CampaignName != null && campaign.StartDate != null)
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

                        HttpResponseMessage getData = await client.PostAsJsonAsync<CampaignModel>("startCampaign", campaignObject);

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
        public async Task<ActionResult<string>> getCsvReport(GetCsvModel getCsv)
        {
            GetCsvModel getCsvObject = new GetCsvModel()
            {
                CampaignId = getCsv.CampaignId,
                CurrentDate = getCsv.CurrentDate
            };

            if (getCsv.CampaignId != 0 && getCsv.CurrentDate != null)
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

                        HttpResponseMessage getData = await client.PostAsJsonAsync<GetCsvModel>("getCsvReport", getCsvObject);

                        if (getData.IsSuccessStatusCode)
                        {
                            // Check if the response is a file
                            if (getData.Content.Headers.ContentType.MediaType == "text/csv")
                            {
                                // Get the response content as a stream
                                var stream = await getData.Content.ReadAsStreamAsync();

                                // Create a FileStreamResult to return the CSV file
                                return new FileStreamResult(stream, "text/csv")
                                {
                                    FileDownloadName = "successfulPurchases.csv"
                                };
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Unexpected response format. Expected CSV.");
                            }
                            //return RedirectToAction("getCsvReport");
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

        public async Task<IActionResult> showReportData(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("CustomError", "File is missing or empty!");
                return View(); // Render the view with validation errors
            }

            string token = HttpContext.Session.GetString("Token");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var formData = new MultipartFormDataContent();
                formData.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

                // Make a POST request to the ShowReportData endpoint with the file
                HttpResponseMessage response = await client.PostAsync("showReportData", formData);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var csvResponseList = JsonConvert.DeserializeObject<List<CsvResponseModel>>(responseContent);

                    var viewModel = new CsvReportViewModel
                    {
                        CsvResponseList = csvResponseList
                    };

                    return View("showReportData", viewModel); // Render the view with response data


                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseObject = JsonConvert.DeserializeObject(responseContent);
                    string errorMessage = responseObject.CustomError[0].ToString();

                    ModelState.AddModelError(string.Empty, errorMessage);
                    return View();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error communicating with the API!");
                    return View();
                }
            }
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