window.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        fetch('https://api.openweathermap.org/data/2.5/weather?id=3054643&appid=3caa2c69a27fcb1aff1c39f3d19f82c8&units=metric&lang=hu')
            .then(response => response.json())
            .then(data => {
                document.getElementById('weather-temp').textContent = Math.round(data.main.temp) + '°C';
                document.getElementById('weather-wind').textContent = Math.round(data.wind.speed * 3.6) + ' km/h';
                document.getElementById('weather-visibility').textContent = (data.visibility / 1000) + ' km';
                document.getElementById('weather-sky').textContent = data.weather[0].description;
            })
            .catch(err => console.error('Időjárás hiba:', err));
    }, 500);
});