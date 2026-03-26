using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SzarnysegedShared.DTOs.ForumDTOs
{
    public class KommentLetrehozasDto
    {
        public int BejegyzesID { get; set; }
        public int? SzuloKommentID { get; set; }
        public string? Tartalom { get; set; }
    }
}
