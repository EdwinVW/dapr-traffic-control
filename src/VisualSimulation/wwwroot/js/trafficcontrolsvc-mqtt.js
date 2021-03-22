import { Utils } from "./utils.js";

export class MqttTrafficControlService {

    constructor(host, port) {
        this.client = new Paho.MQTT.Client(host, port, 'simulation-ui');
        this.client.connect();
    }

    registerVehicleEntry(car) {
        console.log(`entry cam: ${car.id}`);
        const message = new Paho.MQTT.Message(this.createMessage(car));
        message.destinationName = 'trafficcontrol/entrycam';
        this.client.send(message);
    }

    registerVehicleExit(car) {
        console.log(`exit cam: ${car.id}`);
        const message = new Paho.MQTT.Message(this.createMessage(car));
        message.destinationName = 'trafficcontrol/exitcam';
        this.client.send(message);
    }

    createMessage(car) {
        return JSON.stringify({
            lane: car.lane.number,
            licenseNumber: car.id,
            timestamp: Utils.localTime()
        });
    }
}