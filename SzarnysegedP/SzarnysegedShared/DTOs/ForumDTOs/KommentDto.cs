using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SzarnysegedShared.DTOs.ForumDTOs
{
    public class KommentDto
    {
        public int KommentID { get; set; }
        public int BejegyzesID { get; set; }
        public int? SzuloKommentID { get; set; }

        public int FelhasznaloID { get; set; }
        public string? FelhasznaloNev { get; set; }
        public string? TeljesNev { get; set; }
        public string? AvatarUrl { get; set; }

        public string? Tartalom { get; set; }
        public DateTime Letrehozva { get; set; }

        public List<KommentDto> Valaszok { get; set; } = new();
    }
}
