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

                        //Send POST requst to web api for starting campaign
                        HttpResponseMessage getData = await client.PostAsJsonAsync<CampaignModel>("startCampaign", campaign);

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

                        //Send POST request for creating purchase
                        HttpResponseMessage getData = await client.PostAsJsonAsync<PurchaseModel>("createPurchase", purchase);

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

                        //Send POST request for getting .csv campaign report
                        HttpResponseMessage getData = await client.PostAsJsonAsync<GetCsvModel>("getCsvReport", getCsv);

                        if (getData.IsSuccessStatusCode)
                        {
                            //Check if file is in .csv format
                            if (getData.Content.Headers.ContentType.MediaType == "text/csv")
                            {
                                //Get response content as stream
                                var stream = await getData.Content.ReadAsStreamAsync();
                                //Send file throw stream
                                return new FileStreamResult(stream, "text/csv")
                                {
                                    FileDownloadName = "successfulPurchases.csv"
                                };
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Unexpected response format. Expected CSV.");
                            }
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
                return View();
            }

            //Get token from Session
            string token = HttpContext.Session.GetString("Token");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                //Define getting .csv report from file field from the request
                var formData = new MultipartFormDataContent();
                formData.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

                //Send POST reqest for showing report data
                HttpResponseMessage response = await client.PostAsync("showReportData", formData);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var csvResponseList = JsonConvert.DeserializeObject<List<CsvResponseModel>>(responseContent);

                    //Pass response data to view
                    var viewModel = new CsvReportViewModel
                    {
                        CsvResponseList = csvResponseList
                    };

                    return View("showReportData", viewModel);

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