console.log("terkep.js loaded");
window.__terkep_script_loaded = true;

window.addEventListener("error", function (evt) {
    console.error("Global error event:", evt.message, evt.filename + ":" + evt.lineno + ":" + evt.colno, evt.error);
});

window.addEventListener("unhandledrejection", function (evt) {
    console.error("Unhandled promise rejection:", evt.reason);
});


window.__mapbox_initializing = window.__mapbox_initializing || false;
window.__mapbox_initialized = window.__mapbox_initialized || false;

window.initMapbox = async function (containerId) {
    console.log("initMapbox called with containerId:", containerId);

    if (window.__mapbox_initialized) {
        console.log("initMapbox: already initialized - skipping");
        return;
    }
    if (window.__mapbox_initializing) {
        console.log("initMapbox: initialization already in progress - skipping");
        return;
    }

    window.__mapbox_initializing = true;

    try {
        if (!window.mapboxgl) {
            console.error("mapboxgl not found. Ensure Mapbox JS is loaded in the page head.");
            window.__mapbox_initializing = false;
            return;
        }

        const container = document.getElementById(containerId);
        if (!container) {
            console.error(`container "${containerId}" not found.`);
            window.__mapbox_initializing = false;
            return;
        }

        if (!container.style.height || container.style.height === "") {
            container.style.height = "600px";
        }

        console.log("Initializing Mapbox in container:", containerId);

        mapboxgl.accessToken = window.apiKey;

        let map;
        try {
            map = new mapboxgl.Map({
                container: containerId,
                style: 'mapbox://styles/mapbox/outdoors-v12',
                center: [19.0402, 47.4979],
                zoom: 6
            });
        } catch (ex) {
            console.error("Failed to create Mapbox map:", ex);
            window.__mapbox_initializing = false;
            return;
        }

        
        window.__mapbox_map = map;

        map.on('load', () => console.log("Mapbox map loaded (style & tiles started)."));
        map.on('idle', () => console.log("Mapbox idle (all tiles and sources loaded)."));
        map.on('error', (e) => console.error("Mapbox error event:", e));

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
            try {
                console.log("Fetching wind:", url);
                const response = await fetch(url);
                console.log("Wind fetch status:", response.status, response.statusText);
                if (!response.ok) {
                    console.warn("Wind API returned non-ok status:", response.status);
                    return "N/A";
                }
                const data = await response.json();
                return data?.current_weather?.windspeed ?? "N/A";
            } catch (err) {
                console.warn("Wind fetch failed:", err);
                return "N/A";
            }
        }

        async function loadWindAndMarkers() {
            const tbody = document.getElementById("spot-list");
            
            if (tbody) {
                tbody.innerHTML = "";
            } else {
                console.warn("spot-list element not found; markers will still be added to the map.");
            }

            for (let i = 0; i < places.length; i++) {
                const place = places[i];
                const ws = await getWind(place.lat, place.lon);

                if (tbody) {
                    const tr = document.createElement("tr");
                    tr.innerHTML = `
                        <td style="display:flex;align-items:center;">
                            <div style="background-color:${place.color};width:12px;height:12px;margin-right:6px;border-radius:2px;"></div>
                            ${place.name}
                        </td>
                        <td>${place.lon === 6.1293 ? "Franciaország" : "Magyarország"}</td>
                        <td>${place.lat === 45.8992 ? "922 m" : place.lat === 47.7 ? "699 m" : "495 m"}</td>
                        <td>${ws} ${ws === "N/A" ? "" : "km/h"}</td>
                    `;
                    tbody.appendChild(tr);
                }

                try {
                    new mapboxgl.Marker({ color: place.color })
                        .setLngLat([place.lon, place.lat])
                        .setPopup(new mapboxgl.Popup().setHTML(`<strong>${place.name}</strong><br>Szél: ${ws} ${ws === "N/A" ? "" : "km/h"}`))
                        .addTo(map);
                } catch (markerError) {
                    console.warn("Failed adding marker for", place.name, markerError);
                }
            }
        }

        try {
            await loadWindAndMarkers();
            console.log("Markers and spot list populated (if data available).");
            
            window.__mapbox_initialized = true;
        } catch (err) {
            console.error("Error while loading markers/spot list:", err);
            
            window.__mapbox_initialized = false;
        } finally {
            window.__mapbox_initializing = false;
        }
    } catch (e) {
        console.error("initMapbox top-level error:", e);
        window.__mapbox_initializing = false;
    }
};

(function autoInitMapbox() {
    const maxAttempts = 20;
    const intervalMs = 300;
    let attempts = 0;
    const id = setInterval(async () => {
        attempts++;
        try {
            if (typeof window.initMapbox === "function" && document.getElementById("map")) {
                console.log("autoInitMapbox: conditions met, calling initMapbox");
                await window.initMapbox("map");
                clearInterval(id);
                console.log("autoInitMapbox: initialized and stopped polling");
                return;
            }
        } catch (err) {
            console.warn("autoInitMapbox: initMapbox threw:", err);
        }

        if (attempts >= maxAttempts) {
            clearInterval(id);
            console.warn("autoInitMapbox: giving up after", attempts, "attempts");
        }
    }, intervalMs);
})();

