using ownable.Serialization;

namespace ownable.tests
{
    public class SerializationTests
    {
        [Fact]
        public void RoundTripCheck()
        {
            RoundTrip.Check(TestFactory.GetContract());
        }
    }
}
