using ownable.Serialization;

namespace ownable.tests
{
    public class SerializationTests
    {
        [Fact]
        public void RoundTripCheck()
        {
            RoundTrip.Check(TestFactory.GetContract());
            RoundTrip.Check(TestFactory.GetReceived());
            RoundTrip.Check(TestFactory.GetSent());
        }
    }
}
