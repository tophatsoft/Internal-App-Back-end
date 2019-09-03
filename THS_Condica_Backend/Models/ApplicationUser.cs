using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace THS_Condica_Backend.Models
{
    public class ApplicationUser: IdentityUser
    {
        [Column(TypeName = "nvarchar(150)")]
        public string FirstName { get; set; }

        [Column(TypeName = "nvarchar(150)")]
        public string LastName { get; set; }

        [Column(TypeName = "date")]
        public DateTime RegisteredDate { get; set; }

        [Column(TypeName = "nvarchar(150)")]
        public string Department { get; set; }

        [Column(TypeName = "nvarchar(150)")]
        public string Position { get; set; }

        [Column(TypeName = "bit")]
        public bool PassChanged { get; set; }

    }
}
