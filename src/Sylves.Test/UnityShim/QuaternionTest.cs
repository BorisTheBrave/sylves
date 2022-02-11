using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Sylves.Test.TestUtils;

namespace Sylves.Test
{
    [TestFixture]
    public class QuaternionTest
    {
        [Test]
        public void TestEulerAngles()
        {
            var pairs = new[]
            {
                (new Quaternion(0, 0, 0, 1), new Vector3(0, 0, 0)),
                (new Quaternion(0.70710677f, 0, 0, 0.70710677f), new Vector3(90, 0, 0)),
                (new Quaternion(-0.70710677f, 0, 0, 0.70710677f), new Vector3(-90, 0, 0)),
                (new Quaternion(0, 0.70710677f, 0, 0.70710677f), new Vector3(0, 90, 0)),
                (new Quaternion(0, -0.70710677f, 0, 0.70710677f), new Vector3(0, -90, 0)),
                (new Quaternion(0, 0, 0.70710677f, 0.70710677f), new Vector3(0, 0, 90)),
                (new Quaternion(0, 0, -0.70710677f, 0.70710677f), new Vector3(0, 0, -90)),
            };

            foreach(var (q, e) in pairs)
            {
                var q2 = new Quaternion();
                q2.eulerAngles = e;
                AssertAreEqual(q, q2, 1e-4);
                // Wow, we really need to use more robust methods, 0.1 is is not good tolerance.
                AssertAreEqual(e, q.eulerAngles, 1e-1);
            }
        }




    }
}
