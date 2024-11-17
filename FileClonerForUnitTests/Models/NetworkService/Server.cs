/******************************************************************************
 * Filename    = Server.cs
 *
 * Author(s)   = Sai Hemanth Reddy, Neeraj Krishna N
 *
 * Project     = FileCloner
 *
 * Description = Manages server-side communication for the FileCloner application,
 *               handling client connections, message broadcasting, and targeted
 *               communication between clients. The Server class maintains a list
 *               of connected clients and ensures messages are delivered to the
 *               appropriate recipients.
 *****************************************************************************/

using Networking.Communication;
using System.Net.Sockets;
using System.Net;
using Networking;
using Networking.Serialization;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace FileCloner.Models.NetworkService;

/// <summary>
/// The Server class manages incoming and outgoing communication with clients,
/// handling data routing, client management, and logging for the FileCloner application.
/// </summary>
public class Server : INotificationHandler
{
    // Instance of the server communicator for managing connections
    private static CommunicatorServer s_server =
        (CommunicatorServer)CommunicationFactory.GetCommunicator(isClientSide: false);

    // Counter for assigning unique IDs to clients as they join
    private static int s_clientid = 0;

    // Dictionary to store the mapping of client IP addresses to their unique IDs
    private static Dictionary<string, string> s_clientList = new();
    // Lock object for synchronizing access to clientList
    private static readonly object s_clientListLock = new object();

    // Serializer for handling message serialization and deserialization
    private static ISerializer s_serializer = new Serializer();

    // Delegate for logging actions, e.g., writing to UI or console
    private Action<string> _logAction;

    private static Server s_instance;
    private static readonly object s_lock = new object();

    /// <summary>
    /// Initializes the server, starts listening on the specified port,
    /// and subscribes to the message handler for the module.
    /// </summary>
    /// <param name="logAction">Delegate for logging status updates and errors.</param>
    private Server()
    {
        s_server.Subscribe(Constants.ModuleName, this, false);
    }

    /// <summary>
    /// Gets the singleton instance of the Server class, optionally updating the log action.
    /// </summary>
    /// <param name="logAction">Delegate for logging status updates and errors (optional).</param>
    /// <returns>The singleton instance of the Server class.</returns>
    public static Server GetServerInstance(Action<string> logAction = null)
    {
        lock (s_lock)
        {
            if (s_instance == null)
            {
                s_instance = new Server();
            }

            // Update the logAction if a new one is provided
            if (logAction != null)
            {
                s_instance._logAction = logAction;
            }
        }

        return s_instance;
    }

    public void SetUser(string clientId, TcpClient socket)
    {
        string clientIpAddress = ((IPEndPoint)socket.Client.RemoteEndPoint).Address.ToString();

        lock (s_clientListLock) // Synchronize access to clientList
        {
            if (s_clientList.ContainsKey(clientIpAddress))
            {
                s_clientList[clientIpAddress] = clientId; // Update the client ID for the existing IP
            }
            else
            {
                s_clientList.Add(clientIpAddress, clientId); // Add a new entry
            }
        }

        _logAction?.Invoke($"[Server] {clientIpAddress} Joined/Updated");
    }

    /// <summary>
    /// Handles data received from clients, determines if it's a broadcast or
    /// directed message, and routes it accordingly.
    /// </summary>
    /// <param name="serializedData">The serialized data received from a client.</param>
    public void OnDataReceived(string serializedData)
    {
        try
        {
            // Deserialize the message to process its details
            Message message = s_serializer.Deserialize<Message>(serializedData);

            if (message == null)
            {
                return;
            }
            _logAction?.Invoke($"[Server] {message.Subject} {message.From} {message.To}");
            // Handle broadcast or targeted messages
            if (message.To == Constants.Broadcast)
            {
                s_server.Send(serializedData, Constants.ModuleName, null); // Broadcast to all
            }
            else
            {
                string targetClientId;

                lock (s_clientListLock) // Synchronize access to clientList
                {
                    // Safely fetch the target client ID
                    if (!s_clientList.TryGetValue(message.To, out targetClientId))
                    {
                        _logAction?.Invoke($"[Server] Target client {message.To} not found");
                        return;
                    }
                }

                s_server.Send(serializedData, Constants.ModuleName, targetClientId); // Send to the target client
            }
        }
        catch (Exception e)
        {
            _logAction?.Invoke("[Server] Error in sending data: " + e.Message);
        }
    }

    /// <summary>
    /// Removes a client from the client list when they disconnect.
    /// </summary>
    /// <param name="clientId">The unique ID of the client that left.</param>
    public void OnClientLeft(string clientId)
    {
        lock (s_clientListLock) // Synchronize access to clientList
        {
            // Find the client in the dictionary by clientId
            KeyValuePair<string, string> clientEntry = s_clientList.FirstOrDefault(entry => entry.Value == clientId);
            if (!string.IsNullOrEmpty(clientEntry.Key))
            {
                _logAction?.Invoke($"[Server] {s_clientList[clientEntry.Key]} Left");
                s_clientList.Remove(clientEntry.Key); // Safely remove the client
            }
        }
    }

    /// <summary>
    /// Stops the server and terminates all client connections.
    /// </summary>
    public void Stop()
    {
        s_server.Stop();
    }
}
