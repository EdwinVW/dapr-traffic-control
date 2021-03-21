import { Merge } from './merge.js';
import { Settings } from './settings.js';

export const laneMarginTop = 400;
export const laneHeight = 33;

export class Lane extends Phaser.GameObjects.TileSprite {

    constructor(scene, number, lanes) {
        super(scene,
            -(number * 20),
            laneMarginTop + (number * laneHeight),
            scene.physics.world.bounds.width + (number * 40),
            laneHeight,
            'road-middle');

        this.number = number;
        this.lanes = lanes;
        this.cars = [];
        this.setOrigin(0, 0);
        this.setDepth(1);

        scene.add.existing(this);
    }

    hasRoomForNewCar() {
        if (this.cars.length > 0) {
            const lastCar = this.cars[0];
            if (lastCar.body.x > 150) {
                return true;
            }
        }

        return this.cars.length == 0;
    }

    getAdditionalKilometersPerHourForMaximumSpeed() {
        return (this.lanes.length - this.number - 1) * Settings.maximumSpeedIncrementForLeftLanes;
    }

    addNewCar(car) {
        this.cars.unshift(car);
    }

    hasLeftLane() {
        return this.getLeftLane() !== null;
    }

    hasRightLane() {
        return this.getRightLane() !== null;
    }

    getLeftLane() {
        if (this.number > 0) {
            return this.lanes[this.number - 1];
        }
        return null;
    }

    getRightLane() {
        if (this.number < this.lanes.length - 1) {
            return this.lanes[this.number + 1];
        }
        return null;
    }

    tryMerge(car, to) {

        var merge = new Merge(car, this, to);

        if (merge.isSafe()) {

            // Add car to merge lane so that other vehicles won't crash into it.
            merge.to.insertCar(merge.car);
            return merge;
        }

        return null;
    }

    insertCar(car) {
        var index = car.lane.getCarIndex(car);
        console.assert(index >= 0, "Merging car should have index in own lane");

        for (let i = 0; i < this.cars.length; i++) {
            if (this.cars[i].isInFrontOf(car)) {
                this.cars.splice(i, 0, car);
                return;
            }
        }

        this.cars.push(car);
    }

    removeCar(car) {
        const index = this.getCarIndex(car);
        if (index >= 0) {
            this.cars.splice(index, 1);
        }
    }

    getCarIndex(car) {
        return this.cars.findIndex(c => c === car);
    }

    getNextCar(car) {
        const index = this.getCarIndex(car);
        if (this.cars.length >= index + 1) {
            return this.cars[index + 1];
        }
        return null;
    }

    getPreviousCar(car) {
        const index = this.getCarIndex(car);
        if (index > 0) {
            return this.cars[index - 1];
        }
        return null;
    }

    getNextCarFromPosition(x) {
        for (let i = 0; i < this.cars.length; i++) {
            if (this.cars[i].body.x > x) {
                return this.cars[i];
            }
        }
        return null;
    }

    getPreviousCarFromPosition(x) {
        for (let i = this.cars.length - 1; i >= 0; i--) {
            if (this.cars[i].body.x < x) {
                return this.cars[i];
            }
        }
        return null;
    }

    getMiddleOfLanePosition() {
        return this.y + Math.floor(this.height / 2) + 8;
    }
}
