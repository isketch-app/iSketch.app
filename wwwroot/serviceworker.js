var CACHE_NAME = 'is-cache-v2';

var assets = [
    '/static/pages/offline.html',
    '/static/images/isketch.svg',
    '/static/images/logo_512_square.svg',
    '/static/images/logo_32_square.png',
    '/static/fonts/icons.otf',
    '/static/fonts/Roboto.woff2',
    '/static/styles/main.css',
    '/static/styles/offline.css'
];

self.addEventListener('install', function (event) {
    event.waitUntil(
        caches.open(CACHE_NAME).then(function (cache) {
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