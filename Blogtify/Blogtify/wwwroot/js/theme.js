window.themeSwitcher = {
    setTheme: function (themeName) {
        const link = document.getElementById("theme-stylesheet");
        if (link) {
            link.href = `css/theme-${themeName.toLowerCase()}.css`;
        }
    }
};
