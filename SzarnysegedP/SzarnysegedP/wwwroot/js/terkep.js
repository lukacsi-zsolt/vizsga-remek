console.log("terkep.js loaded");
window.__terkep_script_loaded = true;

window.__mapbox_initializing = window.__mapbox_initializing || false;
window.__mapbox_initialized = window.__mapbox_initialized || false;
window.__mapbox_map = window.__mapbox_map || null;
window.__mapbox_spot_slug = window.__mapbox_spot_slug || null;
window.__mapbox_dotnet_ref = window.__mapbox_dotnet_ref || null;
window.__mapbox_markers = window.__mapbox_markers || [];

window.addEventListener("error", function (evt) {
    console.error("Global error event:", evt.message, evt.filename + ":" + evt.lineno + ":" + evt.colno, evt.error);
});

window.addEventListener("unhandledrejection", function (evt) {
    console.error("Unhandled promise rejection:", evt.reason);
});

window.__api_base_url = window.__api_base_url || "";

window.setMapboxConfig = function (apiKey, spotSlug, apiBaseUrl) {
    window.apiKey = apiKey;
    window.__mapbox_spot_slug = spotSlug || null;
    window.__api_base_url = apiBaseUrl || "";
    console.log("Mapbox config set. spotSlug:", window.__mapbox_spot_slug);
    console.log("API base URL set:", window.__api_base_url);
};

window.setMapboxDotNetRef = function (dotnetRef) {
    window.__mapbox_dotnet_ref = dotnetRef;
    console.log("Mapbox dotnet ref set.");
};

async function getSpotokFromApi() {
    try {
        const response = await fetch(`${window.__api_base_url}api/spotok`);
        if (!response.ok) {
            console.error("Spot API hiba:", response.status, response.statusText);
            return [];
        }

        const data = await response.json();
        return Array.isArray(data) ? data : [];
    } catch (err) {
        console.error("Spot API fetch hiba:", err);
        return [];
    }
}

async function getWind(lat, lon) {
    if (lat == null || lon == null) return "N/A";

    const url = `https://api.open-meteo.com/v1/forecast?latitude=${lat}&longitude=${lon}&current_weather=true`;

    try {
        const response = await fetch(url);
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

function getMarkerColor(spot) {
    if (!spot || !spot.magassag) return "#4985c9";
    if (spot.magassag >= 800) return "#fc0341";
    if (spot.magassag >= 600) return "#fcba03";
    return "#4985c9";
}

function formatMagassag(magassag) {
    if (magassag == null) return "-";
    return `${magassag} m`;
}

function formatOrszag(orszag) {
    if (!orszag || orszag.trim() === "") return "Ismeretlen";
    return orszag;
}

function clearMarkers() {
    if (!window.__mapbox_markers) return;

    for (const marker of window.__mapbox_markers) {
        try {
            marker.remove();
        } catch {
        }
    }

    window.__mapbox_markers = [];
}

async function renderSpotok(map, spotok) {
    const tbody = document.getElementById("spot-list");

    if (tbody) {
        tbody.innerHTML = "";
    }

    clearMarkers();

    let spotToZoom = null;
    const selectedSlug = window.__mapbox_spot_slug;

    for (const spot of spotok) {
        if (spot.lat == null || spot.lon == null) {
            console.warn("Spot koordináta nélkül kihagyva:", spot);
            continue;
        }

        const color = getMarkerColor(spot);
        const ws = await getWind(spot.lat, spot.lon);

        if (tbody) {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td style="display:flex;align-items:center;">
                    <div style="background-color:${color};width:12px;height:12px;margin-right:6px;border-radius:2px;"></div>
                    ${spot.nev ?? "Névtelen spot"}
                </td>
                <td>${formatOrszag(spot.orszag)}</td>
                <td>${formatMagassag(spot.magassag)}</td>
                <td>${ws} ${ws === "N/A" ? "" : "km/h"}</td>
            `;
            tbody.appendChild(tr);

            tr.style.cursor = "pointer";
            tr.addEventListener("click", () => {
                map.flyTo({
                    center: [spot.lon, spot.lat],
                    zoom: 11,
                    essential: true
                });
            });
        }

        try {
            const popupHtml = `
                <strong>${spot.nev ?? "Névtelen spot"}</strong><br>
                ${spot.orszag ?? "Ismeretlen ország"}<br>
                Magasság: ${formatMagassag(spot.magassag)}<br>
                Szél: ${ws} ${ws === "N/A" ? "" : "km/h"}
            `;

            const marker = new mapboxgl.Marker({ color })
                .setLngLat([spot.lon, spot.lat])
                .setPopup(new mapboxgl.Popup().setHTML(popupHtml))
                .addTo(map);

            window.__mapbox_markers.push(marker);
        } catch (markerError) {
            console.warn("Failed adding marker for", spot.nev, markerError);
        }

        if (selectedSlug && spot.slug === selectedSlug) {
            spotToZoom = spot;
        }
    }

    if (spotToZoom) {
        console.log("Zooming to selected spot:", spotToZoom.slug);
        map.flyTo({
            center: [spotToZoom.lon, spotToZoom.lat],
            zoom: 11,
            essential: true
        });
    }
}

window.reloadMapboxSpots = async function () {
    const map = window.__mapbox_map;
    if (!map) {
        console.warn("reloadMapboxSpots: map not initialized");
        return;
    }

    const spotok = await getSpotokFromApi();
    await renderSpotok(map, spotok);
};

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
            console.error("mapboxgl not found. Ensure Mapbox JS is loaded.");
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

        mapboxgl.accessToken = window.apiKey;

        let map;
        try {
            map = new mapboxgl.Map({
                container: containerId,
                style: "mapbox://styles/mapbox/outdoors-v12",
                center: [19.0402, 47.4979],
                zoom: 6
            });
        } catch (ex) {
            console.error("Failed to create Mapbox map:", ex);
            window.__mapbox_initializing = false;
            return;
        }

        window.__mapbox_map = map;

        map.on("load", async () => {
            console.log("Mapbox map loaded.");

            try {
                map.addControl(new mapboxgl.NavigationControl());
                map.scrollZoom.disable();

                map.on("click", async (e) => {
                    if (window.__mapbox_dotnet_ref) {
                        try {
                            await window.__mapbox_dotnet_ref.invokeMethodAsync(
                                "HandleMapClick",
                                e.lngLat.lat,
                                e.lngLat.lng
                            );
                        } catch (err) {
                            console.error("Map click -> .NET callback hiba:", err);
                        }
                    }
                });

                const spotok = await getSpotokFromApi();
                await renderSpotok(map, spotok);

                window.__mapbox_initialized = true;
            } catch (err) {
                console.error("Error while rendering spotok:", err);
                window.__mapbox_initialized = false;
            } finally {
                window.__mapbox_initializing = false;
            }
        });

        map.on("error", (e) => console.error("Mapbox error event:", e));
    } catch (e) {
        console.error("initMapbox top-level error:", e);
        window.__mapbox_initializing = false;
    }
};