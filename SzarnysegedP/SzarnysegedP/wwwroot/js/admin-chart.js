window.renderAdminChart = async function (canvasId, labels, users, posts, news, suggestions) {
    console.log(`renderAdminChart meghívva canvasId: ${canvasId}`);
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

    if (canvas._chartInstance) {
        console.log(`Meglévő chart instance "${canvasId}"-en törölve.`);
        canvas._chartInstance.destroy();
    }

    canvas._chartInstance = new Chart(ctx, {
        type: "line",
        data: {
            labels: labels,
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
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true
                }
            },
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