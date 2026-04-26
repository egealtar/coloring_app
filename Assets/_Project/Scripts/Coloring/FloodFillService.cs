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
    /// After the main fill, a small dilation pass extends the painted area into
    /// anti-aliased edge pixels that the strict color-equality check skipped, so
    /// the result reaches up to the outline without leaking past it. Outline checks
    /// are always performed against the SOURCE texture so re-filling a previously
    /// colored region cannot leak through outlines.
    /// </summary>
    public class FloodFillService : MonoBehaviour
    {
        public bool IsFilling { get; private set; }

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

            if (canvas.IsOutlinePixel(startX, startY))
            {
                IsFilling = false;
                yield break;
            }

            Color32 targetColor = canvas.GetWorkingPixel(startX, startY);

            if (ColorsClose(targetColor, fillColor, tolerance))
            {
                IsFilling = false;
                yield break;
            }

            int newlyFilled = 0;
            int processedThisFrame = 0;

            // ---- Pass 1: scanline flood fill on exact-match pixels ----
            var stack = new Stack<int>();
            stack.Push(startX);
            stack.Push(startY);

            // Track which pixels were touched this fill, for the dilation pass.
            var filledMask = new bool[width * height];

            while (stack.Count > 0)
            {
                int y = stack.Pop();
                int x = stack.Pop();

                int xLeft = x;
                while (xLeft >= 0 && CanFill(canvas, xLeft, y, targetColor, tolerance))
                {
                    xLeft--;
                }
                xLeft++;

                bool spanAbove = false;
                bool spanBelow = false;

                int xRight = xLeft;
                while (xRight < width && CanFill(canvas, xRight, y, targetColor, tolerance))
                {
                    if (IsUnpainted(canvas, xRight, y))
                    {
                        newlyFilled++;
                    }

                    canvas.SetWorkingPixel(xRight, y, fillColor);
                    filledMask[y * width + xRight] = true;
                    processedThisFrame++;

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

            // ---- Pass 2: dilate into anti-aliased edge pixels ----
            // Any non-outline pixel adjacent to the freshly filled region that was
            // not painted (because its source color didn't strictly match) gets the
            // fill color too. This closes 1px gaps left by AA without bleeding past
            // the real outline (we still respect IsOutlinePixel against the source).
            const int dilationPasses = 2;
            for (int pass = 0; pass < dilationPasses; pass++)
            {
                var newlyDilated = new List<int>();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * width + x;
                        if (filledMask[idx]) continue;
                        if (canvas.IsOutlinePixel(x, y)) continue;

                        // Has any 4-connected neighbor been filled by us?
                        if (HasFilledNeighbor(filledMask, x, y, width, height))
                        {
                            if (IsUnpainted(canvas, x, y))
                            {
                                newlyFilled++;
                            }
                            canvas.SetWorkingPixel(x, y, fillColor);
                            newlyDilated.Add(idx);
                            processedThisFrame++;

                            if (processedThisFrame >= pixelsPerFrame)
                            {
                                canvas.ApplyChanges();
                                processedThisFrame = 0;
                                yield return null;
                            }
                        }
                    }
                }

                foreach (var idx in newlyDilated)
                {
                    filledMask[idx] = true;
                }

                if (newlyDilated.Count == 0)
                {
                    break;
                }
            }

            canvas.ApplyChanges();
            onPixelsFilled?.Invoke(newlyFilled);
            IsFilling = false;
        }

        private static bool HasFilledNeighbor(bool[] mask, int x, int y, int w, int h)
        {
            if (x > 0 && mask[y * w + (x - 1)]) return true;
            if (x < w - 1 && mask[y * w + (x + 1)]) return true;
            if (y > 0 && mask[(y - 1) * w + x]) return true;
            if (y < h - 1 && mask[(y + 1) * w + x]) return true;
            return false;
        }

        private static bool CanFill(
            CanvasController canvas,
            int x,
            int y,
            Color32 targetColor,
            float tolerance)
        {
            if (canvas.IsOutlinePixel(x, y))
            {
                return false;
            }

            var current = canvas.GetWorkingPixel(x, y);
            return ColorsClose(current, targetColor, tolerance);
        }

        private static bool IsUnpainted(CanvasController canvas, int x, int y)
        {
            var src = canvas.GetSourcePixel(x, y);
            var work = canvas.GetWorkingPixel(x, y);
            return src.r == work.r && src.g == work.g && src.b == work.b && src.a == work.a;
        }

        private static bool ColorsClose(Color32 a, Color32 b, float tolerance)
        {
            float dr = (a.r - b.r) / 255f;
            float dg = (a.g - b.g) / 255f;
            float db = (a.b - b.b) / 255f;
            return Mathf.Abs(dr) <= tolerance
                && Mathf.Abs(dg) <= tolerance
                && Mathf.Abs(db) <= tolerance;
        }
    }
}