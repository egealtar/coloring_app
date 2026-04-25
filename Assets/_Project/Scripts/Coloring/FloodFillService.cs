using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OtsimoColoring.Coloring
{
    /// <summary>
    /// Performs a non-blocking flood fill on the working texture of a CanvasController.
    /// Uses an iterative scanline algorithm to keep memory low and yields every N pixels
    /// to avoid blocking the main thread for more than one frame.
    ///
    /// Outline checks are performed against the SOURCE texture so re-filling a previously
    /// colored region cannot leak through outlines.
    /// </summary>
    public class FloodFillService : MonoBehaviour
    {
        public bool IsFilling { get; private set; }

        /// <summary>
        /// Begin a flood fill from the given pixel with the given color.
        /// onPixelsFilled is invoked with the count of newly painted (previously
        /// uncolored) pixels — used by completion detection.
        /// </summary>
        public IEnumerator FillCoroutine(
            CanvasController canvas,
            int startX,
            int startY,
            Color32 fillColor,
            System.Action<int> onPixelsFilled)
        {
            if (IsFilling)
            {
                yield break;
            }
            IsFilling = true;

            int width = canvas.Width;
            int height = canvas.Height;
            int pixelsPerFrame = canvas.FillConfig != null
                ? canvas.FillConfig.PixelsPerFrame
                : 4000;
            float tolerance = canvas.FillConfig != null
                ? canvas.FillConfig.ColorTolerance
                : 0.1f;

            // Outline pixels must never be filled. The source is the source of truth.
            if (canvas.IsOutlinePixel(startX, startY))
            {
                IsFilling = false;
                yield break;
            }

            // Reference color: what the user is replacing. Read from the *working*
            // texture because the user might be re-coloring an already-painted region.
            Color32 targetColor = canvas.GetWorkingPixel(startX, startY);

            // No-op if user taps a pixel that already has the chosen color.
            if (ColorsClose(targetColor, fillColor, tolerance))
            {
                IsFilling = false;
                yield break;
            }

            int newlyFilled = 0;
            int processedThisFrame = 0;

            // Scanline flood fill: push horizontal spans and walk up/down from each.
            var stack = new Stack<int>();
            stack.Push(startX);
            stack.Push(startY);

            while (stack.Count > 0)
            {
                int y = stack.Pop();
                int x = stack.Pop();

                // Walk left to find span start.
                int xLeft = x;
                while (xLeft >= 0 && CanFill(canvas, xLeft, y, targetColor, tolerance))
                {
                    xLeft--;
                }
                xLeft++;

                bool spanAbove = false;
                bool spanBelow = false;

                // Walk right, painting the span.
                int xRight = xLeft;
                while (xRight < width && CanFill(canvas, xRight, y, targetColor, tolerance))
                {
                    // Count "newly filled" only if the working pixel was the original
                    // line-art color (i.e., not previously painted).
                    if (IsUnpainted(canvas, xRight, y))
                    {
                        newlyFilled++;
                    }

                    canvas.SetWorkingPixel(xRight, y, fillColor);
                    processedThisFrame++;

                    // Check pixel directly above.
                    if (y + 1 < height)
                    {
                        bool fillableAbove = CanFill(canvas, xRight, y + 1, targetColor, tolerance);
                        if (!spanAbove && fillableAbove)
                        {
                            stack.Push(xRight);
                            stack.Push(y + 1);
                            spanAbove = true;
                        }
                        else if (spanAbove && !fillableAbove)
                        {
                            spanAbove = false;
                        }
                    }

                    // Check pixel directly below.
                    if (y - 1 >= 0)
                    {
                        bool fillableBelow = CanFill(canvas, xRight, y - 1, targetColor, tolerance);
                        if (!spanBelow && fillableBelow)
                        {
                            stack.Push(xRight);
                            stack.Push(y - 1);
                            spanBelow = true;
                        }
                        else if (spanBelow && !fillableBelow)
                        {
                            spanBelow = false;
                        }
                    }

                    xRight++;

                    if (processedThisFrame >= pixelsPerFrame)
                    {
                        canvas.ApplyChanges();
                        processedThisFrame = 0;
                        yield return null;
                    }
                }
            }

            canvas.ApplyChanges();
            onPixelsFilled?.Invoke(newlyFilled);
            IsFilling = false;
        }

        private static bool CanFill(
            CanvasController canvas,
            int x,
            int y,
            Color32 targetColor,
            float tolerance)
        {
            // Never cross the outline — checked against the *source* texture.
            if (canvas.IsOutlinePixel(x, y))
            {
                return false;
            }

            // Only repaint pixels that match the current target color in the working buffer.
            var current = canvas.GetWorkingPixel(x, y);
            return ColorsClose(current, targetColor, tolerance);
        }

        private static bool IsUnpainted(CanvasController canvas, int x, int y)
        {
            // A pixel is "unpainted" if the working buffer still matches the source.
            var src = canvas.GetSourcePixel(x, y);
            var work = canvas.GetWorkingPixel(x, y);
            return src.r == work.r && src.g == work.g && src.b == work.b && src.a == work.a;
        }

        private static bool ColorsClose(Color32 a, Color32 b, float tolerance)
        {
            float dr = (a.r - b.r) / 255f;
            float dg = (a.g - b.g) / 255f;
            float db = (a.b - b.b) / 255f;
            // Tolerance is per-channel max distance; this is a simple but effective check.
            return Mathf.Abs(dr) <= tolerance
                && Mathf.Abs(dg) <= tolerance
                && Mathf.Abs(db) <= tolerance;
        }
    }
}