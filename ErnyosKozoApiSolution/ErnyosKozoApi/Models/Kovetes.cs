namespace ErnyosKozoApi.Models
{
    public class Kovetes
    {
        public int KovetesID { get; set; }

        public int KovetoFelhasznaloID { get; set; }
        public int KovetettFelhasznaloID { get; set; }

        public DateTime Letrehozva { get; set; } = DateTime.UtcNow;
    }
}