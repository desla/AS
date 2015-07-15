using Alvasoft.AudioServer.ChannelsManager;
using Alvasoft.AudioServer.ChannelsManager.Impl;
//using NUnit.Framework;

namespace AudioServer.Tests
{
    //[TestFixture]
    //class PriorityQueueTest
    //{
    //    [Test]
    //    public void QueuePriorityTest()
    //    {
    //        var queue = new SoundsPriorityQueue();

    //        queue.InsertMessage(new SoundMessage(new byte[1] { 1 }, 1));
    //        queue.InsertMessage(new SoundMessage(new byte[1] { 2 }, 2));
    //        queue.InsertMessage(new SoundMessage(new byte[1] { 3 }, 3));
    //        queue.InsertMessage(new SoundMessage(new byte[1] { 4 }, 4));
    //        queue.InsertMessage(new SoundMessage(new byte[1] { 5 }, 5));

    //        Assert.AreEqual(5, queue.GetTopMessage().GetSound()[0]);
    //        Assert.AreEqual(4, queue.GetTopMessage().GetSound()[0]);
    //        Assert.AreEqual(3, queue.GetTopMessage().GetSound()[0]);
    //        Assert.AreEqual(2, queue.GetTopMessage().GetSound()[0]);
    //        Assert.AreEqual(1, queue.GetTopMessage().GetSound()[0]);
    //        Assert.IsNull(queue.GetTopMessage());
    //    }

    //    [Test]
    //    public void QueuePriorityTest_1()
    //    {
    //        var queue = new SoundsPriorityQueue();

    //        queue.InsertMessage(new SoundMessage(new byte[1] { 1 }, 1));
    //        queue.InsertMessage(new SoundMessage(new byte[1] { 21 }, 2));
    //        queue.InsertMessage(new SoundMessage(new byte[1] { 3 }, 3));
    //        queue.InsertMessage(new SoundMessage(new byte[1] { 4 }, 4));
    //        queue.InsertMessage(new SoundMessage(new byte[1] { 5 }, 5));

    //        Assert.AreEqual(5, queue.GetTopMessage().GetSound()[0]);
    //        Assert.AreEqual(4, queue.GetTopMessage().GetSound()[0]);

    //        queue.InsertMessage(new SoundMessage(new byte[1] { 5 }, 5));
    //        Assert.AreEqual(5, queue.GetTopMessage().GetSound()[0]);

    //        queue.InsertMessage(new SoundMessage(new byte[1] { 22 }, 2));
    //        Assert.AreEqual(3, queue.GetTopMessage().GetSound()[0]);
    //        Assert.AreEqual(21, queue.GetTopMessage().GetSound()[0]);
    //        Assert.AreEqual(22, queue.GetTopMessage().GetSound()[0]);
    //    }
    //}
}
