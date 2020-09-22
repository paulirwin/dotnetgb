

const drawCanvasPixels = (function () {
    let context = null;

    document.addEventListener("DOMContentLoaded", function() {
        const canvas = document.getElementById("display");

        if (!canvas || !canvas.getContext) {
            return;
        }

        canvas.width = 160;
        canvas.height = 144;
        context = canvas.getContext("2d");
    });

    return function(pixels) {
        if (!context) {
            return;
        }

        context.putImageData(pixels, 0, 0);
    }
})();