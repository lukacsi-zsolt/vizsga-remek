window.renderAdminChart = function (canvasId, labels, users, posts, news, suggestions) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    if (canvas._chartInstance) {
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