namespace ErnyosKozoApi.Models
{
    public class Utvonal
    {
        public int UtvonalID { get; set; }
        public int FelhasznaloID { get; set; }
        public int SpotID { get; set; }
        public DateTime? IndulasIdo { get; set; }
        public DateTime? ErkezesIdo { get; set; }
        public double? TavolsagKM { get; set; }
        public string Megjegyzes { get; set; }
    }

}
