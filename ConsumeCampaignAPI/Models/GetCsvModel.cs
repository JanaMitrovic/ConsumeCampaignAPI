using System.ComponentModel.DataAnnotations;

namespace ConsumeCampaignAPI.Models
{
    public class GetCsvModel
    {
        public int CampaignId { get; set; }

        public DateTime CurrentDate { get; set; }
    }
}
