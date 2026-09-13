using Nation.Core.Signals;
using NUnit.Framework;

namespace Nation.Tests
{
    public sealed class SignalBusTests
    {
        private readonly struct PingSignal : ISignal
        {
            public int Value { get; }

            public PingSignal(int value)
            {
                Value = value;
            }
        }

        [Test]
        public void Subscribed_Handler_Receives_Published_Signal()
        {
            var bus = new SignalBus();
            var received = 0;
            bus.Subscribe<PingSignal>(signal => received = signal.Value);

            bus.Publish(new PingSignal(7));

            Assert.AreEqual(7, received);
        }

        [Test]
        public void Disposed_Subscription_No_Longer_Receives()
        {
            var bus = new SignalBus();
            var count = 0;
            var subscription = bus.Subscribe<PingSignal>(_ => count++);

            bus.Publish(new PingSignal(1));
            subscription.Dispose();
            subscription.Dispose();
            bus.Publish(new PingSignal(2));

            Assert.AreEqual(1, count);
            Assert.AreEqual(0, bus.SubscriberCount<PingSignal>());
        }

        [Test]
        public void Publishing_With_No_Subscribers_Is_Safe()
        {
            var bus = new SignalBus();

            bus.Publish(new PingSignal(1));

            Assert.AreEqual(0, bus.SubscriberCount<PingSignal>());
        }

        [Test]
        public void Handler_May_Unsubscribe_During_Publish()
        {
            var bus = new SignalBus();
            var secondCalls = 0;
            System.IDisposable first = null;
            first = bus.Subscribe<PingSignal>(_ => first.Dispose());
            bus.Subscribe<PingSignal>(_ => secondCalls++);

            bus.Publish(new PingSignal(1));
            bus.Publish(new PingSignal(2));

            Assert.AreEqual(2, secondCalls);
            Assert.AreEqual(1, bus.SubscriberCount<PingSignal>());
        }
    }
}
