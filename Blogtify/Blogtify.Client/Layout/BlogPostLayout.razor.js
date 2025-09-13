

export function addDisqusComments() {
    var d = document, s = d.createElement('script');
    s.src = 'https://symphonix.disqus.com/embed.js';
    s.setAttribute('data-timestamp', +new Date());
    (d.head || d.body).appendChild(s);
}