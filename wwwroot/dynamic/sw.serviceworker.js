var CACHE_NAME = ['is-cache-v$VERSION$'];

var assets = $STATICJSON$;

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