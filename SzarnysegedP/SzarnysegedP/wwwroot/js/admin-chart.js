// ===== ADMIN DASHBOARD DIAGRAM – CHART.JS =====
window.renderAdminChart = async function (canvasId, labels, users, posts, news, suggestions) {
    console.log(`renderAdminChart meghívva canvasId: ${canvasId}`);
    // ===== CANVAS ELEM ELLENŐRZÉSE =====
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.log(`Canvas id "${canvasId}" nem található.`);
        return;
    } 

    const ctx = canvas.getContext("2d");
    if (!ctx) {
        console.log(`Nem sikerült 2D kontextust létrehozni a canvas id "${canvasId}"-hez.`);
        return;
    }

    // ===== KORÁBBI DIAGRAM MEGSEMMISÍTÉSE =====
    if (canvas._chartInstance) {
        console.log(`Meglévő chart instance "${canvasId}"-en törölve.`);
        canvas._chartInstance.destroy();
    }

    // ===== ÚJ DIAGRAM LÉTREHOZÁSA =====
    canvas._chartInstance = new Chart(ctx, {
        // ===== DIAGRAM TÍPUS =====
        type: "line",
        // ===== ADATOK =====
        data: {
            labels: labels,
            // Adatsorok (datasets): minden sor egy vonal a diagramon
            datasets: [
                {
                    label: "Felhasználók",
                    data: users,
                    borderWidth: 2,
                    tension: 0.35
                },
                {
                    label: "Posztok",
                    data: posts,
                    borderWidth: 2,
                    tension: 0.35
                },
                {
                    label: "Hírek",
                    data: news,
                    borderWidth: 2,
                    tension: 0.35
                },
                {
                    label: "Spot javaslatok",
                    data: suggestions,
                    borderWidth: 2,
                    tension: 0.35
                }
            ]
        },
        // ===== DIAGRAM BEÁLLÍTÁSOK =====
        options: {
            responsive: true,
            maintainAspectRatio: false,
            // Plugin beállítások
            plugins: {
                legend: {
                    display: true
                }
            },
            // Tengely beállítások
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        precision: 0
                    }
                }
            }
        }
    });
};