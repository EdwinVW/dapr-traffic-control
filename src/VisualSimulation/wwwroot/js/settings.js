import { AggressiveMergeBehavior } from './merge-aggressive.js';
import { MqttTrafficControlService } from './trafficcontrolsvc-mqtt.js';
import { HttpTrafficControlService } from './trafficcontrolsvc-http.js';

export const Settings =
{
    trafficControlService: new MqttTrafficControlService('127.0.0.1', 9001),
    //trafficControlService: new HttpTrafficControlService('http://127.0.0.1:5000'),
    laneCount: 5,
    carCount: 15,
    carTypes:
        [
            // Speed demon
            {
                initialSpeed:
                {
                    minimum: 120,
                    maximum: 130
                },
                maximumSpeed: 180,
                acceleration: 150,
                maxSafeDistance: 2,
                brakeThreshold: 2,
                mergeBehavior: new AggressiveMergeBehavior(),
                lanes: [0, 1, 2],
                overtakeOnRightSide: true,
                sourceImages: [
                    { key: 'hummer', paintKey: 'paint-general' }
                ],
                selectionWeight: 5
            },
            // Regular cars
            {
                initialSpeed:
                {
                    minimum: 90,
                    maximum: 110
                },
                maximumSpeed: 110,
                acceleration: 100,
                maxSafeDistance: 4,
                brakeThreshold: 4,
                sourceImages: [
                    { key: 'ambulance' },
                    { key: 'crossover', paintKey: 'paint-general' },
                    { key: 'mini', paintKey: 'paint-general' },
                    { key: 'minivan', paintKey: 'paint-general' },
                    { key: 'pickup', paintKey: 'paint-general' },
                    { key: 'sedan', paintKey: 'paint-general' },
                    { key: 'semitruck' },
                ],
                selectionWeight: 20
            },
            // Trucks & buses
            {
                initialSpeed: {
                    minimum: 60,
                    maximum: 85
                },
                maximumSpeed: 85,
                acceleration: 50,
                maxSafeDistance: 4,
                brakeThreshold: 4,
                lanes: [3, 4],
                sourceImages: [
                    { key: 'garbage' },
                    { key: 'schoolbus' },
                    { key: 'truck' }
                ],
                selectionWeight: 10
            }
        ],
    maximumSpeedIncrementForLeftLanes: 5
}