using System.Collections.Generic;

namespace MatchUp
{
    /// <summary>Info about a match</summary>
    /// <remarks>
    /// When you call CreateMatch() or GetMatchList() the Match object(s) will be created for you.
    /// matchData and clientID will not be set on clients until after the match is joined.
    /// </remarks>
    public class Match
    {
        /// <summary>The name of the match</summary>
        /// <remarks>
        /// This should work fine with spaces and quotes and most special characters. Go nuts.
        /// </remarks>
        string name = " ";

        /// <summary>The id of the match</summary>
        /// <remarks>
        /// This will be the same for all clients in the match.
        /// </summary>
        public long id;

        /// <summary>The id of this particular client.</summary>
        /// <remarks>
        /// This will be different for each client in the match.
        /// clientID is not set on clients until after the match is joined.
        /// </remarks>
        public long clientID;

        /// <summary>Data related to the match</summary>
        /// <remarks>
        /// This can be anything you want, but generally it is used to store the host's connection info.
        /// </remarks>
        public Dictionary<string, MatchData> matchData = new Dictionary<string, MatchData>();

        /// <summary>Create a new match.</summary>
        /// <remarks>
        /// You should never need to do this yourself. Matches are constructed for you by the
        /// Matchmaking when you call CreateMatch() or GetMatchList()
        /// </remarks>
        /// <param name="matchID"></param>
        /// <param name="matchData"></param>
        public Match(long matchID = -1, Dictionary<string, MatchData> matchData = null)
        {
            this.id = matchID;
            this.matchData = matchData;
        }

        /// <summary>Serialize the match for sending to the matchmaking server.</summary>
        /// <remarks>
        /// Escapes quotes in the match name and serializes the data.
        /// You should not be using this directly unless really know what you're doing.
        /// </remarks>
        /// <param name="maxClients">The maximum number of clients that will be allowed to join this match.</param>
        /// <returns></returns>
        public string Serialize(int maxClients)
        {
            string escapedMatchName = "\"" + name.Replace("\"", "\\\"") + "\"";
            string s = escapedMatchName + "," + maxClients;
            if (matchData != null && matchData.Count > 0)
            {
                s += "|" + MatchData.SerializeDict(matchData);
            }
            return s;
        }
    }
}
