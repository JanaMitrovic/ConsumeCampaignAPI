using System.ComponentModel.DataAnnotations;

namespace ConsumeCampaignAPI.Models
{
    public class PurchaseModel
    {
        public int AgentId { get; set; }
        public int CustomerId { get; set; }
        public int CampaignId { get; set; }
        public int Price { get; set; }
        public int Discount { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}
