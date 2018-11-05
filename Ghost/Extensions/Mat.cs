using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ghost.Extensions
{
    public static class MatExtensions
    {
        public static Scalar[] Scalars(this Mat src)
        {
            var indexer = (new MatOfByte3(src)).GetIndexer();
            var set = new HashSet<Scalar>();

            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    var vec = indexer[y, x];
                    set.Add(vec.ToScalar());
                }
            }

            return set.ToArray();
        }
        public static Scalar[] Scalars(this Mat src, ParallelOptions po)
        {
            var indexer = (new MatOfByte3(src)).GetIndexer();
            var dict = new ConcurrentDictionary<uint, Scalar>();

            Parallel.For(0, src.Surface(), po, (int i) =>
            {
                int y = i / src.Width;
                int x = i % src.Width;

                var vec = indexer[y, x];
                var key = vec.ToInt();

                dict.TryAdd(key, vec.ToScalar());
            });

            return dict.Values.ToArray();
        }

        public static void Slide(this Mat src, Size window, Action<Mat, Point> body)
        {
            int w = (src.Width - window.Width + 1);
            int h = (src.Height - window.Height + 1);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var point = new Point(x, y);
                    using (var trg = src.SubMat(new Rect(point, window)))
                    {
                        body(trg, point);
                    }
                }
            }
        }
        public static void Slide(this Mat src, Size window, CancellationToken token, Action<Mat, Point> body)
        {
            int w = (src.Width - window.Width + 1);
            int h = (src.Height - window.Height + 1);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (token.IsCancellationRequested) return;
                    var loc = new Point(x, y);
                    using (var trg = src.SubMat(new Rect(loc, window)))
                    {
                        body(trg, loc);
                    }
                }
            }
        }
        public static void Slide(this Mat src, Size window, ParallelOptions po, Action<Mat, Point> body)
        {
            int w = (src.Width - window.Width + 1);
            int h = (src.Height - window.Height + 1);

            try
            {
                Parallel.For(0, w * h, po, (int i) =>
                {
                    if (po.CancellationToken.IsCancellationRequested) return;
                    var loc = new Point(i % w, i / w);

                    using (var trg = src.SubMat(new Rect(loc, window)))
                    {
                        body(trg, loc);
                    }
                });
            }
            catch { }
        }

        public static bool TryFindSameOne(this Mat src, Mat trg, Vec3b bg, out Point loc)
        {
            var rst = new Point(-1, -1);
            var trgIdxr = (new MatOfByte3(trg)).GetIndexer();
            var cts = new CancellationTokenSource();

            src.Slide(trg.Size(), cts.Token, (Mat sbj, Point point) =>
            {
                var sbjIdxr = (new MatOfByte3(sbj)).GetIndexer();

                for (int y = 0; y < sbj.Height; y++)
                {
                    for (int x = 0; x < sbj.Width; x++)
                    {
                        var trgVec = trgIdxr[y, x];
                        if (trgVec == bg) continue;

                        var sbjVec = sbjIdxr[y, x];
                        if (sbjVec != trgVec) return;
                    }
                }

                rst = point;
                cts.Cancel();
            });

            cts.Dispose();
            loc = rst;

            if (rst.X < 0)
                return false;
            else return true;
        }
        public static bool TryFindSameOne(this Mat src, Mat trg, Vec3b bg, CancellationToken token, out Point loc)
        {
            var rst = new Point(-1, -1);
            var trgIdxr = (new MatOfByte3(trg)).GetIndexer();
            var cts = new CancellationTokenSource();

            src.Slide(trg.Size(), cts.Token, (Mat sbj, Point point) =>
            {
                var sbjIdxr = (new MatOfByte3(sbj)).GetIndexer();

                for (int y = 0; y < sbj.Height; y++)
                {
                    if (cts.Token.IsCancellationRequested
                     || token.IsCancellationRequested)
                    {
                        cts.Cancel();
                        return;
                    }

                    for (int x = 0; x < sbj.Width; x++)
                    {
                        var trgVec = trgIdxr[y, x];
                        if (trgVec == bg) continue;

                        var sbjVec = sbjIdxr[y, x];
                        if (sbjVec != trgVec) return;
                    }
                }

                rst = point;
                cts.Cancel();
            });

            cts.Dispose();
            loc = rst;

            if (rst.X < 0)
                return false;
            else return true;
        }
        public static bool TryFindSameOne(this Mat src, Mat trg, Vec3b bg, ParallelOptions po, out Point loc)
        {
            var rst = new Point(-1, -1);
            var trgIdxr = (new MatOfByte3(trg)).GetIndexer();
            var cts = new CancellationTokenSource();

            src.Slide(trg.Size(), cts.Token, (Mat window, Point point) =>
            {
                var sbjIdxr = (new MatOfByte3(window)).GetIndexer();

                for (int y = 0; y < window.Height; y++)
                {
                    if (po.CancellationToken.IsCancellationRequested
                     || cts.Token.IsCancellationRequested)
                    {
                        cts.Cancel();
                        return;
                    }

                    for (int x = 0; x < window.Width; x++)
                    {
                        var trgVec = trgIdxr[y, x];
                        if (trgVec == bg) continue;

                        var sbjVec = sbjIdxr[y, x];
                        if (sbjVec != trgVec) return;
                    }
                }

                rst = point;
                cts.Cancel();
            });

            loc = rst;

            if (rst.X < 0)
                return false;
            else return true;
        }

        public static Point[] FindSameAll(this Mat src, Mat trg, Vec3b bg)
        {
            var rst = new List<Point>();
            var trgIdxr = (new MatOfByte3(trg)).GetIndexer();

            src.Slide(trg.Size(), (Mat sbj, Point point) =>
            {
                var sbjIdxr = (new MatOfByte3(sbj)).GetIndexer();

                for (int y = 0; y < sbj.Height; y++)
                {
                    for (int x = 0; x < sbj.Width; x++)
                    {
                        var trgVec = trgIdxr[y, x];
                        if (trgVec == bg) continue;

                        var sbjVec = sbjIdxr[y, x];
                        if (sbjVec != trgVec) return;
                    }
                }

                rst.Add(point);
            });

            return rst.ToArray();
        }
        public static Point[] FindSameAll(this Mat src, Mat trg, Vec3b bg, CancellationToken token)
        {
            var rst = new List<Point>();
            var trgIdxr = (new MatOfByte3(trg)).GetIndexer();
            var cts = new CancellationTokenSource();

            src.Slide(trg.Size(), cts.Token, (Mat sbj, Point point) =>
            {
                var sbjIdxr = (new MatOfByte3(sbj)).GetIndexer();

                for (int y = 0; y < sbj.Height; y++)
                {
                    if (cts.Token.IsCancellationRequested
                     || token.IsCancellationRequested)
                    {
                        cts.Cancel();
                        return;
                    }

                    for (int x = 0; x < sbj.Width; x++)
                    {
                        var trgVec = trgIdxr[y, x];
                        if (trgVec == bg) continue;

                        var sbjVec = sbjIdxr[y, x];
                        if (sbjVec != trgVec) return;
                    }
                }

                rst.Add(point);
            });

            return rst.ToArray();
        }
        public static Point[] FindSameAll(this Mat src, Mat trg, Vec3b bg, ParallelOptions po)
        {
            var rst = new ConcurrentBag<Point>();
            var trgIdxr = (new MatOfByte3(trg)).GetIndexer();

            src.Slide(trg.Size(), po, (Mat window, Point point) =>
            {
                var sbjIdxr = (new MatOfByte3(window)).GetIndexer();

                for (int y = 0; y < window.Height; y++)
                {
                    if (po.CancellationToken.IsCancellationRequested) return;

                    for (int x = 0; x < window.Width; x++)
                    {
                        var trgVec = trgIdxr[y, x];
                        if (trgVec == bg) continue;

                        var sbjVec = sbjIdxr[y, x];
                        if (sbjVec != trgVec) return;
                    }
                }

                rst.Add(point);
            });

            return rst.ToArray();
        }
    }
}
