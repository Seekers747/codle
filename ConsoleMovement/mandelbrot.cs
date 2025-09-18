using System;
using SkiaSharp;
using System.IO;

class Mandelbrot
{
    public static void Run()
    {
        // Image dimensions
        int width = 800;
        int height = 600;

        // Determines the detail level of each pixel
        int maxAttempts = 10000;

        // Create a bitmap to draw on
        var canvas = new SKBitmap(width, height);

        // Set the minimum and maximum values for the real and imaginary parts
        // Everything outside this range will be colored black
        float minReal = -2.5f, maxReal = 1.0f;
        float minImag = -1.0f, maxImag = 1.0f;

        // Loop over each pixel in the image
        for (float px = 0; px < width; px++)
        {
            for (float py = 0; py < height; py++)
            {
                // Determine the total width and height of the rectangle in the complex plane
                // "realspan" means the width of the rectangle and "imagspan" means the height
                float realSpan = maxReal - minReal;
                float imagSpan = maxImag - minImag;

                // Get the percentage (as a fraction) across the image that this pixel is
                float xFraction = px / width;
                float yFraction = py / height;

                // Convert the fraction into an actual distance by multiplying by the span
                float xOffset = xFraction * realSpan;
                float yOffset = yFraction * imagSpan;

                // Add the offset to the minimum to get the actual coordinate of this pixel
                float x0 = minReal + xOffset;
                float y0 = minImag + yOffset;
                // Now we have the coordinate in the complex plane for this pixel

                // Assign initial values for x and y and the attempt counter
                float x = 0, y = 0;
                int attempt = 0;

                // x * x + y * y basically means "the distance from the origin" but is written as the magnitude squared rather than computing a square root
                // 4 is used because if the distance from the origin is greater than 2, because 2 is the square root of 4 it means the magnitude squared is greater than 4
                // x * x + y * y <= 4 means that we havent escaped yet if we are still within a distance of 2 from the origin
                // We also limit the number of attempts to avoid infinite loops
                while (x * x + y * y <= 4 && attempt < maxAttempts)
                {
                    // square the complex number (x + yi) and add the original coordinate (x0 + y0i)
                    float xtemp = x * x - y * y + x0;

                    // Becomes the new y value after squaring and adding the imaginery part (y0)
                    y = 2 * x * y + y0;

                    // Becomes the new x value after squaring and adding the real part (x0)
                    x = xtemp;
                    // Now x and y represent the new complex number for the next attempt

                    // Increment the attempt counter this is used to color the pixel later
                    attempt++;
                }

                // Each pixel is colored based on how many attempts it took to escape
                // If it took the maximum number of attempts, we color it black
                // If it escaped sooner(attempt is low), we color it blue
                // If it escaped later(attempt is high), we color it red

                // The fraction of attempts is multiplied by 255 to get a value between 0 and 255 and converted to a byte
                // Green is always 0 so there is no green in the image
                // Red increases as the number of attempts increases
                // Blue decreases as the number of attempts increases
                byte r = (byte)(attempt * 255 / maxAttempts);
                byte g = 0;
                byte b = (byte)(255 - (attempt * 255 / maxAttempts));

                // If the point is in the Mandelbrot set (did not escape), color it black
                // Otherwise, use the calculated color based on the previous logic
                SKColor color = attempt == maxAttempts ? SKColors.Black : new SKColor(r, g, b);
                canvas.SetPixel((int)px, (int)py, color);
            }
        }

        // Save as PNG
        using var image = SKImage.FromBitmap(canvas);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite("mandelbrot.png");
        data.SaveTo(stream);

        Console.WriteLine("Mandelbrot image saved as mandelbrot.png!");
    }
}