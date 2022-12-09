using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class ConwayOperatorsTest
    {
        [Test]
        public void TestDual()
        {
            // Primal
            //  ____         
            // |\  /|               
            // | \/ |               
            // | /\ |               
            // |/__\|               
            //                      
            // Dual
            //   |     
            //   /\    
            //__/  \__ 
            //  \  /   
            //   \/    
            //   |    
            var mesh = ConwayOperators.Kis(TestMeshes.PlaneXY);
            Assert.AreEqual(4, MeshUtils.GetFaces(mesh, 0).Count());
            var r = ConwayOperators.Dual(mesh);
            Assert.AreEqual(5, MeshUtils.GetFaces(r, 0).Count());
            var r2 = ConwayOperators.Dual(r);
            Assert.AreEqual(4, MeshUtils.GetFaces(r2, 0).Count());
        }
    }
}
