mapboxgl.accessToken = 'pk.eyJ1IjoienNvbHRsdWthY3NpIiwiYSI6ImNta2R6dWh2eDAxcmMzZnF3dXptbnpwcDEifQ.pkJgWCxAikfxhxLGQwrIuA';
const map = new mapboxgl.Map({
    container: 'map',
    style: 'mapbox://styles/mapbox/outdoors-v12',
    center: [19.0402, 47.4979],
    zoom: 6
});
map.addControl(new mapboxgl.NavigationControl());
map.scrollZoom.disable();

const places = [
    { name: "Dobogókő", lat: 47.7, lon: 18.9, color: "#fcba03" },
    { name: "Hármashatár-hegy", lat: 47.5545, lon: 18.9989, color: "#fc0341" },
    { name: "Annecy", lat: 45.8992, lon: 6.1293, color: "#4985c9" },
    { name: "Szársomlyó", lat: 45.8550005, lon: 18.4099661, color: "#4985c9" },
    { name: "Csobánc", lat: 46.8728286, lon: 17.4624976, color: "#4985c9" }
];

async function getWind(lat, lon) {
    const url = `https://api.open-meteo.com/v1/forecast?latitude=${lat}&longitude=${lon}&current_weather=true`;
    const response = await fetch(url);
    const data = await response.json();
    return data.current_weather.windspeed;
}

async function loadWindAndMarkers() {
    const tbody = document.getElementById("spot-list");
    for (let i = 0; i < places.length; i++) {
        const ws = await getWind(places[i].lat, places[i].lon);
        tbody.innerHTML += `
                <tr>
                    <td style="display:flex;"><div style="background-color:${places[i].color}"></div>${places[i].name}</td>
                    <td>${places[i].lon === 6.1293 ? "Franciaország" : "Magyarország"}</td>
                    <td>${places[i].lat === 45.8992 ? "922 m" : places[i].lat === 47.7 ? "699 m" : "495 m"}</td>
                    <td>${ws} km/h</td>
                </tr>`;
        new mapboxgl.Marker({ color: places[i].color }).setLngLat([places[i].lon, places[i].lat])
            .setPopup(new mapboxgl.Popup().setHTML(`<strong>${places[i].name}</strong><br>Szél: ${ws} km/h`))
            .addTo(map);
    }
}
loadWindAndMarkers();