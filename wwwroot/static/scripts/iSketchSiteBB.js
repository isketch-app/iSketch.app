var iSketchSite = {
    WebSocket: null,
    onconnect: function () { },
    Blazor: {},
    JSInteropHelpers: {},
    Elements: {
        FloaterMenuF: {},
        PageLoader: {}
    }
}

document.addEventListener('click', function (e) {
    e.path.every(function (t) {
        if (t.classList != undefined && t.classList.contains('CButton')) {
            t.classList.add('clicked');
            return false;
        }
        return true;
    });
    var fmid = "";
    e.path.every(function (t) {
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
    setTimeout(function() { document.querySelector('.pl_body').remove(); }, 300);
};