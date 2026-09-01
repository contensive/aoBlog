
using System;

namespace Contensive.Blog.Controllers {
    public sealed class ImageController {
        //
        // ====================================================================================================
        /// <summary>
        /// Calculate height based on width and aspect ratio
        /// </summary>
        /// <param name="holeWidthPx">The desired width in pixels</param>
        /// <param name="imageAspectRatioId">The aspect ratio ID (1=As-Is, 2=1:1, 3=3:2, 4=4:3, 5=16:9, 6=2:1)</param>
        /// <returns>Calculated height in pixels, or 0 if As-Is</returns>
        public static int getImageHeight(int holeWidthPx, int imageAspectRatioId) {
            double aspectRatio = getAspectRatio(imageAspectRatioId);
            if (imageAspectRatioId == 0 || aspectRatio <= 0) {
                return 0;  // As-Is, no resize
            }
            return (int)Math.Round(holeWidthPx / aspectRatio);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Get aspect ratio CSS class for styling
        /// </summary>
        /// <param name="imageAspectRatioId">The aspect ratio ID</param>
        /// <returns>CSS class name for aspect ratio container</returns>
        public static string getAspectRatioStyle(int imageAspectRatioId) {
            switch (imageAspectRatioId) {
                case 2:
                    return "blogImageAspect-1-1";
                case 3:
                    return "blogImageAspect-3-2";
                case 4:
                    return "blogImageAspect-4-3";
                case 5:
                    return "blogImageAspect-16-9";
                case 6:
                    return "blogImageAspect-2-1";
                case 7:
                    return "blogImageAspect-3-1";
                case 8:
                    return "blogImageAspect-4-1";
                case 9:
                    return "blogImageAspect-5-1";
                default:
                    return string.Empty;
            }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Get numeric aspect ratio value
        /// </summary>
        /// <param name="imageAspectRatioId">The aspect ratio ID</param>
        /// <returns>Numeric aspect ratio (width/height), or -1 for As-Is</returns>
        private static double getAspectRatio(int imageAspectRatioId) {
            switch (imageAspectRatioId) {
                case 1:
                    return -1;      // As-Is
                case 2:
                    return 1.0;     // 1:1 (Square)
                case 3:
                    return 1.5;     // 3:2 (Classic)
                case 4:
                    return 1.333;   // 4:3 (Standard)
                case 5:
                    return 1.778;   // 16:9 (Widescreen)
                case 6:
                    return 2.0;     // 2:1 (Panoramic)
                case 7:
                    return 3.0;     // 3:1
                case 8:
                    return 4.0;     // 4:1
                case 9:
                    return 5.0;     // 5:1
                default:
                    return -1;      // Treat unknown as As-Is
            }
        }
    }
}
