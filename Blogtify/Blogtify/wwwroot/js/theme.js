window.themeSwitcher = {
    setTheme: function (themeName) {
        const link = document.getElementById("theme-stylesheet");
        if (link) {
            link.href = `css/theme-${themeName.toLowerCase()}.css`;
        }

        const lightThemes = ['yeti', 'flatly', 'lumen', 'materia', 'simplex', 'sketchy', 'sandstone'];
        const darkThemes = ['darkly', 'slate', 'superhero', 'vapor', 'solar'];
        const codeBlockLink = document.getElementById("code-block-stylesheet");
        if (codeBlockLink) {
            if (lightThemes.includes(themeName.toLowerCase())) {
                codeBlockLink.href = `css/prism-coy-without-shadows.min.css`;
            }
            else if (darkThemes.includes(themeName.toLowerCase())) {
                codeBlockLink.href = `css/prism-vsc-dark-plus.min.css`;
            }
        }
    }
};

(function () {
    const theme = localStorage.getItem("Theme") || "Yeti";
    themeSwitcher.setTheme(theme);
})();