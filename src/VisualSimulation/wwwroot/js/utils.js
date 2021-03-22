export const Utils =
{
    tzoffset: (new Date()).getTimezoneOffset() * 60000, // timezone offset in milliseconds

    getRandomInteger: function (min, max) {
        return Math.floor(Math.random() * (max - min + 1)) + min;
    },

    pixelsToMeters: function (pixels) {
        // 10 pixels = 1m
        return pixels / 10;
    },

    pixelsToKilometers: function (pixels) {
        return Utils.pixelsToMeters(pixels) / 1000;
    },

    velocityToKilometersPerHour: function (velocity) {
        return Utils.pixelsToKilometers(velocity * 3600);
    },

    metersToPixels: function (meters) {
        // 1m = 10 pixels
        return meters * 10;
    },

    kilometersToPixels: function (kilometers) {
        return Utils.metersToPixels(kilometers) * 1000;
    },

    kilometersPerHourToVelocity: function (kmPerHour) {
        return Utils.kilometersToPixels(kmPerHour) / 3600;
    },

    localTime: function () {
        return (new Date(Date.now() - this.tzoffset)).toISOString().slice(0, -1);
    }
}