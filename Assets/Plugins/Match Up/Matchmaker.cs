using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MatchUp
{
    /// <summary>Provides functionality for creating, listing, joining, and leaving matches.</summary>
    /// <remarks>
    /// Opens up a tcp socket connection to the matchmaking server in order to send
    /// commands and receive responses.
    /// Most methods take an optional callback parameter which you can use to get
    /// the response once it is received.
    /// </remarks>
    public class Matchmaker : MonoBehaviour
    {
        const string TAG = "Match Up: ";

        /// <summary>How often in seconds to send keepalive messages to the matchmaking server</summary>
        /// <remarks>
        /// NAT Bindings do not last forever, so they must be refreshed periodically. 
        /// RFC 4787 Section 12 REQ-5 specifies that bindings must not expire in less than 2 minutes, 
        /// so as long as this refresh time is less than 2 minutes it is guaranteed that bindings 
        /// will stay alive on any complient nat device.
        /// I've chosen a default of sending every 30 seconds, with the server disconnecting a client if they haven't
        /// sent a keepalive message for 1 minute.
        /// </remarks>
        const int KEEP_ALIVE_TIMEOUT = 30; // Seconds

        #region -- Properties ---------------------------------------------------------------------

        /// <summary>How long to wait for a response before giving up</summary>
        public float timeout = 5;

        /// <summary>The url of the matchmaking server.</summary>
        /// <remarks>
        /// You can use mine for testing but it could go offline at any time so don't even
        /// think of trying to release a game without hosting your own matchmaking server.
        /// </remarks>
        public string matchmakerURL = "noblewhale.com";

        /// <summary>The port to connect to on the matchmaking server.</summary>
        /// <remarks>
        /// Leave this as default unless you started the server on a specific port using
        /// <code>
        ///     ./MatchUp -p [PORT NUMBER]
        /// </code>
        /// </remarks>
        public int matchmakerPort = 20203;

        /// <summary>A web service to query to retrieve the external IP of this computer.</summary>
        public string externalIPSource = "ipv4.noblewhale.com";

        #endregion

        #region -- Runtime data -------------------------------------------------------------------

        NetworkManager networkManager;

        /// <summary>The connection to the matchmaking server.</summary>
        TcpClient matchmakingClient;
        /// <summary>Used for sending to the matchmaking server.</summary>
        NetworkStream networkStream;

        public void CreateMatch(int v, Dictionary<string, MatchData> matchData, object onMatchCreated)
        {
            throw new NotImplementedException();
        }

        /// <summary>Used for receiving from the matchmaking server.</summary>
        StreamReader streamReader;

        /// <summary>Returns true when the matchmaking server has been succesfully connected to.</summary>
        public bool IsReady {
            get {
                return matchmakingClient != null && networkStream != null && streamReader != null && networkStream.CanWrite;
            }
        }

        /// <summary>Keep track of open Transactions so we can call the appropriate onResponse method when a response is received.</summary>
        /// <remarks>
        /// Each request that is sent generates a Transaction with a unique transaction ID.
        /// When the matchmaking server response to a request it will include the ID in the response.
        /// When the response is received the transaction ID is used to look up the transaction
        /// so that it can be completed and the onResponse handler can be called.
        /// </remarks>
        Dictionary<string, Transaction> transactions = new Dictionary<string, Transaction>();

        /// <summary>The current Match. This is set whenever a Match is created or joined.</summary>
        public Match currentMatch;
        #endregion

        #region -- Unity stuff --------------------------------------------------------------------

        /// <summary>Set up the networking stuff</summary>
        IEnumerator Start()
        {
            var asyncResult = Dns.BeginGetHostAddresses(matchmakerURL, null, null);

            networkManager = FindObjectOfType<NetworkManager>();
            // We need to get the external IP
            if (networkManager) StartCoroutine(NetworkManagerExtension.FetchExternalIP(externalIPSource));

            while (!asyncResult.IsCompleted) yield return 0;

            var addresses = Dns.EndGetHostAddresses(asyncResult);
            if (addresses == null || addresses.Length == 0)
            {
                Debug.LogError(TAG + "Failed to resolve matchmakerUrl: " + matchmakerURL);
                yield break;
            }

            var matchmakerIP = addresses[0];

            matchmakingClient = new TcpClient();
            asyncResult = matchmakingClient.BeginConnect(matchmakerIP, matchmakerPort, null, null);

            while (!asyncResult.IsCompleted) yield return 0;

            StartCoroutine(KeepAlive());

            try
            {
                matchmakingClient.EndConnect(asyncResult);
                networkStream = matchmakingClient.GetStream();
                streamReader = new StreamReader(networkStream);
            }
            catch (SocketException e)
            {
                Debug.LogError(TAG + "Failed to connect to the matchmaking server. Is it running? " + e);
            }
            catch (Exception e)
            {
                Debug.LogError(TAG + "Failed to connect to the matchmaking server. " + e);
            }
        }

        IEnumerator KeepAlive()
        {
            while (gameObject.activeInHierarchy)
            {
                yield return new WaitForSeconds(KEEP_ALIVE_TIMEOUT);
                networkStream.Write(new byte[] { (byte)'\n' }, 0, 1);
            }
        }

        /// <summary>Close the socket connection.</summary>
        private void OnDestroy()
        {
            if (matchmakingClient != null) matchmakingClient.Close();
        }
        
        /// <summary>Check for incoming responses from the matchmaking server.</summary>
        protected virtual void Update()
        {
            ReadData();
        }

        void ReadData()
        {
            // Check if there is anything to read and if there is read a line
            while (networkStream != null && networkStream.CanRead && networkStream.DataAvailable)
            {
                string result = streamReader.ReadLine();

                if (result.Length == 0)
                {
                    return;
                }

                int pipePos = result.IndexOf('|');
                string transactionID = result.Substring(0, pipePos);
                string response = result.Substring(pipePos + 1);

                Transaction t;
                bool success = transactions.TryGetValue(transactionID, out t);
                if (success)
                {
                    transactions.Remove(transactionID);
                    t.Complete(response);
                }
                else
                {
                    Debug.LogWarning(TAG + "Received a response for which there is no open transaction: " + result);
                }
            }
        }

        #endregion

        #region -- Public interface ---------------------------------------------------------------

        /// <summary>Send the command to the matchmaking server to create a new match.</summary>
        /// <param name="maxClients">The maximum number of clients to allow. Once a match is full it is no longer returned in match listings (until a client leaves).</param>
        /// <param name="matchData">Optional match data to include with the match. This is a good place to store your connection data.</param>
        /// <param name="matchName">The name of the match.</param>
        /// <param name="onCreateMatch">Optional callback method to call when a response is received from the matchmaking server.</param>
        public void CreateMatch(int maxClients, Dictionary<string, MatchData> matchData = null, Action<bool, Match> onCreateMatch = null)
        {
            if (matchData == null) matchData = new Dictionary<string, MatchData>();
            if (networkManager)
            {
                if (!matchData.ContainsKey("internalIP")) matchData["internalIP"] = NetworkManagerExtension.GetLocalAddress(AddressFamily.InterNetwork);
                if (!matchData.ContainsKey("externalIP")) matchData["externalIP"] = NetworkManagerExtension.externalIP;
                if (!matchData.ContainsKey("port")) matchData["port"] = networkManager.networkPort;
                if (networkManager.matchMaker != null && networkManager.matchInfo != null)
                {
                    matchData["unetMatchID"] = (ulong)networkManager.matchInfo.networkId;
                }
            }
            if (matchmakerURL == "grabblesgame.com" || matchmakerURL == "noblewhale.com")
            {
                // If you're using my matchmaking server then we need to include some sort of ID to keep your game's matches separate from everyone else's
                if (!matchData.ContainsKey("applicationID")) matchData["applicationID"] = Application.productName;
            }
            currentMatch = new Match(-1, matchData);
            SendCommand(
                Command.CREATE_MATCH, 
                currentMatch.Serialize(maxClients), 
                (success, transaction) => OnCreateMatchInternal(success, transaction.response, onCreateMatch)
            );
        }
        
        /// <summary>Set a single MatchData value and immediately send it to the matchmaking server.</summary>
        /// <remarks>
        /// Ex:
        /// <code>matchUp.SetMatchData("eloScore", 100);</code>
        /// </remarks>
        /// <param name="key">The key</param>
        /// <param name="matchData">The value</param>
        /// <param name="onSetMatchData">Optional callback method to call when a response is received from the matchmaking server.</param>
        public void SetMatchData(string key, MatchData matchData, Action<bool, Match> onSetMatchData = null)
        {
            if (currentMatch == null || currentMatch.id == -1)
            {
                Debug.LogWarning("Can not SetMatchData until after a match has been created: " + key);
                onSetMatchData(false, null);
                return;
            }
            currentMatch.matchData[key] = matchData;
            SendCommand(
                Command.SET_MATCH_DATA,
                currentMatch.id + "|" + matchData.Serialize(key),
                (success, response) => {
                    if (onSetMatchData != null) onSetMatchData(success, currentMatch);
                }
            );
        }

        /// <summary>Replace all existing match data with new match data.</summary>
        /// <remarks>
        /// Ex:
        /// <code>
        /// var newMatchData = new Dictionary<string, MatchData>() {
        ///    { "Key1", "value1" },
        ///    { "Key2", 3.14159 }
        /// };
        /// matchUp.SetMatchData(newMatchData);
        /// </code>
        /// </remarks>
        /// <param name="matchData">A Dictionary of new MatchData</param>
        /// <param name="onSetMatchData">Optional callback method to call when a response is received from the matchmaking server.</param>
        public void SetMatchData(Dictionary<string, MatchData> matchData, Action<bool, Match> onSetMatchData = null)
        {
            if (currentMatch == null || currentMatch.id == -1)
            {
                Debug.LogWarning("Can not SetMatchData until after a match has been created");
                onSetMatchData(false, null);
                return;
            }

            currentMatch.matchData = matchData;
            UpdateMatchData(onSetMatchData);
        }

        /// <summary>Merge new MatchData with existing MatchData and immediately send it all to the matchmaking server.</summary>
        /// <remarks>
        /// Ex:
        /// <code>
        /// var additionalMatchData = new Dictionary<string, MatchData>() {
        ///    { "Key1", new MatchData("value1") },
        ///    { "Key2", new MatchData(3.14159) }
        /// };
        /// matchUp.UpdateMatchData(additionalMatchData);
        /// </code>
        /// </remarks>
        /// <param name="additionalData">A Dictionary of additional MatchData to merge into existing match data</param>
        /// <param name="onUpdateMatchData">Optional callback method to call when a response is received from the matchmaking server.</param>
        public void UpdateMatchData(Dictionary<string, MatchData> additionalData, Action<bool, Match> onUpdateMatchData = null)
        {
            if (currentMatch == null || currentMatch.id == -1)
            {
                Debug.LogWarning("Can not UpdateMatchData until after a match has been created");
                onUpdateMatchData(false, null);
                return;
            }

            // Add new MatchData entries and replace existing one
            foreach (KeyValuePair<string, MatchData> kv in additionalData)
            {
                currentMatch.matchData[kv.Key] = kv.Value;
            }
            UpdateMatchData(onUpdateMatchData);
        }

        /// <summary>Send current MatchData to the matchmaking server.</summary>
        /// <remarks>
        /// Ex:
        /// <code>
        /// matchUp.currentMatch.matchData["Key1"] = 3.14159;
        /// matchUp.currentMatch.matchData["Key2"] = "Hello world";
        /// matchUp.UpdateMatchData();
        /// </code>
        /// </remarks>
        /// <param name="onUpdateMatchData">Optional callback method to call when a response is received from the matchmaking server.</param>
        public void UpdateMatchData(Action<bool, Match> onUpdateMatchData = null)
        {
            if (currentMatch == null || currentMatch.id == -1)
            {
                Debug.LogWarning("Can not UpdateMatchData until after a match has been created");
                onUpdateMatchData(false, null);
                return;
            }

            SendCommand(
                Command.SET_MATCH_DATA, 
                currentMatch.id + "|" + MatchData.SerializeDict(currentMatch.matchData), 
                (success, response) => {
                    if (onUpdateMatchData != null) onUpdateMatchData(success, currentMatch);
                }
            );
        }

        /// <summary>Destroy a match. This also removes all Client entries and MatchData on the matchmaking server.</summary>
        /// <param name="onDestroyMatch">Optional callback method to call when a response is received from the matchmaking server.</param>
        public void DestroyMatch(Action<bool> onDestroyMatch = null)
        {
            if (currentMatch == null || currentMatch.id == -1)
            {
                // There is no match to destroy
                Debug.LogWarning("Can not DestroyMatch because there is no current match.");
                onDestroyMatch(false);
                return;
            }
            SendCommand(
                Command.DESTROY_MATCH, 
                currentMatch.id.ToString(), 
                (success, response) => {
                    if (onDestroyMatch != null) onDestroyMatch(success);
                }
            );
        }

        /// <summary>Join one of the matches returned my GetMatchList().</summary>
        /// <remarks>
        /// You can use the callback to get the Match object after it is received from the matchmaking server.
        /// Once the match is joined you'll have access to all the match's MatchData.
        /// </remarks>
        /// <param name="match">The Match to join. Generally this will come from GetMatchList()</param>
        /// <param name="onJoinMatch">Optional callback method to call when a response is received from the matchmaking server.</param>
        public void JoinMatch(Match match, Action<bool, Match> onJoinMatch = null)
        {
            if (match == null || match.id == -1)
            {
                // There is no match to join
                Debug.LogWarning("Can not JoinMatch because the match is invalid (null or id == -1)");
                onJoinMatch(false, match);
                return;
            }
            currentMatch = match;
            SendCommand(
                Command.JOIN_MATCH,
                currentMatch.id.ToString(), 
                (success, transaction) => OnJoinMatchInternal(success, transaction.response, match, onJoinMatch)
            );
        }
        
        /// <summary>Leave a match.</summary>
        /// <param name="onLeaveMatch">Optional callback method to call when a response is received from the matchmaking server.</param>
        public void LeaveMatch(Action<bool> onLeaveMatch = null)
        {
            if (currentMatch == null || currentMatch.id == -1)
            {
                // There is no match to leave
                Debug.LogWarning("Can not LeaveMatch because there is no current match.");
                onLeaveMatch(false);
                return;
            }
            SendCommand(
                Command.LEAVE_MATCH, 
                currentMatch.clientID.ToString(), 
                (success, response) => {
                    if (onLeaveMatch != null) onLeaveMatch(success);
                }
            );
        }

        /// <summary>Get info on a single match</summary>
        /// <param name="onGetMatch">Callback method to call when the response is received from the matchmaking server</param>
        /// <param name="id">The ID of the match to fetch info for</param>
        /// <param name="includeMatchData">Whether or not to include match data in the response</param>
        public void GetMatch(Action<bool, Match> onGetMatch, int id, bool includeMatchData = true)
        {
            char includeMatchDataChar = includeMatchData ? '1' : '0';
            SendCommand(
                Command.GET_MATCH,
                id + "," + includeMatchDataChar,
                (success, transaction) => OnGetMatchInternal(success, transaction.response, onGetMatch)
            );
        }

        /// <summary>Get the match list, optionally filtering the results.</summary>
        /// <remarks>
        /// Ex:
        /// <code>
        /// var filters = new List<MatchFilter>(){
        ///     new MatchFilter("eloScore", 100, MatchFilter.OperationType.GREATER),
        ///     new MatchFilter("eloScore", 300, MatchFilter.OperationType.LESS)
        /// };
        /// matchUp.GetMatchList(OnMatchList, filters);
        /// ...
        /// void OnMatchList(bool success, Match[] matches)
        /// {
        ///     matchUp.JoinMatch(matches[0], OnJoinMatch);
        /// }
        /// </code>
        /// </remarks>
        /// <param name="onMatchList">Callback method to call when a response is received from the matchmaking server.</param>
        /// <param name="pageNumber">Used with resultsPerPage. Determines which page of results to return. Defaults to 0.</param>
        /// <param name="resultsPerPage">User with pageNumber. Determines how many matches to return for each page. Defaults to 10.</param>
        /// <param name="filters">Optional List of Filters to use when fetching the match list</param>
        /// <param name="includeMatchData">
        /// By default match data is included for every match in the list. 
        /// If you don't need / want this you can pass false in here and save some bandwidth. 
        /// If you don't retrieve match data here you can still get it when joining the match.
        /// </param>
        public void GetMatchList(Action<bool, Match[]> onMatchList, int pageNumber = 0, int resultsPerPage=10, List<MatchFilter> filters = null, bool includeMatchData = true)
        {
            if (matchmakerURL == "grabblesgame.com" || matchmakerURL == "noblewhale.com")
            {
                if (filters != null)
                {
                    if (filters.Find(x => x.key == "applicationID") == null)
                    {
                        // If you're using my matchmaking server then we need to include some sort of ID to keep your game's matches separate from everyone else's
                        filters.Add(new MatchFilter("applicationID", Application.productName));
                    }
                }
            }
            string filterString = "";
            if (filters != null) filterString = "|" + MatchFilter.Serialize(filters);
            char includeMatchDataChar = includeMatchData ? '1' : '0';
            SendCommand(
                Command.GET_MATCH_LIST, 
                pageNumber + "," + resultsPerPage + "," + includeMatchDataChar + filterString, 
                (success, transaction) => OnGetMatchListInternal(success, transaction.response, onMatchList)
            );
        }

        #endregion

        #region -- Internal stuff -----------------------------------------------------------------

        /// <summary>Parses the CreateMatch response to get the match id and clientID</summary>
        void OnCreateMatchInternal(bool success, string response, Action<bool, Match> onCreateMatch)
        { 
            if (!success)
            {
                if (onCreateMatch != null) onCreateMatch(success, currentMatch);
                return;
            }

            string[] parts = response.Split(',');
            try
            {
                currentMatch.id = long.Parse(parts[0]);
                currentMatch.clientID = long.Parse(parts[1]);
            }
            catch(Exception e)
            {
                Debug.LogError(TAG + "Error parsing CreateMatch response from matchmaking server." + e, gameObject);
                success = false;
            }

            if (onCreateMatch != null) onCreateMatch(success, currentMatch);
        }

        /// <summary>Parses the clientID and MatchData returned by the matchmaking server.</summary>
        void OnJoinMatchInternal(bool success, string response, Match match, Action<bool, Match> onJoinMatch)
        {
            int endPos = 0;
            int startPos = 0;
            try
            {
                // Client id
                endPos = response.IndexOf(',', startPos);
                if (endPos == -1)
                {
                    currentMatch.clientID = int.Parse(response);
                }
                else
                {
                    currentMatch.clientID = int.Parse(response.Substring(startPos, (endPos - startPos)));
                    startPos = endPos + 1;

                    // The rest of the match data
                    match.matchData = parseMatchData(response, startPos, ref endPos);
                    startPos = endPos + 1;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(TAG + "Error parsing JoinMatch response from matchmaking server." + e, gameObject);
                success = false;
            }

            if (onJoinMatch != null)
            {
                onJoinMatch(success, match);
            }
        }

        /// <summary>Parses the match list returned by the matchmaking server</summary>
        void OnGetMatchInternal(bool success, string response, Action<bool, Match> onMatch)
        {
            int endPos = 0;
            int startPos = 0;

            if (response == "")
            {
                onMatch(false, null);
                return;
            }

            Match match = null;
            try
            {
                // Match id
                endPos = response.IndexOf(',', startPos);
                int matchID = int.Parse(response.Substring(startPos, (endPos - startPos)));
                startPos = endPos + 1;

                // Create the match object
                match = new Match(matchID);

                // Add the match data to the match
                match.matchData = parseMatchData(response, startPos, ref endPos);
                startPos = endPos + 1;
            }
            catch (Exception e)
            {
                Debug.LogError(TAG + "Error parsing GetMatchList response from matchmaking server." + e, gameObject);
                success = false;
            }

            if (onMatch != null)
            {
                onMatch(success, match);
            }
        }

        /// <summary>Parses the match list returned by the matchmaking server</summary>
        void OnGetMatchListInternal(bool success, string response, Action<bool, Match[]> onMatchList)
        {
            int endPos = 0;
            int startPos = 0;

            List<Match> matches = new List<Match>();
            try
            {
                while (endPos != -1 && endPos < response.Length && startPos < response.Length)
                {
                    // Match id
                    endPos = response.IndexOf(',', startPos);
                    int matchID = int.Parse(response.Substring(startPos, (endPos - startPos)));
                    startPos = endPos + 1;

                    // Match name
                    // This is actually deprecated and unused but left in so I don't have to change the server
                    parseQuoted(response, startPos, ref endPos);
                    bool hasMatchData = endPos < response.Length && response[endPos] == ',';
                    startPos = endPos + 1;
                    
                    // Create the match object
                    Match match = new Match(matchID);
                    
                    // Check if there is match data that needs parsing
                    if (hasMatchData)
                    {
                        // Add the match data to the match
                        match.matchData = parseMatchData(response, startPos, ref endPos);
                        startPos = endPos + 1;
                    }

                    // Add the match to the list
                    matches.Add(match);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(TAG + "Error parsing GetMatchList response from matchmaking server." + e, gameObject);
                success = false;
            }
            
            if (onMatchList != null)
            {
                onMatchList(success, matches.ToArray());
            }
        }
        
        /// <summarySend a command to the matchmaking server.</summary>
        void SendCommand(Command command, string textToSend = "", Action<bool, Transaction> onResponse = null)
        {
            var request = new Message(command, textToSend);
            StartCoroutine(SendCommandAsync(request, onResponse));
        }
        
        /// <summary>Send a Command to the matchmaking server and wait for a response.</summary>
        /// <remarks>
        /// Creates a transaction for the request if a response is expected.
        /// Also starts a coroutine to timeout the transaction if no response is received.
        /// </remarks>
        IEnumerator SendCommandAsync(Message request, Action<bool, Transaction> onResponse = null)
        {
            if (matchmakingClient == null) yield break;

            string transactionID = "";
            if (onResponse != null)
            {
                transactionID = Transaction.GenerateID();
                var transaction = new Transaction(transactionID, request, onResponse);
                transactions[transactionID] = transaction;
                StartCoroutine(TimeoutTransaction(transaction));
            }

            // If the command has a payload it is separated by a pipe
            string textToSend = (int)request.command + "|" + transactionID + "|" + request.payload;

            // Send the command
            byte[] bytesToSend = Encoding.ASCII.GetBytes(textToSend);
            networkStream.Write(bytesToSend, 0, bytesToSend.Length);
        }

        /// <summary>Wait to see if a transaction times out.</summary>
        /// <remarks>
        /// If no response is received then the transaction has failed.
        /// The transaction's onResponse method will be called with success = false.
        /// </remarks>
        /// <param name="transaction"></param>
        /// <returns></returns>
        IEnumerator TimeoutTransaction(Transaction transaction)
        {
            yield return new WaitForSeconds(timeout);

            // One last chance to complete the transaction
            // This helps prevent false negatives caused by long scene loads
            // Without the extra read the transaction will appear to have timed out
            // when the scene finishes loading because the check below runs before
            // the Update() loop that would normally read the message.
            ReadData();

            if (!transaction.isComplete)
            {
                transactions.Remove(transaction.transactionID);
                transaction.Timeout();
            }
        }

        /// <summary>Deserialize the match data string returned from the matchmaking server.</summary>
        Dictionary<string, MatchData> parseMatchData(string text, int startPos, ref int endPos)
        {
            var matchData = new Dictionary<string, MatchData>();
            
            // Number of match data entries
            endPos = text.IndexOf('|', startPos);
            int numDatas = int.Parse(text.Substring(startPos, (endPos - startPos)));
            startPos = endPos + 1;

            int count = 0;
            while (endPos != -1 && endPos < text.Length && count < numDatas)
            {
                // Value type
                endPos = text.IndexOf(",", startPos);
                string typeString = text.Substring(startPos, (endPos - startPos));
                int typeID = int.Parse(typeString);
                MatchData.ValueType type = (MatchData.ValueType)typeID;
                startPos = endPos + 1;

                // Key
                endPos = text.IndexOf(",", startPos);
                string key = text.Substring(startPos, (endPos - startPos));
                startPos = endPos + 1;

                // Value
                string valueText;
                if (type == MatchData.ValueType.STRING)
                {
                    valueText = parseQuoted(text, startPos, ref endPos);
                    if (endPos == -1)
                    {
                        return null;
                    }
                    startPos = endPos + 1;
                }
                else
                {
                    endPos = text.IndexOf("|", startPos);
                    if (endPos == -1) endPos = text.Length;
                    valueText = text.Substring(startPos, (endPos - startPos));
                    startPos = endPos + 1;
                }
                
                matchData[key] = new MatchData(valueText, type); 

                count++;
            }

            return matchData;
        }
        
        /// <summary>Parse a quoted bit of text, skipping escaped quotes.</summary>
        /// <remarks>
        /// Returns the quoted string with the outer quotes removed.
        /// Sets endPos equal to the index of the closing quote.
        /// </remarks>
        string parseQuoted(string text, int startPos, ref int endPos)
        {
            startPos++; // For starting quote
            int lastQuotePos = startPos;
            while (true)
            {
                endPos = text.IndexOf('"', lastQuotePos);
                lastQuotePos = endPos + 1;
                if (endPos == -1)
                {
                    Debug.LogError(TAG + "Failed parsing match data string. Found open quote with no closing quote.");
                    return "";
                }
                if (text[endPos - 1] != '\\')
                {
                    // Found the non-escaped closing quote
                    break;
                }
            }
            string parsed = text.Substring(startPos, (endPos - startPos));
            parsed = parsed.Replace("\\\"", "\""); // Replace escaped quotes with quotes
            endPos = endPos + 1; // For ending quote
            return parsed;
        }

        #endregion
    }
}
