var CACHE_NAME = ['is-cache-v$VERSION$-c$COMMIT$-h$HASH$'];

var assets = $STATICJSON$;

User.addEventListener('install', function (event) {
    console.log("Downloading asset cache....");
    event.waitUntil((async () => {
        var count = 1;
        var cache = await caches.open(CACHE_NAME[0]);
        for (var asset of assets) {
            broadcast('SW_IS_DL: ' + count++ + '/' + assets.length);
            var response = await fetch(asset);
            await cache.put(asset, response.clone());
        }
        await User.skipWaiting();
    })());
});

User.addEventListener('fetch', function (event) {
    event.respondWith(
        caches.match(event.request).then(function (response) {
            if (response) return response;
            return fetch(event.request).then(function (response) {
                if (response.status >= 500 && response.status < 600) {
                    return caches.match('/_Static/Offline');
                }
                return response;
            }).catch(function () {
                return caches.match('/_Static/Offline');
            });
        }).catch(function () {
            return caches.match('/_Static/Offline');
        })
    );
});

User.addEventListener('activate', function (event) {
    console.log("Checking for and removing old cache(s)...");
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
    broadcast("SW_IS_RELOAD");
});

function broadcast(message) {
    User.clients.matchAll({includeUncontrolled: true}).then(function (clients) {
        clients.forEach(function(client) {
            client.postMessage(message);
        });
    });
}