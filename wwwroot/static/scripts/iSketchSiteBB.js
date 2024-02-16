var iSketchSite = {
    WebSocket: null,
    onconnect: function () { },
    Blazor: {},
    JSInteropHelpers: {},
    Theme: {},
    Loader: {
        AssetsToLoad: 0,
        AssetsLoaded: 0
    },
    MutationObserver: new MutationObserver(MutationObserverCallback),
    Elements: {
        FloaterMenuF: {},
        PageLoader: {
            Message: document.querySelector('.pl_body .message')
        },
        ISBody: document.getElementById("is_body")
    }
}

document.addEventListener('click', function (e) {
    e.composedPath().every(function (t) {
        if (t.classList != undefined && t.classList.contains('CButton')) {
            t.classList.add('clicked');
            return false;
        }
        return true;
    });
    var fmid = "";
    e.composedPath().every(function (t) {
        if (t.attributes != undefined && t.attributes.fmid != undefined) {
            fmid = t.attributes.fmid.value;
            return false;
        }
        return true;
    });
    document.querySelectorAll('.floater-menu, .rmi-mi').forEach(function (t) {
        if (t.attributes.fmid.value != fmid) t.classList.remove('visible');
    });
});

document.addEventListener('keydown', function (e) {
    if (e.target.attributes['clickonenter'] !== undefined &&
        e.target.attributes['clickonenter'] !== '' &&
        e.key == 'Enter') {
        var target = document.querySelector('[name=' + e.target.attributes['clickonenter'].value + ']');
        target.focus();
        target.click();
    }
    if (e.target.attributes['focusonenter'] !== undefined &&
        e.target.attributes['focusonenter'] !== '' &&
        e.key == 'Enter') {
        document.querySelector('[name=' + e.target.attributes['focusonenter'].value + ']').focus();
    }
});

iSketchSite.Elements.ISBody.addEventListener('scroll', function (e) {
    if (e.target.scrollTop > 0) {
        e.target.classList.add('scrolled');
    } else {
        e.target.classList.remove('scrolled');
    }
});

class WebSocketOverride extends WebSocket {
    constructor(url, protos) {
        super(url, protos);
        if (url.includes('_blazor')) {
            iSketchSite.WebSocket = this;
            iSketchSite.onconnect();
        }
    }
}

WebSocket = WebSocketOverride;

iSketchSite.onconnect = function () {
    iSketchSite.WebSocket.addEventListener('error', function () {
        //window.location.reload();
    });
    iSketchSite.WebSocket.addEventListener('close', function () {
        //window.location.reload();
    });
}

iSketchSite.registerJSInteropHelper = function (componentID, dotNetHelper) {
    iSketchSite.JSInteropHelpers[componentID] = dotNetHelper;
};

iSketchSite.Elements.FloaterMenuF.Toggle = function (item) {
    var FMID = item.attributes.fmid.value;
    var FM = document.querySelector('.floater-menu[fmid=' + FMID + ']');
    var RMI = document.querySelector('.rmi-mi[fmid=' + FMID + ']');
    if (FM.classList.contains('visible')) {
        FM.classList.remove('visible');
        RMI.classList.remove('visible');
    } else {
        FM.classList.add('visible');
        RMI.classList.add('visible');
    }
};

iSketchSite.Elements.PageLoader.Hide = function () {
    document.querySelector('.pl_body').classList.add('fadeout');
    setTimeout(function () { document.querySelector('.pl_body').remove(); }, 300);
};

iSketchSite.Blazor.Ready = function () {
    iSketchSite.Loader.Start();
};

iSketchSite.Loader.Start = function () {
    iSketchSite.Elements.PageLoader.Message.textContent = 'Retrieving asset list...';
    var req = new XMLHttpRequest();
    req.open('GET', '/dynamic/static.json');
    req.onload = function () {
        var assets = JSON.parse(req.response);
        iSketchSite.Loader.AssetsToLoad = assets.length;
        iSketchSite.Elements.PageLoader.Message.textContent = 'Loading assets (0 / ' + iSketchSite.Loader.AssetsToLoad + ')...';
        assets.forEach(function (asset) {
            iSketchSite.Loader.LoadAsset(asset);
        });
    };
    req.send();
};

iSketchSite.Loader.LoadAsset = function (path) {
    var req = new XMLHttpRequest();
    req.open('GET', path);
    req.onload = function () {
        iSketchSite.Loader.AssetsLoaded++;
        iSketchSite.Elements.PageLoader.Message.textContent = 'Loading assets (' + iSketchSite.Loader.AssetsLoaded + ' / ' + iSketchSite.Loader.AssetsToLoad + ')...';
        if (iSketchSite.Loader.AssetsToLoad == iSketchSite.Loader.AssetsLoaded) {
            iSketchSite.Elements.PageLoader.Hide();
        }
    };
    req.send();
};

iSketchSite.Theme.ChangeTheme = function (setDark) {
    iSketchSite.Elements.ISBody.classList.remove('theme_light');
    iSketchSite.Elements.ISBody.classList.remove('theme_dark');
    if (setDark) {
        iSketchSite.Elements.ISBody.classList.add('theme_dark');
    } else {
        iSketchSite.Elements.ISBody.classList.add('theme_light');
    }
};

iSketchSite.Theme.ChangeTheme(window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches);
window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
    iSketchSite.Theme.ChangeTheme(e.matches);
});

function MutationObserverCallback(list, observer) {
    list.forEach(function (record) {
        record.addedNodes.forEach(function (node) {
            if (node.attributes !== undefined && node.attributes['autofocus'] !== undefined) {
                node.focus();
            }
        });
    });
}

iSketchSite.MutationObserver.observe(document, {childList: true, subtree: true});