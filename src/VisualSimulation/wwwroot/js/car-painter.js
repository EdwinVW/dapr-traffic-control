import { Utils } from './utils.js';

export class CarPainter {
    
    constructor(scene)
    {
        this.scene = scene;
    }

    paintCar(carSettings) {
        const imageKeys = [];
        for (let sourceImage of carSettings.sourceImages) {
            this.paintCarImage(sourceImage, imageKeys);
        }
        return imageKeys;
    }

    // The code in this function is based on the Phaser 3 palette swapping example by Colbydude.
    // See: https://github.com/Colbydude/phaser-3-palette-swapping-example
    paintCarImage(sourceImage, imageKeys) {

        if (sourceImage.paintKey) {

            // Create color lookup from palette image.
            var colorLookup = [];
            var x, y;
            var pixel, palette;
            var paletteImage = this.scene.game.textures.get(sourceImage.paintKey).getSourceImage();
            
            // Go through each pixel in the palette image and add it to the color lookup.
            for (y = 0; y < paletteImage.height; y++) {

                colorLookup[y] = [];
        
                for (x = 0; x < paletteImage.width; x++) {
                    pixel = this.scene.game.textures.getPixel(x, y, sourceImage.paintKey);
                    colorLookup[y].push(pixel);
                }
            }

            // Iterate over each palette.
            for (y = 1; y < colorLookup.length; y++) {

                const sheet = this.scene.game.textures.get(sourceImage.key).getSourceImage();
                const palette = colorLookup[y];

                // Create a canvas to draw new image data onto.
                const canvasTexture = this.scene.game.textures.createCanvas(sourceImage.key + '-temp', sheet.width, sheet.height);
                const canvas = canvasTexture.getSourceImage();
                const context = canvas.getContext('2d');

                // Copy the sheet.
                context.drawImage(sheet, 0, 0);

                // Get image data from the new sheet.
                var imageData = context.getImageData(0, 0, sheet.width, sheet.height);
                var pixelArray = imageData.data;
            
                // Iterate through every pixel in the image.
                for (var p = 0; p < pixelArray.length / 4; p++) {
                    var index = 4 * p;

                    var r = pixelArray[index];
                    var g = pixelArray[++index];
                    var b = pixelArray[++index];
                    var alpha = pixelArray[++index];

                    // If this is a transparent pixel, ignore, move on.
                    if (alpha === 0) {
                        continue;
                    }

                    // Iterate through the colors in the palette.
                    for (var c = 0; c < palette.length; c++) {
                        var oldColor = colorLookup[0][c];
                        var newColor = colorLookup[y][c];

                        // If the color matches, replace the color.
                        if (r === oldColor.r && g === oldColor.g && b === oldColor.b && alpha === 255) {
                            pixelArray[--index] = newColor.b;
                            pixelArray[--index] = newColor.g;
                            pixelArray[--index] = newColor.r;
                        }
                    }
                }

                // Put our modified pixel data back into the context.
                context.putImageData(imageData, 0, 0);

                // Add the canvas as a sprite sheet to the game.
                this.scene.game.textures.addSpriteSheet(sourceImage.key + '-' + y, canvasTexture.getSourceImage(), {
                    frameWidth: sheet.width,
                    frameHeight: sheet.height,
                });

                // Destroy temp texture.
                this.scene.game.textures.get(sourceImage.key + '-temp').destroy();
                
                imageKeys.push(sourceImage.key + '-' + y);//sourceImage.key);
            }
        }

        imageKeys.push(sourceImage.key);
    }
}