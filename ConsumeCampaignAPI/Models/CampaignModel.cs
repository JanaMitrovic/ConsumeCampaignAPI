using System.ComponentModel.DataAnnotations;

namespace ConsumeCampaignAPI.Models
{
    public class CampaignModel
    {
        public String Company { get; set; }
        public String CampaignName { get; set; }
        public DateTime StartDate { get; set; }
    }
}
