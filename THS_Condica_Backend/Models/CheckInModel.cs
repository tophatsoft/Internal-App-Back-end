using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace THS_Condica_Backend.Models
{
    public class CheckInModel
    {   [Key]
        public int ID { get; set; }
        [Column(TypeName = "date")]
        [JsonConverter(typeof(IsoDateConverter))]
        public DateTime Day { get; set; }
        public string FirstEntry { get; set; }
        public string SecondEntry { get; set; }
        [ForeignKey("OwnerId")]
       
        public string OwnerId { get; set; }

    }

    public class IsoDateConverter : IsoDateTimeConverter
    {
        public IsoDateConverter() =>
            this.DateTimeFormat = Culture.DateTimeFormat.ShortDatePattern;
    }
}
