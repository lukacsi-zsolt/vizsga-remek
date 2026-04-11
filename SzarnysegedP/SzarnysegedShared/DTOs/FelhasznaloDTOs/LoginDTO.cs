// LOGIN DTO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SzarnysegedShared.DTOs.FelhasznaloDTOs
{
    // A bejelentkezési kérés adatai
    // A kliens (Login.razor) ezt küldi az API-nak: POST api/auth/login
    // Az AuthController.Login() metódus fogadja
    public class LoginDTO
    {
        public string Username { get; set; }    // A felhasználó által megadott felhasználónév
        public string Password { get; set; }    // A nyers jelszó (NEM hash!) – HTTPS-en megy, biztonságos
                                                // Az API oldalon a PasswordHasher ellenőrzi a hash-sel
    }
}



// MI AZ A DTO?
// A DTO (Data Transfer Object) egy egyszerű adathordozó osztály,
// amelyet a kliens és a szerver közötti adatátvitelre használunk.
//
// MIÉRT NEM AZ ENTITÁST KÜLDJÜK?
// Az entitás (pl. Felhasznalo.cs) tartalmaz érzékeny adatokat (PasswordHash)
// és adatbázis-specifikus mezőket (navigációs property-k).
// A DTO-val CSAK a szükséges mezőket küldjük, így:
//   1. Biztonságosabb (jelszó hash soha nem megy a kliensre)
//   2. Hatékonyabb (kevesebb adat megy a hálózaton)
//   3. Rugalmasabb (az API válasz formátuma független az adatbázis sémától)
//
// MIÉRT A SHARED PROJEKTBEN VANNAK?
// A SzarnysegedShared projekt mind az API (backend), mind a Blazor (frontend) által
// hivatkozott közös könyvtár. Így a DTO-kat egyszer definiáljuk, és mindkét oldalon
// használjuk – nincs kódduplikáció.
