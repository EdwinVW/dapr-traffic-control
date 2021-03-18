import { Utils } from './utils.js';

export let Car = new Phaser.Class({

    Extends: Phaser.Physics.Arcade.Image,

    initialize: function Car(scene) {

        Phaser.Physics.Arcade.Image.call(this, scene, 0, 0, 'car');

        this.setOrigin(0, 1);
    },

    update: function () {
        this.setDepth(this.lane.number + 10);

        this.adjustSpeed();

        if (this.merge) {
            if (this.merge.continueMerge()) {
                this.merge = null;
            }
        } else if (this.settings.mergeBehavior) {
            this.merge = this.settings.mergeBehavior.tryMerge(this);
        }

        // Check if we're at the end of the road.
        if (this.x > this.scene.physics.world.bounds.right) {
            this.onExit();
        }
    },

    onEntry: function (licensePlate, lane, settings, trafficControlService) {
        this.id = licensePlate;
        this.lane = lane;
        this.lane.addNewCar(this);
        this.settings = settings;
        this.trafficControlService = trafficControlService;
        this.merge = null;
        
        if (this.trafficControlService) {
            this.trafficControlService.registerVehicleEntry(this);
        }

        this.setTexture(settings.imageKeys[Utils.getRandomInteger(0, settings.imageKeys.length - 1)]);
        this.setBodySize(this.displayWidth, this.displayHeight);
        this.body.reset(-this.displayWidth, lane.getMiddleOfLanePosition());

        // Pick some random start speed.
        const initialSpeed = Utils.kilometersPerHourToVelocity(
            Utils.getRandomInteger(settings.initialSpeed.minimum, settings.initialSpeed.maximum));
        this.setVelocityX(initialSpeed);
            
        this.setActive(true);
        this.setVisible(true);
    },

    onExit: function () {
        this.lane.removeCar(this);
        if (this.merge) {
            this.merge.to.removeCar(this);
        }
        this.merge = null;

        this.setActive(false);
        this.setVisible(false);
        this.body.stop();

        if (this.trafficControlService) {
            this.trafficControlService.registerVehicleExit(this);
        }
    },

    adjustSpeed: function () {

        let accelerationX;
        let velocityX = this.body.velocity.x;

        const nextCar = this.getCarInFront();
        if (nextCar) {

            var safeDistance = this.getSafeDistance();
            var distanceToNextCar = this.getDistanceTo(nextCar);
            var distanceToSafePosition = distanceToNextCar - safeDistance;
            var relativeVelocityToNextCar = velocityX - nextCar.body.velocity.x;

            // Check if we're too close to the next car.
            if (distanceToSafePosition < 0) {

                // If we're going faster than the next car, stop doing that by matching the
                // next car's speed...
                if (relativeVelocityToNextCar > 0) {
                    velocityX = nextCar.body.velocity.x;
                }

                // ...and brake to increase distance.
                accelerationX = Math.min(-50, nextCar.body.acceleration.x);
            }
            // We're not too close yet, but gaining.
            else if (relativeVelocityToNextCar > 0) {

                // If the distance is less than the safe distance times the braking threshold, start braking.
                if (distanceToSafePosition < safeDistance * this.settings.brakeThreshold)
                {
                    // Calculate the time it takes to reach the minimum safe distance.
                    var timeToSafePosition = distanceToSafePosition / relativeVelocityToNextCar;

                    // Brake with the appropriate force.
                    accelerationX = -(relativeVelocityToNextCar / timeToSafePosition);

                    // If the next car is braking as well, add that to the acceleration.
                    if (nextCar.body.acceleration.x < 0) {
                        accelerationX += nextCar.body.acceleration.x;
                    }
                } 
            }
        }

        // If no braking manoeuvre has started, we can safely go a bit faster.
        if (accelerationX === undefined) {
            accelerationX = this.settings.acceleration;
        }

        // Never go over the driver's maximum speed, though.
        const maximumVelocity = this.getMaximumVelocity();
        if (velocityX >= maximumVelocity) {
            accelerationX = Math.min(accelerationX, 0);
            velocityX = maximumVelocity;
        }

        // Never move backwards.
        if (velocityX < 0) {
            accelerationX = 0;
            velocityX = 0;
        }

        this.setVelocityX(velocityX);
        this.setAccelerationX(accelerationX);
    },

    isInFrontOf: function (car) {
        return this.body.x > car.body.x;
    },

    getDistanceTo: function (car) {
        if (this.isInFrontOf(car)) {
            return this.x - car.x - car.displayWidth;
        } else {
            return car.x - this.x - this.displayWidth;
        }
    },

    getSafeDistance: function () {
        let safeDistanceInMeters = Utils.velocityToKilometersPerHour(this.body.velocity.x) / 4;
        if (this.settings.maxSafeDistance && safeDistanceInMeters > this.settings.maxSafeDistance) {
            safeDistanceInMeters = this.settings.maxSafeDistance;
        }
       return Math.ceil(Utils.metersToPixels(safeDistanceInMeters));
    },

    getMaximumVelocity: function() {
        let result = this.settings.maximumSpeed;
        result += this.lane.getAdditionalKilometersPerHourForMaximumSpeed();
        result = Utils.kilometersPerHourToVelocity(result);
        return result;
    },

    getCarInFront: function () {
        let result = this.lane.getNextCar(this);

        if (this.merge) {
            var carInMergeLane = this.merge.to.getNextCar(this);
            if (carInMergeLane) {
                if (!result || result.isInFrontOf(carInMergeLane)) {
                    result = carInMergeLane;
                }
            }
        }
        // If we don't want to overtake on right side, check if there's a car
        // closer to us in the left lane.
        else if (this.lane.number > 0 && !this.settings.overtakeOnRightSide) {
            var carInLeftLane = this.lane.lanes[this.lane.number - 1].getNextCarFromPosition(
                this.body.x + this.displayWidth);
            if (carInLeftLane) {
                if (!result || result.isInFrontOf(carInLeftLane)) {
                    result = carInLeftLane;
                }
            }
        }


        return result;
    },

    getCarBehind: function () {
        let result = this.lane.getPreviousCar(this);

        if (this.mergeRightLane) {
            var carInRightLane = this.mergeRightLane.getPreviousCar(this);
            if (carInRightLane) {
                if (!result || carInRightLane.inFrontOf(result)) {
                    result = carInRightLane;
                }
            }
        }

        return result;
    },

    getDebugInfo: function (indentation) {
        const result =
        {
            accelerationX: this.body.acceleration.x,
            displayWidth: this.displayWidth,
            image: this,
            position: this.lane.getCarIndex(this),
            safeDistance: this.getSafeDistance() + ' (' + Utils.pixelsToMeters(this.getSafeDistance()) + ' m)',
            velocityX: this.body.velocity.x + ' (' + Utils.velocityToKilometersPerHour(this.body.velocity.x) + ' km/h)',
            x: this.x
        };

        var lane = this.lane.number;
        if (this.merge) {
            lane += ' (merging into lane ' + this.merge.to.number + ' at position ' + this.merge.to.getCarIndex(this) + ')';
        }
        result.lane = lane;

        var carInFront = this.getCarInFront();
        if (carInFront) {

            let carInFrontText = carInFront.id
                + ' (distance ' + this.distanceTo(carInFront)
                + ', time ' + this.timeTo(carInFront)
                + ', lane ' + carInFront.lane.number;

            if (carInFront.merge) {
                carInFrontText += ', merging into lane ' + carInFront.merge.to.number;
            }

            result.carInFront = carInFrontText + ')';
        };

        var carBehind = this.getCarBehind();
        if (carBehind) {
            let carBehindText = carBehind.id + ' ('
                + Math.floor(Utils.pixelsToMeters(this.distanceTo(carBehind))) + 'm, lane '
                + carBehind.lane.number;

            if (carBehind.merge) {
                carBehindText += ', merging into lane ' + carBehind.merge.to.number;
            }

            result.carBehind = carBehindText + ')';
        };

        if (this.lane.number < this.lane.lanes.length - 1) {
            var rightLanePrevCar = this.lane.lanes[this.lane.number + 1].getPreviousCarFromPosition(this.body.x);
            if (rightLanePrevCar) {
                result.rightLanePrevCar = rightLanePrevCar.id;
            }
        
        }

        if (this.merge && this.merge.mergeStatus) {
            result.mergeStatus = this.mergeStatus;
        }

        return result;
    }
});