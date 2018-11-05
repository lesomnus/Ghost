using Mat = OpenCvSharp.Mat;
using Size = OpenCvSharp.Size;

namespace Ghost.Extensions
{
    public static class Metrics
    {
        public static int Surface(in this Size src)
        {
            return src.Height * src.Width;
        }
        public static int Surface(this Mat src)
        {
            return src.Height * src.Width;
        }
    }
}
