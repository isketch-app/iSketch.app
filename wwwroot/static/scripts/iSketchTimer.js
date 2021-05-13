export default class iSketchTimer {
    constructor(selector) {
        this.is_timer = document.querySelector(selector);
        this.ctx = this.is_timer.getContext("2d");

        this.colorTM = "skyblue";
        this.colorTM2 = "pink";
        this.stroke = "transparent";
        this.warnAt = 0.25;
        this.radius = 50;
        this.originX = 55;
        this.originY = 55;
        this.startRad = (Math.PI / 2 * -1);
        this.endRad = (Math.PI / 2 * 3);
        this.radDiff = this.endRad - this.startRad;
        this.timerMS = 10000;
        this.startMS = 0;
        this.isTick = false;
        this.ding = new Audio("./static/sounds/iSketchTimer.ding.ogg");

        this.audioCtx = new AudioContext();
        this.req = new Request("./static/sounds/iSketchTimer.tick.ogg");
        this.audioSrc = null;
        this.audioBuf = null;

        var parent = this;
        fetch(this.req).then(function (resp) {
            resp.arrayBuffer().then(function (buffer) {
                parent.audioCtx.decodeAudioData(buffer, function (decoded) {
                    parent.audioBuf = decoded;
                });
            });
        });

        this.ctx.strokeStyle = this.stroke;
    }

    loop(parent) {
        var timerRatio = ((performance.now() - this.startMS) / this.timerMS);
        this.ctx.fillStyle = "#fff"
        this.ctx.fillRect(0, 0, this.is_timer.width, this.is_timer.height);
        if (timerRatio >= 1) {
            this.stopTick();
            this.ding.play();
            return;
        }
        if (timerRatio >= this.warnAt) {
            this.startTick();
            this.ctx.fillStyle = this.colorTM2;
        } else {
            this.stopTick();
            this.ctx.fillStyle = this.colorTM;
        }
        this.ctx.beginPath();
        this.ctx.moveTo(this.originX, this.originY);
        this.ctx.arc(this.originX, this.originY, this.radius, (this.radDiff * timerRatio) + this.startRad, this.startRad);
        this.ctx.lineTo(this.originX, this.originY);
        this.ctx.fill();
        this.ctx.stroke();
        requestAnimationFrame(function () {
            parent.loop(parent);
        });
    }

    startTimer() {
        var parent = this;
        this.startMS = performance.now();
        this.ctx.fillStyle = this.colorTM;
        requestAnimationFrame(function () {
            parent.loop(parent);
        });
    }

    startTick() {
        if (this.isTick) return;
        if (this.audioSrc != null) this.audioSrc.stop(0);
        this.isTick = true;
        this.audioSrc = this.audioCtx.createBufferSource();
        this.audioSrc.buffer = this.audioBuf;
        this.audioSrc.loop = true;
        this.audioSrc.connect(this.audioCtx.destination);
        this.audioSrc.start(0);
    }

    stopTick() {
        if (this.audioSrc != null) this.audioSrc.stop(0);
        this.isTick = false;
    }
}