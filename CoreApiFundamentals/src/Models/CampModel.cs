using CoreCodeCamp.Data;
using System;
using System.Collections.Generic;

namespace CoreCodeCamp.Models
{
    public class CampModel
    {
        public string Name { set; get; }
        public string Moniker { set; get; }
        public DateTime EventDate { set; get; } = DateTime.MinValue;
        public int Length { set; get; } = 1;

        public string Venue { get; set; }
        public string LocationAddress1 { get; set; }
        public string LocationAddress2 { get; set; }
        public string LocationAddress3 { get; set; }
        public string LocationCityTown { get; set; }
        public string LocationStateProvince { get; set; }
        public string LocationPostalCode { get; set; }
        public string LocationCountry { get; set; }

        public ICollection<TalkModel> Talks { get; set; }
    }
}
