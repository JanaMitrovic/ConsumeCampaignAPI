using System;

namespace ConsumeCampaignAPI.Models
{
    public class CsvResponseModel
    {
        public int Id { get; set; }
        public string AgentName { get; set; }
        public string AgentSurname { get; set; }
        public string AgentEmail { get; set; }
        public string CampaignName { get; set; }
        public int Price { get; set; }
        public int Discount { get; set; }
        public int PriceWithDiscount { get; set; }
        public DateTime PurchaseDate { get; set; }
        public PersonModel Customer { get; set; }
    }
}
