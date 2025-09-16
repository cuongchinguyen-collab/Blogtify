

export function addDisqusComments() {
    var d = document, s = d.createElement('script');
    s.src = 'https://symphonix.disqus.com/embed.js';
    s.setAttribute('data-timestamp', +new Date());
    (d.head || d.body).appendChild(s);
}




export function extractHeadings(containerId) {
    const container = document.getElementById(containerId);
    if (!container) return [];

    const headings = [];
    container.querySelectorAll("h1, h2, h3, h4, h5, h6").forEach(h => {
        headings.push({
            Level: parseInt(h.tagName.substring(1)),
            Name: h.innerText,
            Anchor: h.id || null
        });
    });

    return headings;
}
