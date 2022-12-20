using System.Text;
using BoDi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ownable.dht;
using ownable.store;

namespace ownable.tests.specs.StepDefinitions;

[Binding]
public class KademliaStepDefinitions
{
    private KademliaServer? _server;
    private KademliaClient? _client;
    private bool _result;

    private readonly IObjectContainer _container;

    private string? _key;
    private string? _value;
    private int _port;

    public KademliaStepDefinitions(IObjectContainer container)
    {
        _container = container;
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        _container.RegisterInstanceAs(NullLoggerFactory.Instance.CreateLogger<KademliaClient>());
        _container.RegisterInstanceAs(NullLoggerFactory.Instance.CreateLogger<KademliaServer>());
        _container.RegisterInstanceAs(NullLoggerFactory.Instance.CreateLogger<KeyValueStore>());
        _container.RegisterInstanceAs<IKeyValueStore>(new KeyValueStore(Guid.NewGuid().ToString(), _container.Resolve<ILogger<KeyValueStore>>()));
    }
        
    [Given(@"a Kademlia server is running")]
    public void GivenAServerIsRunning()
    {
        var scanner = new PortScanner();
        _port = scanner.GetNextAvailablePort();
        _server = new KademliaServer(_port, _container.Resolve<IKeyValueStore>(),_container.Resolve<ILogger<KademliaServer>>());
        _server.Start();
    }

    [Given(@"the server is connected to at least one other node in the network")]
    public void GivenTheServerIsConnectedToAnotherNode()
    {
        Assert.NotNull(_server);
        _client = new KademliaClient("localhost", _port, _container.Resolve<ILogger<KademliaClient>>());
        _client.Add(_server!);
        _client.Connect();
    }

    [When(@"I send a request to store the value ""(.*)"" with the key ""(.*)""")]
    public void WhenISendAStoreRequest(string value, string key)
    {
        _key = key;
        _value = value;
        _result = _client!.Store(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(value));
    }

    [Then(@"the value should be stored in the network")]
    public void ThenTheValueShouldBeStoredInTheNetwork()
    {
        Assert.True(_result);
        Assert.NotNull(_key);
        Assert.NotNull(_value);

        var value = _client!.FindValue(Encoding.UTF8.GetBytes(_key!));
        Assert.Equal(value.Item1, Encoding.UTF8.GetBytes(_value!));
    }

    [When(@"I send a (.*) request")]
    public void WhenISendARequest(string command)
    {
        _result = false;

        switch (command.Trim().ToUpperInvariant())
        {
            case "PING":
                _result = _client!.Ping();
                break;
        }
    }

    [Then(@"I should receive a pong response")]
    public void ThenIShouldReceiveAPongResponse()
    {
        Assert.True(_result);
    }
}