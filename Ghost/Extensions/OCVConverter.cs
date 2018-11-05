using System;
using OCV = OpenCvSharp;
using Scalar = OpenCvSharp.Scalar;
using Vec3b = OpenCvSharp.Vec3b;

namespace Ghost.Extensions
{
    public static class OCVConverter
    {
        public static Scalar ToScalar(in this Vec3b src, byte alpha = 0xFF)
        {
            return new OCV.Scalar(src[0], src[1], src[2], alpha);
        }
        public static Scalar ToScalar(in this uint src)
        {
            return new OCV.Scalar(
                    0xFF & (src >> 24),
                    0xFF & (src >> 16),
                    0xFF & (src >> 8),
                    0xFF & src
                );
        }

        public static uint ToInt(in this Vec3b src, byte alpha = 0xFF)
        {
            uint rst = 0;

            rst += src[0]; rst <<= 8;
            rst += src[1]; rst <<= 8;
            rst += src[2]; rst <<= 8;
            rst += alpha;

            return rst;
        }
    }
}
