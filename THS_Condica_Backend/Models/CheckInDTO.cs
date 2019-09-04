using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace THS_Condica_Backend.Models
{
    public class CheckInDTO
    {
   
        public int ID { get; set; }
        public string FirstEntry { get; set; }
        public string SecondEntry { get; set; }

        [JsonConverter(typeof(IsoDateConverter))]
        public DateTime Day { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }



    }

   
}
