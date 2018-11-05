using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ghost.Extensions;
using GhostTest.Properties;
using OpenCvSharp.Extensions;
using System;
using System.Threading.Tasks;

namespace Ghost.Extensions.Tests
{
    [TestClass()]
    public class MatExtensionsTests
    {
        private OpenCvSharp.Mat Color16_4x4;
        private OpenCvSharp.Mat Color16_8x8;
        private OpenCvSharp.Mat Color16_128x128;
        private OpenCvSharp.Mat Green_4x4;


        [TestInitialize]
        public void Initialize()
        {
            Color16_4x4 = Resources.Color16_4x4.ToMat();
            Color16_8x8 = Resources.Color16_8x8.ToMat();
            Color16_128x128 = Resources.Color16_128x128.ToMat();
            Green_4x4 = Resources.Green_4x4.ToMat();
        }

        [TestCategory("Serial")]
        [TestMethod()]
        public void SerialScalarsTestFor16()
        {
            var scalars = Color16_4x4.Scalars();

            Assert.AreEqual(16, scalars.Length);
        }
        [TestCategory("Serial")]
        [TestMethod()]
        public void SerialScalarsTestFor64()
        {
            var scalars = Color16_8x8.Scalars();

            Assert.AreEqual(16, scalars.Length);
        }
        [TestCategory("Serial")]
        [TestMethod()]
        public void SerialScalarsTestFor16384()
        {
            var scalars = Color16_128x128.Scalars();

            Assert.AreEqual(16, scalars.Length);
        }

        [TestCategory("Parallel")]
        [TestMethod()]
        public void ParallelScalarsTestFor16()
        {
            var scalars = Color16_4x4.Scalars(new System.Threading.Tasks.ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
            });

            Assert.AreEqual(16, scalars.Length);
        }
        [TestCategory("Parallel")]
        [TestMethod()]
        public void ParallelScalarsTestFor64()
        {
            var scalars = Color16_8x8.Scalars(new System.Threading.Tasks.ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
            });

            Assert.AreEqual(16, scalars.Length);
        }
        [TestCategory("Parallel")]
        [TestMethod()]
        public void ParallelScalarsTestFor16384()
        {
            var scalars = Color16_128x128.Scalars(new System.Threading.Tasks.ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
            });

            Assert.AreEqual(16, scalars.Length);
        }

        [TestCategory("Serial")]
        [TestMethod()]
        public void SerialTryFindSameOne()
        {
            bool isFind = Color16_128x128.TryFindSameOne(Green_4x4, OpenCvSharp.Scalar.Red.ToVec3b(), out var loc);

            Assert.AreEqual(true, isFind);
        }
        [TestCategory("Parallel")]
        [TestMethod()]
        public void ParallelTryFindSameOne()
        {
            bool isFind = Color16_128x128.TryFindSameOne(Green_4x4, OpenCvSharp.Scalar.Red.ToVec3b(), new ParallelOptions(){
                MaxDegreeOfParallelism = Environment.ProcessorCount
            },out var loc);

            Assert.AreEqual(true, isFind);
        }


        [TestCategory("Serial")]
        [TestMethod()]
        public void SerialFindSameAll()
        {
            var locs = Color16_128x128.FindSameAll(Green_4x4, OpenCvSharp.Scalar.Red.ToVec3b());

            Assert.AreEqual(841, locs.Length);
        }
        [TestCategory("Parallel")]
        [TestMethod()]
        public void ParallelFindSameAll()
        {
            var locs = Color16_128x128.FindSameAll(Green_4x4, OpenCvSharp.Scalar.Red.ToVec3b(), new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            });

            Assert.AreEqual(841, locs.Length);
        }
    }
}