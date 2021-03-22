import { Utils } from "./utils.js";

export class HttpTrafficControlService {
    
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
    }

    registerVehicleEntry(car) {
        console.log(`entry cam: ${car.id}`);
        const request = this.createRequest(car);
        fetch(this.baseUrl + '/entrycam', request);
    }

    registerVehicleExit(car) {
        console.log(`exit cam: ${car.id}`);
        const request = this.createRequest(car);
        fetch(this.baseUrl + '/exitcam', request);
    }

    createRequest(car) {
        const body = {
            lane: car.lane.number,
            licenseNumber: car.id,
            timestamp: Utils.localTime()
        };

        return {
            method: 'POST',
            body: JSON.stringify(body),
            headers: {
                'Content-Type': 'application/json'
            }
        }
    }
}