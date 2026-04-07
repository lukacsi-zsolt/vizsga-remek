namespace ErnyosKozoApi.Models
{
    // ===== SPOT ENTITÁS (REPÜLŐS HELYSZÍN) =====
    // Az adatbázis "Spotok" táblájának C# leképezése
    // Egy spot egy siklóernyős/sárkányrepülős felszállóhelyet reprezentál
    public class Spot
    {
        // ===== ELSŐDLEGES KULCS =====
        public int SpotID { get; set; }

        // ===== AZONOSÍTÁSI MEZŐK =====
        public string? Nev { get; set; }    // A helyszín neve (pl. "Kékestető")
        public string? Slug { get; set; }   // URL-barát azonosító (pl. "kekesteto")
                                            // A SpotokController.Slugify() metódus generálja
                                            // A frontend szép URL-eket épít belőle: /spotok/kekesteto
        // ===== FÖLDRAJZI ADATOK =====
        public string? Orszag { get; set; } // Melyik országban van (pl. "Magyarország")
        public string? Megye { get; set; }  // Megye/régió (pl. "Heves")
        public string? HelyLeiras { get; set; } // Szöveges leírás a helyszín megközelítéséről

        // ===== REPÜLÉSI ADATOK =====
        // int? és double? (nullable numerikus típusok) → opcionális, nem minden spothoz van adat
        public int? Magassag { get; set; }  // Tengerszint feletti magasság méterben
        public double? AtlagSzel { get; set; } // Átlagos szélsebesség (km/h vagy m/s)

        // ===== SZABÁLYOK =====
        public string? Szabalyok { get; set; } // Helyi repülési szabályok, korlátozások szövegesen

        // ===== GPS KOORDINÁTÁK =====
        // double? → lebegőpontos szám, térkép megjelenítéshez (pl. Google Maps, Leaflet)
        // A SpotokController validálja, hogy létrehozáskor kötelezőek legyenek
        public double? Lat { get; set; }    // Latitude – földrajzi szélesség (pl. 47.8745)
        public double? Lon { get; set; }    // Longitude – földrajzi hosszúság (pl. 20.0088)

        // ===== LÉTREHOZÓ FELHASZNÁLÓ (OPCIONÁLIS FK) =====
        // int? (nullable) → nem kötelező, mert:
        //   - Admin közvetlenül is létrehozhat spotot (lehet null)
        //   - Ha a létrehozó felhasználó törlődik, ez null-ra áll
        //     (az AdminController.DeleteUser() explicit nullázza)
        // MEGJEGYZÉS: nincs navigációs property (Felhasznalo? Letrehozo)
        //   → ha kellene a létrehozó adatai, explicit Join-nal lehetne lekérdezni

        public int? LetrehozoFelhasznaloID { get; set; }
    }
}