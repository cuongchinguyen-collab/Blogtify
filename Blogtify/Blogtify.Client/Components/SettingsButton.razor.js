export const outsideClickHelper = {
    register: (selector, dotnetHelper) => {
        document.addEventListener("click", (e) => {
            const wrapper = document.querySelector(selector);
            if (wrapper && !wrapper.contains(e.target)) {
                dotnetHelper.invokeMethodAsync("CloseMenus");
            }
        });
    }
};
