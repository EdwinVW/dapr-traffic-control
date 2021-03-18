export class Merge {
    
    constructor(car, from, to) {
        this.car = car;
        this.from = from;
        this.to = to;
    }

    isSafe() {

        // Don't want to cut off the car in the other lane.
        var previousCar = this.to.getPreviousCarFromPosition(this.car.body.x);
        if (previousCar
            && (previousCar.getDistanceTo(this.car) < this.car.getSafeDistance())) {
            return false;
        }
        
        var nextCar = this.to.getNextCarFromPosition(this.car.body.x);
        if (nextCar)
        {
            // Don't want to crash into the car in front of us in the other lane.
            if (this.car.getDistanceTo(nextCar) < this.car.getSafeDistance()) {
                return false;
            }
        }
        
        return true;
    }

    continueMerge() {

        if (this.to.number > this.from.number) {

            if (this.car.y >= this.to.getMiddleOfLanePosition()) {

                this.car.setAccelerationY(0);
                this.car.setVelocityY(0);
                this.car.setPosition(this.car.x, this.to.getMiddleOfLanePosition());

                this.from.removeCar(this.car);
                this.car.lane = this.to;

                return true;
            } else {
                this.car.setAccelerationY(this.car.body.velocity.x / 2);
                return false;
            }
        } else {

            if (this.car.y <= this.to.getMiddleOfLanePosition()) {
                this.car.setAccelerationY(0);
                this.car.setVelocityY(0);
                this.car.setPosition(this.car.x, this.to.getMiddleOfLanePosition());

                this.from.removeCar(this.car);
                this.car.lane = this.to;

                return true;
            } else {
                this.car.setAccelerationY(-this.car.body.velocity.x / 2);
                return false;
            }

        }
    }
}