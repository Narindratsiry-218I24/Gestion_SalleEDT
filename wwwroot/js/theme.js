// Theme toggle script - adds/removes 'dark' class on <body>
(function () {
    const STORAGE_KEY = 'theme';
    const darkClass = 'dark';
    const toggleButtonId = 'themeToggleBtn';

    function setTheme(theme) {
        if (theme === 'dark') {
            document.body.classList.add(darkClass);
        } else {
            document.body.classList.remove(darkClass);
        }
        localStorage.setItem(STORAGE_KEY, theme);
    }

    function init() {
        const saved = localStorage.getItem(STORAGE_KEY);
        const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
        const theme = saved || (prefersDark ? 'dark' : 'light');
        setTheme(theme);
        const btn = document.getElementById(toggleButtonId);
        if (btn) {
            btn.addEventListener('click', () => {
                const newTheme = document.body.classList.contains(darkClass) ? 'light' : 'dark';
                setTheme(newTheme);
                // Update icon
                btn.innerHTML = newTheme === 'dark' ? '<i data-lucide="sun" class="h-5 w-5"></i>' : '<i data-lucide="moon" class="h-5 w-5"></i>';
                if (window.lucide) lucide.createIcons();
            });
        }
    }

    document.addEventListener('DOMContentLoaded', init);
})();
