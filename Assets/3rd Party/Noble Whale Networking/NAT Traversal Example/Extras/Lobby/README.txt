The ExampleLobby scene contains an example implementation of UNet lobby combined with NAT Traversal.

Instructions:
	1. Add the ExampleLobby and Online scenes to your build settings.
	2. On the NetworkManager, set the LobbyScene to the ExampleLobby scene and the Play Scene to the Online scene.
	3. Build and press the "Enable Match Maker" button and then "Create Internet Match"
	4. Run in the editor and press the "Enable Match Maker" and then "Find Internet Match"
	5. Click the button for the match to join it (should be the top button)
	6. Press the "START" button on the host to become ready.
	7. Press the "START" button on the client to become ready.
	8. When both players are ready the Online scene is automatically loaded.

Next Steps:
	
	Check out the NATLobbyManager.cs and NATLobbyPlayer.cs scripts for details. They are almost exact copies of UNET's NetworkLobbyManager and NetworkLobbyPlayer so you can read Unity's documentation for those components for more info. 