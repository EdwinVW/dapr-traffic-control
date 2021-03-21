export class AggressiveMergeBehavior {
    
    tryMerge(car) {

        // If we're at maximum speed or still accelerating, just continue.
        if (car.body.velocity.x >= car.getMaximumVelocity() || car.body.acceleration.x > 0) {
            return null;
        }

        let furthestCar = car.getCarInFront();
        if (furthestCar) {

            if (furthestCar.settings.mergeBehavior === this) {
                return null;
            }

            const leftLane = car.lane.getLeftLane();
            if (leftLane) {
                const nextCarOnLeft = leftLane.getNextCarFromPosition(car.body.x);
                if (nextCarOnLeft && nextCarOnLeft.isInFrontOf(furthestCar)) {
                    furthestCar = nextCarOnLeft;
                }
            }

            const rightLane = car.lane.getRightLane();
            if (rightLane) {
                const nextCarOnRight = rightLane.getNextCarFromPosition(car.body.x);
                if (nextCarOnRight && nextCarOnRight.isInFrontOf(furthestCar)) {
                    furthestCar = nextCarOnRight;
                }
            }

            if (furthestCar.lane !== car.lane) {
                return car.lane.tryMerge(car, furthestCar.lane);
            }
        }

        return null;
    }
}