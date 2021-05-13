import iSketchTimer from "./iSketchTimer.js";
export default class iSketchGame {
    constructor() {
        this.GameObjects = {};
    }
    NewISketchTimer(selector, instanceID) {
        this.GameObjects[instanceID] = new iSketchTimer(selector);
    }
}