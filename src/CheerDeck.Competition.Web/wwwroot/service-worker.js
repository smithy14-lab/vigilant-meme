self.addEventListener('install', event => self.skipWaiting());
self.addEventListener('activate', event => event.waitUntil(self.clients.claim()));

self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') return;

    const url = new URL(event.request.url);

    if (url.pathname.startsWith('/_framework/') ||
        url.pathname.startsWith('/_blazor') ||
        url.pathname.startsWith('/hubs/')) {
        return;
    }

    event.respondWith(
        fetch(event.request)
            .catch(() => caches.match(event.request))
    );
});
