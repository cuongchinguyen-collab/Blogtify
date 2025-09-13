export function initBackToTopButton(buttonId) {
    const btn = document.getElementById(buttonId);
    if (!btn) return;

    window.addEventListener("scroll", () => {
        if (document.body.scrollTop > 100 || document.documentElement.scrollTop > 100) {
            btn.classList.add("show");
        } else {
            btn.classList.remove("show");
        }
    });
}

export function scrollToTop() {
    const start = window.scrollY;
    const duration = 600;
    const startTime = performance.now();

    function easeOutCubic(t) {
        return 1 - Math.pow(1 - t, 3);
    }

    function animateScroll(currentTime) {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / duration, 1);
        const eased = easeOutCubic(progress);

        window.scrollTo(0, start * (1 - eased));

        if (progress < 1) {
            requestAnimationFrame(animateScroll);
        }
    }

    requestAnimationFrame(animateScroll);
}
