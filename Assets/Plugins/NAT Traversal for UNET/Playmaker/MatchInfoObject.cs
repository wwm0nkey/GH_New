using System.Collections.Generic;
using UnityEngine.Networking.Types;

namespace UnityEngine.Networking.Match
{
    //
    // Summary:
    //     A class describing the match information as a snapshot at the time the request
    //     was processed on the MatchMaker.
    public class MatchInfoObject : Object
    {

        public MatchInfoObject()
        {

        }

#if UNITY_5_3
        public MatchInfoObject(MatchDesc snapshot)
#else
        public MatchInfoObject(MatchInfoSnapshot snapshot)
#endif
        {
            this.networkId = snapshot.networkId;
            this.hostNodeId = snapshot.hostNodeId;
            this.name = snapshot.name;
            this.averageEloScore = snapshot.averageEloScore;
            this.maxSize = snapshot.maxSize;
            this.currentSize = snapshot.currentSize;
            this.isPrivate = snapshot.isPrivate;
            this.matchAttributes = snapshot.matchAttributes;
            this.directConnectInfos = snapshot.directConnectInfos;
        }

        //
        // Summary:
        //     The network ID for this match.
        public NetworkID networkId;
        //
        // Summary:
        //     The NodeID of the host for this match.
        public NodeID hostNodeId;
        //
        // Summary:
        //     The text name for this match.
        new public string name;
        //
        // Summary:
        //     The average Elo score of the match.
        public int averageEloScore;
        //
        // Summary:
        //     The maximum number of players this match can grow to.
        public int maxSize;
        //
        // Summary:
        //     The current number of players in the match.
        public int currentSize;
        //
        // Summary:
        //     Describes if the match is private. Private matches are unlisted in ListMatch
        //     results.
        public bool isPrivate;
        //
        // Summary:
        //     The collection of match attributes on this match.
        public Dictionary<string, long> matchAttributes;
        //
        // Summary:
        //     The collection of direct connect info classes describing direct connection information
        //     supplied to the MatchMaker.
#if UNITY_5_3
        public List<MatchDirectConnectInfo> directConnectInfos;
#else
        public List<MatchInfoSnapshot.MatchInfoDirectConnectSnapshot> directConnectInfos;
#endif

    }
}