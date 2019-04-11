using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MatchUp;
using UnityEngine.Networking.Match;
using System;
using UnityEngine.Networking.Types;
using TMPro;

#if !UNITY_5_3
using System.Net.Sockets;
#endif

/// <summary>Utilize the MatchUp Matchmaker to perform matchmaking</summary>
/// <remarks>
/// Connect to the match host using UNet.
/// Also creates a UNET match if relay connections are enabled.
/// </remarks>s
[RequireComponent(typeof(NetworkManager))]
[RequireComponent(typeof(Matchmaker))]
public class ExampleMatchmakerForUNet : MonoBehaviour
{
    /// <summary>A reference to the MatchUp Matchmaker component that will be used for matchmaking</summary>
    public Matchmaker matchUp;
    /// <summary>A reference to the NetworkManager that will be used for hosting and joining.</summary>
    public NetworkManager networkManager;
    /// <summary>A list of matches returned by the MatchUp server.</summary>
    /// <remarks>This is populated in the GetMatchList() method</remarks>
    Match[] matches;

    /// <summary>Enable relay connections</summary>
    /// <remarks>
    /// If relays are enabled a UNET match will be created along with the Match Up match.
    /// The UNET match ID is stored along with the other match data and used when calling
    /// StartClient(). UNET's matchmaking isn't actually used for listing matches or storing
    /// match data, but the match ID is needed to connect via relay.
    /// </remarks>
    public bool useRelays;

    ///Use to setup match info
    public TMP_InputField matchName;
    public TMP_Text map;
    public TMP_Text gametype;
    public GameObject playerPrefab;
    /// <summary>Get a references to components we will use often</summary>
	void Awake()
    {
        matchUp = GetComponent<Matchmaker>();
        networkManager = GetComponent<NetworkManager>();
	}

    /// <summary>Display buttons for hosting, listing, joining, and leaving matches.</summary>
    void OnGUI()
    {
        if (!matchUp.IsReady || NetworkManagerExtension.externalIP == null) GUI.enabled = false;
        else GUI.enabled = true;

        if (!NetworkServer.active && !NetworkClient.active)
        {
            //// Host a match
            //if (GUI.Button(new Rect(10, 10, 150, 48), "Host"))
            //{
            //    HostAMatch();
            //}

            //// List matches
            //if (GUI.Button(new Rect(10, 60, 150, 48), "Search matches"))
            //{
            //    GetMatchList();
            //}

            // Display the match list
            if (matches != null)
            {
                for (int i = 0; i < matches.Length; i++)
                {
                    DisplayJoinMatchButton(i, matches[i]);
                }
            }
        }
        else
        {
            // Leave match
            if (GUI.Button(new Rect(10, 110, 150, 48), "Disconnect"))
            {
                Disconnect();
            }
        }

        // Set some match data
        if (NetworkServer.active)
        {
            if (GUI.Button(new Rect(10, 160, 150, 48), "Set match data"))
            {
                SetMatchData();
            }
        }
    }

    /// <summary>Display a button to join a match</summary>
    /// <param name="i"></param>
    /// <param name="match"></param>
    void DisplayJoinMatchButton(int i, Match match)
    {
        // Grab some match data to display on the button
        var data = matches[i].matchData;
        string matchDesc = "";
        matchDesc += matches[i].id + "|";
        matchDesc += data["Match name"] + "|";
        matchDesc += data["eloScore"] + "|";
        matchDesc += data["Region"] + "|";
        matchDesc += data["Map name"] + "|";
        matchDesc += data["Game type"];

        // Join the match
        if (GUI.Button(new Rect(170, 10 + i * 26, 600, 25), matchDesc))
        {
            matchUp.JoinMatch(matches[i], OnJoinMatch);
        }
    }

    /// <summary>Host a match</summary>
    /// <remarks>
    /// If relays are enabled this will cause a UNET match to be created as well.
    /// The Match Up match will not be created until after the UNET match is up because
    /// the match ID needs to be added to the match data for relay connections to work.
    /// If relays are not enabled the MAtch Up match is created immediately and no UNET
    /// match is created.
    /// </remarks>
    public void HostAMatch()
    {
        if (useRelays)
        {
            // If using the relays we must create a unet match and wait to get the match id before we can create the match up match
            networkManager.StartMatchMaker();
#if UNITY_5_3
            networkManager.matchMaker.CreateMatch("", networkManager.matchSize, true, "", OnUNETMatchCreated);
#else
            networkManager.matchMaker.CreateMatch(
                "", 
                networkManager.matchSize, 
                true, 
                "", 
                NetworkManagerExtension.externalIP, 
                NetworkManagerExtension.GetLocalAddress(AddressFamily.InterNetwork), 0, 0, OnUNETMatchCreated);
#endif
        }
        else
        {
            // Host and create the match
            networkManager.StartHost();
            CreateMatch();

        }
    }

    public void StartMatch() {
       // networkManager.StartHost();
      // networkManager.singleton.ServerChangeScene();
//      networkManager.playerPrefab = playerPrefab;
      networkManager.ServerChangeScene("OnlineEnviroment");
    }


    /// <summary>Called when a UNET match is created. This is only used when relays are enabled.</summary>
#if UNITY_5_3
    void OnUNETMatchCreated(CreateMatchResponse response)
    {
        // Start hosting
        networkManager.OnMatchCreate(response);
        // Create the match
        CreateMatch();
    }
#else
    void OnUNETMatchCreated(bool success, string extendedInfo, MatchInfo response)
    {
        // Start hosting
        networkManager.OnMatchCreate(success, extendedInfo, response);
        // Create the match
        CreateMatch();
    }
#endif

    /// <summary>Create a match with some example match data</summary>
   public void CreateMatch()
    {
        // You can set MatchData when creating the match. (string, float, double, int, or long)
        var matchData = new Dictionary<string, MatchData>() {
            //{ "Match name", "Layla's Match" },
            //{ "eloScore", 200 },
            //{ "Region", "North America" },
            //{ "Map name", "gh_test-level" },
            //{ "Game type", "Capture the flag" },
            { "Match name", matchName.text },
            { "eloScore", 200 },
            { "Region", "North America" },
            { "Map name", map.text },
            { "Game type", gametype.text },
        };

        // Create the Match with the associated MatchData
        matchUp.CreateMatch(networkManager.maxConnections + 1, matchData, OnMatchCreated);
    }

    /// <summary>Called when a response is received from the CreateMatch request.</summary>
    /// <param name="success">True if the match was created. False if something went wrong.</param>
    /// <param name="match">The match</param>
    void OnMatchCreated(bool success, Match match)
    {
        Debug.Log("Created match: " + match.matchData["Match name"]);
    }
    
    /// <summary>Get a filtered list of matches</summary>
    public void GetMatchList()
    {
        Debug.Log("Fetching match list");

        // Filter so that we only receive matches with 
        // an eloScore between 150 and 350
        // and the Region is North America
        // and the Game type is Capture the Flag
        var filters = new List<MatchFilter>(){
            new MatchFilter("eloScore", 150, MatchFilter.OperationType.GREATER),
            new MatchFilter("eloScore", 350, MatchFilter.OperationType.LESS),
            new MatchFilter("Region", "North America", MatchFilter.OperationType.EQUALS),
            //new MatchFilter("Game type", "Capture the flag", MatchFilter.OperationType.EQUALS)
        };

        //Get the filtered match list.The results will be received in OnMatchList()
        matchUp.GetMatchList(OnMatchListGot, 0, 10, filters);

        // This is how you would get a single match with a match ID of 13.
        //matchUp.GetMatch(OnMatchGot, 13);
    }

    /// <summary>Called when the match list is retreived via GetMatchList</summary>
    /// <param name="success">True unless something went wrong</param>
    /// <param name="matches">The list of Matches</param>
    void OnMatchListGot(bool success, Match[] matches)
    {
        if (!success) return;

        Debug.Log("Received match list.");
        this.matches = matches;
    }

    /// <summary>Called when a single match is retreived via GetMatch</summary>
    /// <param name="success">True if the match was found. False otherwise.</param>
    /// <param name="match">The match, if found</param>
    void OnMatchGot(bool success, Match match)
    {
        if (!success)
        {
            Debug.Log("No match found.");
            return;
        }

        this.matches = new Match[1];
        this.matches[0] = match;
    }

    /// <summary>Called when a response is received from a JoinMatch request.</summary>
    /// <param name="success">True if the match existed and was joined. False otherwise.</param>
    /// <param name="match">The Match that was joined.</param>
    void OnJoinMatch(bool success, Match match)
    {
        if (!success) return;

        Debug.Log("Joined match: " + match.matchData["Match name"]);

        if (useRelays && match.matchData.ContainsKey("unetMatchID"))
        {
            // When using relays we must also join the unet match in order to connect via relay
            networkManager.StartMatchMaker();
            var netID = (NetworkID)match.matchData["unetMatchID"].ulongValue;
#if UNITY_5_3      
            networkManager.matchMaker.JoinMatch(netID, "", networkManager.OnMatchJoined);
#else
            string externalIP = match.matchData["externalIP"];
            string internalIP = match.matchData["internalIP"];
            networkManager.networkPort = match.matchData["port"];
            networkManager.matchMaker.JoinMatch(netID, "", externalIP, internalIP, 0, 0, networkManager.OnMatchJoined);
#endif
        }
        else
        {
            // When not using relays we connect directly
            if (useRelays)
            {
                throw new Exception("unetMatchID missing from match data. Can not connect via relay. Falling back to direct connect.");
            }
            // Connect to the host
            networkManager.StartClient(match);
        }
    }
    
    /// <summary>An example of setting some match data.</summary>
    /// <remarks>
    /// There are a couple of ways to set match data depending on how much you are
    /// setting and what your syntax preferences are.
    ///
    /// NOTE: If you are setting more than one value at once you should not use Option 1 
    /// since it will send a message to the server each time you call it.
    /// 
    /// NOTE: If you are setting a value very often (like every frame) you probably want to use
    /// Option 3 so that you don't send the data until you explicity call UpdateMatchData()
    /// </remarks>
    public void SetMatchData()
    {
        Debug.Log("Setting match data");

        /**
         * Option 1: Add or set a single match data value and immediately send it to the matchmaking server
         */
        matchUp.SetMatchData("eloScore", 50);

        /**
         * Option 2: Completely replace existing match data and immediately send it to the matchmaking server
         */
        //var newMatchData = new Dictionary<string, MatchData>() {
        //    { "Key1", "value1" },
        //    { "Key2", 3.14159 }
        //};
        //matchUp.SetMatchData(newMatchData);

        /**
         * Option 3a: Add or set several match data values and then send them all at once
         */
        //matchUp.currentMatch.matchData["Key1"] = 3.14159;
        //matchUp.currentMatch.matchData["Key2"] = "works for strings too";
        //matchUp.UpdateMatchData(); // Send the data to the matchmaking server

        /**
         * Option 3b: Alternative syntax to add or set several match data values and then send them all at once.
         */
        //var additionalMatchData = new Dictionary<string, MatchData>() {
        //    { "Key1", "value1" },
        //    { "Key2", 3.14159 }
        //};
        // This will merge the match data you pass in with any existing match data
        //matchUp.UpdateMatchData(additionalMatchData); // Send the data to the matchmaking server
    }

    /// <summary>Disconnect and leave the Match</summary>
    public void Disconnect()
    {
        // Stop hosting and destroy the match
        if (NetworkServer.active)
        {
            Debug.Log("Destroyed match");
            networkManager.StopHost();
            matchUp.DestroyMatch();
        }

        // Disconnect from the host and leave the match
        else
        {
            Debug.Log("Left match");
            networkManager.StopClient();
            matchUp.LeaveMatch();
        }
    }
}
