iSketchSite.Blazor.reconnectHandlerFailedRejected = function(e = null) {
    location.reload();
};
/*Blazor.defaultReconnectionHandler.defaultOnConnectionDown = Blazor.defaultReconnectionHandler.onConnectionDown;
Blazor.defaultReconnectionHandler.onConnectionDown = function (opts, message) {
    opts.maxRetries = 3;
    opts.retryIntervalMilliseconds = 1000;
    Blazor.defaultReconnectionHandler.defaultOnConnectionDown(opts, message);
    Blazor.defaultReconnectionHandler._reconnectionDisplay.message.innerText = "Connection lost, reconnecting...";
    Blazor.defaultReconnectionHandler._reconnectionDisplay.failed = iSketchSite.Blazor.reconnectHandlerFailedRejected;
    Blazor.defaultReconnectionHandler._reconnectionDisplay.rejected = iSketchSite.Blazor.reconnectHandlerFailedRejected;
};*/