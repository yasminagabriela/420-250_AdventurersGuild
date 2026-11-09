using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WaypointCycleTests
{
    [Test]
    public void PingPong_boucleDernierWP() {

        var cycle = new WaypointCycle(3, true);
        Assert.AreEqual(1, cycle.Next());
        Assert.AreEqual(2, cycle.Next());
        Assert.AreEqual(1, cycle.Next()); // turns around
        Assert.AreEqual(0, cycle.Next());

    }



    [Test]
    public void TesterOnCollisionDetect() {

        var cycle = new WaypointCycle(3, true);


    }

}
