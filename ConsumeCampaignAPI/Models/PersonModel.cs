using System.Net;
using System;

namespace ConsumeCampaignAPI.Models
{
    public class PersonModel
    {
        public string Name { get; set; }
        public string SSN { get; set; }
        public DateTime? DOB { get; set; }
        public long? Age { get; set; }
    }
}
