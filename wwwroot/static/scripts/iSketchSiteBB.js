var iSketchSite = {
    WebSocket: null,
    onconnect: function () { },
    Blazor: {},
    JSInteropHelpers: {}
}

document.body.addEventListener('click', function (e) {
    e.path.forEach(function (t) {
        if (t.classList != undefined && t.classList.contains('CButton')) {
            t.classList.add('clicked');
            return true;
        }
    });
});

document.getElementById('is_body').addEventListener('scroll', function (e) {
    if (e.srcElement.scrollTop > 0) {
        e.srcElement.classList.add('scrolled');
    } else {
        e.srcElement.classList.remove('scrolled');
    }
});

class WebSocketOverride extends WebSocket {
    constructor(url, protos) {
        super(url, protos);
        iSketchSite.WebSocket = this;
        iSketchSite.onconnect();
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