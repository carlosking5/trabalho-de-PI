const themeKey = "cineverse.theme";

export function isLightTheme() {
    try {
        return localStorage.getItem(themeKey) === "light";
    } catch {
        return false;
    }
}

export function saveTheme(light) {
    try {
        localStorage.setItem(themeKey, light ? "light" : "dark");
    } catch {
        // O tema continua ativo nesta página se o navegador bloquear a gravação.
    }
}

export function showDetails(dialog) {
    if (!dialog.open) dialog.showModal();
}
