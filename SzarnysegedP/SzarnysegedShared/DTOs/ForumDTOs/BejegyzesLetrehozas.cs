using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SzarnysegedShared.DTOs.ForumDTOs
{
    public class BejegyzesLetrehozasDto
    {
        public string? Cim { get; set; }
        public string? Tartalom { get; set; }
        public string? KepUrl { get; set; }
        public int? SpotID { get; set; }
    }
}
