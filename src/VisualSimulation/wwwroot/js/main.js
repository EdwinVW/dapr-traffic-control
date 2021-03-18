import { DebugScene } from './debugscene.js';
import { TrafficScene } from './trafficscene.js';

var phaserConfig = {
    type: Phaser.WEBGL,
    width: 1280,
    height: 600,
    backgroundColor: '#336023',
    parent: 'phaser',
    pixelArt: true,
    physics: {
        default: 'arcade',
        arcade: { debug: false }
    },
    scale: {
        mode: Phaser.Scale.FIT,
        autoCenter: Phaser.Scale.CENTER_HORIZONTALLY
    },
    scene: [ TrafficScene, DebugScene ]
};

new Phaser.Game(phaserConfig);