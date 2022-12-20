Feature: Kademlia

Scenario: Sending a ping
	Given a Kademlia server is running
	And the server is connected to at least one other node in the network
	When I send a ping request
	Then I should receive a pong response

Scenario: Storing a value
	Given a Kademlia server is running
	And the server is connected to at least one other node in the network
	When I send a request to store the value "foo" with the key "bar"
	Then the value should be stored in the network