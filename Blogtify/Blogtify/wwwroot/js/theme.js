window.themeSwitcher = {
    setTheme: function (themeName) {
        const link = document.getElementById("theme-stylesheet");
        if (link) {
            link.href = `css/theme-${themeName.toLowerCase()}.css`;
        }
    }
};

(function () {
    var theme = localStorage.getItem("Theme") || "Yeti";
    themeSwitcher.setTheme(theme);
})();