var CACHE_NAME = ['is-cache-v$VERSION$'];

var assets = [
    '/static/pages/offline.html',
    '/static/images/isketch.svg',
    '/static/images/logo_512_square.svg',
    '/static/images/logo_32_square.png',
    '/static/images/spinner.svg',
    '/static/fonts/icons.otf',
    '/static/fonts/Roboto.woff2',
    '/static/styles/main.css',
    '/static/styles/offline.css'
];

self.addEventListener('install', function (event) {
    event.waitUntil(
        caches.open(CACHE_NAME[0]).then(function (cache) {
            return cache.addAll(assets);
        })
    );
});

self.addEventListener('fetch', function (event) {
    event.respondWith(
        caches.match(event.request).then(function (response) {
            if (response) return response;
            return fetch(event.request);
        }).catch(function () {
            return caches.match('/static/pages/offline.html');
        })
    );
});

self.addEventListener('activate', function (event) {
    event.waitUntil(
        caches.keys().then(function (cacheNames) {
            return Promise.all(
                cacheNames.map(function (cacheName) {
                    if (CACHE_NAME.indexOf(cacheName) === -1) {
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});