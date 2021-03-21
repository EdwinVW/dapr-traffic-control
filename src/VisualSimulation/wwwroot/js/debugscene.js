export let DebugScene = new Phaser.Class({

    Extends: Phaser.Scene,

    initialize: function DebugScene() {
            Phaser.Scene.call(this, { key: 'debugScene' });
        },

    create: function () {
        console.log('Entering debug mode');

        const trafficScene = this.game.scene.getScene('trafficScene');
        const cars = trafficScene.cars.children.entries;

        for (let lane of trafficScene.lanes) {
            console.log('lane ' + lane.number + ' cars:');
            for (let laneCar of lane.cars) {
                console.log('   ' + laneCar.id + ' : ' + laneCar.body.x);
            }
        }

        for (let car of cars) {

            var style = { font: "14px Arial", fill: "Black", wordWrap: true, align: "center", backgroundColor: "#ffff00" };
            const label = this.add.text(car.x + car.displayWidth / 2, car.y - car.displayHeight / 2, car.id, style);
            label.setOrigin(0.5);
        }

        this.input.keyboard.on('keydown-D', () => {
            this.scene.resume('trafficScene');
            this.scene.stop();
        }, this);

        this.input.on('pointerup', function (pointer) {
            const x = pointer.upX;
            const y = pointer.upY;

            for (let car of cars) {
                if (x >= car.body.x && x <= (car.body.x + car.displayWidth)
                    && y >= car.body.y && y <= (car.body.y + car.displayHeight)) {
                    console.log('🚘 Debug info for car ' + car.id + ':\n%O', car.getDebugInfo());
                }
            }
        }, this);
    }

});