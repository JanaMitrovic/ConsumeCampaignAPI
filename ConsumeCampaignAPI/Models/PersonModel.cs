using System.Net;
using System;

namespace ConsumeCampaignAPI.Models
{
    public class PersonModel
    {
        public string Name { get; set; }
        public string SSN { get; set; }
        public DateTime? DOB { get; set; }
        public AddressModel Home { get; set; }
        public AddressModel Office { get; set; }
        public PersonModel Spouse { get; set; }
        public string[] FavoriteColors { get; set; }
        public long? Age { get; set; }
    }
}
