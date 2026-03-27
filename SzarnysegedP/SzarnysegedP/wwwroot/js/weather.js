window.loadWeather = async function () {
    try {
        const response = await fetch("https://api.openweathermap.org/data/2.5/weather?id=3054643&appid=3caa2c69a27fcb1aff1c39f3d19f82c8&units=metric&lang=hu");

        if (!response.ok) {
            throw new Error(`HTTP hiba: ${response.status}`);
        }

        const data = await response.json();

        const temp = document.getElementById("weather-temp");
        const wind = document.getElementById("weather-wind");
        const visibility = document.getElementById("weather-visibility");
        const sky = document.getElementById("weather-sky");

        if (temp && data.main) {
            temp.textContent = Math.round(data.main.temp) + "°C";
        }

        if (wind && data.wind) {
            wind.textContent = Math.round(data.wind.speed * 3.6) + " km/h";
        }

        if (visibility && data.visibility !== undefined) {
            visibility.textContent = (data.visibility / 1000).toFixed(1) + " km";
        }

        if (sky && data.weather && data.weather.length > 0) {
            sky.textContent = data.weather[0].description;
        }
    } catch (err) {
        console.error("Időjárás hiba:", err);

        const temp = document.getElementById("weather-temp");
        const wind = document.getElementById("weather-wind");
        const visibility = document.getElementById("weather-visibility");
        const sky = document.getElementById("weather-sky");

        if (temp) temp.textContent = "Hiba";
        if (wind) wind.textContent = "Hiba";
        if (visibility) visibility.textContent = "Hiba";
        if (sky) sky.textContent = "Nem elérhető";
    }
};