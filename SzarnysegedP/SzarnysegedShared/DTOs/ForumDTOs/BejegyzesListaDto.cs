using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SzarnysegedShared.DTOs.ForumDTOs
{
    public class BejegyzesListaDto
    {
        public int BejegyzesID { get; set; }
        public string? Cim { get; set; }
        public string? Tartalom { get; set; }
        public string? KepUrl { get; set; }
        public DateTime Letrehozva { get; set; }

        public int FelhasznaloID { get; set; }
        public string? FelhasznaloNev { get; set; }
        public string? TeljesNev { get; set; }
        public string? AvatarUrl { get; set; }

        public int? SpotID { get; set; }
        public string? SpotNev { get; set; }
        public string? SpotSlug { get; set; }

        public int KommentekSzama { get; set; }
    }
}
