window.setBodyLanguageMode = function (mode) {
    document.body.classList.remove('default-mode', 'minecraft-mode');
    if (mode === 'minecraft') {
        document.body.classList.add('minecraft-mode');
    } else {
        document.body.classList.add('default-mode');
    }
};
