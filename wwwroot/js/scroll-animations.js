document.addEventListener("DOMContentLoaded", function () {
    const prefersReduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const scrollItems = document.querySelectorAll('.scroll-appear');

    if (prefersReduced) {
        scrollItems.forEach(el => el.classList.add('is-visible'));
    } else if ('IntersectionObserver' in window) {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.18 });
        scrollItems.forEach(el => observer.observe(el));
    } else {
        scrollItems.forEach(el => el.classList.add('is-visible'));
    }

    const parallaxNodes = document.querySelectorAll('[data-parallax-speed]');
    if (!prefersReduced && parallaxNodes.length) {
        window.addEventListener('scroll', () => {
            const offset = window.scrollY;
            parallaxNodes.forEach(node => {
                const speed = parseFloat(node.dataset.parallaxSpeed) || 0.2;
                node.style.transform = `translateY(${offset * speed}px)`;
            });
        }, { passive: true });
    }
});
