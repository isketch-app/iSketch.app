var iSketchSite = {
    ServiceWorker: null,
    WebSocket: null,
    OnConnect: function () { },
    Blazor: {},
    JSInteropHelpers: {},
    Theme: {},
    MutationObserver: new MutationObserver(MutationObserverCallback),
    Elements: {
        FloaterMenuF: {},
        PageLoader: {
            Message: document.querySelector('.pl_body .message'),
            SetMessage: function(string) {
                if (iSketchSite.Elements.PageLoader.Message != null) {
                    iSketchSite.Elements.PageLoader.Message.textContent = string;
                }
            }
        },
        ISBody: document.getElementById('is_body'),
        ISTX: null,
        ISRX: null
    },
    Communications: {
        TX: new Event('istx'),
        RX: new Event('isrx')
    },
    Reload: function() {
        iSketchSite.ServiceWorker.unregister().then(() => location.reload());
    }
}

navigator.serviceWorker.register(
    "./dynamic/sw.serviceworker.js", 
    { scope: "/" }
).then((e) => {
    iSketchSite.ServiceWorker = e;
    e.update();
});

navigator.serviceWorker.addEventListener('message', function(e) {
    if (e.data.startsWith('SW_IS_DL')) {
        iSketchSite.Elements.PageLoader.SetMessage('Downloading ' + e.data.replace('SW_IS_DL: ', '') + ' assets...');
    }
    if (e.data == 'SW_IS_RELOAD') {
        iSketchSite.Elements.PageLoader.SetMessage('Reloading...');
        setTimeout(function() {
            location.reload();
        }, (performance.now() * -1) + 1500);
    }
});


document.addEventListener('click', function (e) {
    e.composedPath().every(function (t) {
        if (t.classList != undefined && t.classList.contains('CButton')) {
            t.classList.add('clicked');
            return false;
        }
        return true;
    });
    var fmid = '';
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
            iSketchSite.OnConnect();
        }
    }
    send(data) {
        super.send(data);
        if (super.url.includes('_blazor')) {
            document.dispatchEvent(iSketchSite.Communications.TX);
        }
    }
}

WebSocket = WebSocketOverride;

iSketchSite.OnConnect = function () {
    iSketchSite.WebSocket.addEventListener('error', function () {
        //location.reload();
    });
    iSketchSite.WebSocket.addEventListener('close', function () {
        location.reload();
    });
    iSketchSite.WebSocket.addEventListener('message', function () {
        document.dispatchEvent(iSketchSite.Communications.RX);
    });
    /*
    document.addEventListener('istx', function () {
        
    });
    document.addEventListener('isrx', function () {
        
    });
    */
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
    Blazor.defaultReconnectionHandler.onConnectionDown = null;
    iSketchSite.Elements.PageLoader.SetMessage('');
    iSketchSite.Communications.Init();
    if (iSketchSite.ServiceWorker.installing == null) {
        iSketchSite.Elements.PageLoader.Hide();
    } else {
        iSketchSite.Elements.PageLoader.SetMessage('Downloading assets...');
    }
};

iSketchSite.Communications.Flicker = function (element) {
    if (!element.classList.contains('on')) {
        element.classList.add('on');
        setTimeout(function () {
            element.classList.remove('on');
        }, 100);
    }
}

iSketchSite.Communications.Init = function () {
    iSketchSite.Elements.ISTX = document.querySelector('.indicator .tx');
    iSketchSite.Elements.ISRX = document.querySelector('.indicator .rx');
    document.addEventListener('istx', function () {
        iSketchSite.Communications.Flicker(iSketchSite.Elements.ISTX);
    });
    document.addEventListener('isrx', function () {
        iSketchSite.Communications.Flicker(iSketchSite.Elements.ISRX);
    });
}

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