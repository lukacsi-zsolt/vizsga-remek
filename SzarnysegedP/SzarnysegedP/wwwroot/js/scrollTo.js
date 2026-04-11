// ===== SMOOTH SCROLL SEGÉDFÜGGVÉNY =====
window.scrollToSection = (id) => {
    // Az elem megkeresése az ID alapján a DOM-ban
    const element = document.getElementById(id);

    if (element) {
        // scrollIntoView: beépített böngésző API – az elemet a látható területre görgeti
        element.scrollIntoView({
            behavior: "smooth", // Sima, animált görgetés (nem ugrás)
            block: "start"      // Az elem teteje kerüljön a képernyő tetejére
                                // Egyéb opciók: "center" (középre), "end" (aljára)
        });
    }
};