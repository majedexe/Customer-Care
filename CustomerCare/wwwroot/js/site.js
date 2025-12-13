(function () {
    function applyTheme(theme) {
        if (theme === "dark") {
            document.body.classList.add("dark-mode");
        } else {
            document.body.classList.remove("dark-mode");
        }

        var iconSpan = document.getElementById("themeToggleIcon");
        if (iconSpan) {
            iconSpan.textContent = theme === "dark" ? "☀️" : "🌙";
        }
    }

    function getSavedTheme() {
        try {
            var value = localStorage.getItem("theme");
            if (value === "dark" || value === "light") {
                return value;
            }
        } catch (e) {
            // ignore
        }
        return "light";
    }

    function saveTheme(theme) {
        try {
            localStorage.setItem("theme", theme);
        } catch (e) {
            // ignore
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        var toggleBtn = document.getElementById("themeToggle");
        if (!toggleBtn) return;

        var currentTheme = getSavedTheme();
        applyTheme(currentTheme);

        toggleBtn.addEventListener("click", function () {
            var isDark = document.body.classList.contains("dark-mode");
            var newTheme = isDark ? "light" : "dark";
            applyTheme(newTheme);
            saveTheme(newTheme);
        });
    });
})();
