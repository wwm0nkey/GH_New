using System.Collections;
using System.Net;
using System.Net.Sockets;
#if !UNITY_5_3
using System;
using System.Net.NetworkInformation;
#endif

namespace UnityEngine.Networking
{
    /// <summary>Some useful extensions to the NetworkManager class</summary>
    public static class NetworkManagerExtension
    {
        /// <summary>The externalIP which is fetched from an external server</summary>
        public static string externalIP;

        /// <summary>Adds a version of StartClient to the NetworkManager that accepts a Match.</summary>
        /// <param name="manager">The NetworkManager instance</param>
        /// <param name="match">The Match whose host we should connect to</param>
        public static void StartClient(this NetworkManager manager, MatchUp.Match match)
        {
            // Get the connection info from the Match's MatchData
            string externalIP, internalIP;
            externalIP = match.matchData["externalIP"];
            internalIP = match.matchData["internalIP"];

            manager.networkPort = match.matchData["port"];
            manager.networkAddress = pickCorrectAddressToConnectTo(externalIP, internalIP);
            manager.StartClient();
        }

        /// <summary>Fetch the external IP</summary>
        /// <param name="ipSource">The url from which to fetch the IP</param>
        public static IEnumerator FetchExternalIP(string ipSource)
        {
            WWW www = new WWW(ipSource);
            yield return www;
            externalIP = IPAddress.Parse(www.text.Trim()).ToString();
        }

        /// <summary>Select between internal and external IP.</summary>
        /// <remarks>
        /// Most of the time we connect to the externalIP but when connecting to another PC on the same local network or 
        /// another build on the same computer we need to use the local address or localhost instead
        /// </remarks>
        /// <param name="hostExternalIP">The host's external IP</param>
        /// <param name="hostInternalIP">The host's internal IP</param>
        /// <returns></returns>
        static string pickCorrectAddressToConnectTo(string hostExternalIP, string hostInternalIP)
        {
            if (hostExternalIP == externalIP && !string.IsNullOrEmpty(hostInternalIP))
            {
                // Client and host are behind the same router
                if (hostInternalIP == GetLocalAddress(AddressFamily.InterNetwork))
                {
                    // Host is running on the same computer as client, two separate builds
                    if (LogFilter.currentLogLevel == 0) Debug.Log("Using localhost address.");
                    return "127.0.0.1";
                }
                else
                {
                    // Host is on the same local network as client
                    if (LogFilter.currentLogLevel == 0) Debug.Log("Using host's local ip address.");
                    return hostInternalIP;
                }
            }
            else
            {
                // Host is somewhere out on the internet
                if (LogFilter.currentLogLevel == 0) Debug.Log("Using host's external ip address.");
                return hostExternalIP;
            }
        }

        /// <summary>
        /// Gets the a local address by looping through all network interfaces and returning first address from the first interface whose OperationalStatus is Up and whose
        /// address family matches the provided family.
        /// </summary>
        /// <returns>The local address as a string or an empty string if there is none</returns>
        public static string GetLocalAddress(AddressFamily family)
        {
#if UNITY_5_3
            return Network.player.ipAddress;
#else
            try
            {
                foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (item.OperationalStatus == OperationalStatus.Up)
                    {
                        foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == family)
                            {
                                return ip.Address.ToString().Trim();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return "";
            }

            return "";
#endif
        }

    }
}
