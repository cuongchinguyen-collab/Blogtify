

window.addCopyButtons = function () {
    const codeBlocks = document.querySelectorAll('pre');

    codeBlocks.forEach((block) => {
        const code = block.innerText;
        const button = document.createElement('button');
        button.innerText = "Copy";
        button.className = "copy-code-btn btn btn-primary btn-sm";

        button.onclick = () => {

            navigator.clipboard.writeText(code).then(() => {
                button.innerText = "Copied!";
                setTimeout(() => button.innerText = "Copy", 1500);
            });
        };

        block.appendChild(button);
    });
}