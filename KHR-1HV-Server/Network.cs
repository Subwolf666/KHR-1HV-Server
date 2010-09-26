using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;

namespace Server
{
    class Network
    {
        private static Logging Log = new Logging();
        private static string Module = "Network.cs";

        private IPAddress ipAddress; // Will store the IP address passed to it
        private static TcpClient tcpClient = new TcpClient();

        private Thread thrListener; // The thread that will hold the connection listener

        private TcpListener tlsClient; // The TCP object that listens for connections

        bool ServRunning = false; // Will tell the while loop to keep monitoring for connections

        // The constructor sets the IP address to the one retrieved by the instantiating object
        public Network()
        {
            Log.Module = Module;
            string myHost = System.Net.Dns.GetHostName();
            Log.WriteLineMessage(string.Format("Hostname: {0}", myHost));

            string IP4Address = string.Empty;
            foreach (IPAddress IPA in System.Net.Dns.GetHostAddresses(myHost))
            {
                if (IPA.AddressFamily.ToString() == "InterNetwork")
                {
                    IP4Address = IPA.ToString();
                    break;
                }
            }
            Log.WriteLineMessage(string.Format("Server IP Address: {0}", IP4Address));
            ipAddress = IPAddress.Parse(IP4Address);
        }

        // Send messages from one user to all the others
        public static void SendMessage(string Message)
        {
            StreamWriter swSenderSender;

            try
            {
                // If the message is blank or the connection is null, break out
                if (Message.Trim() == "" || tcpClient == null)
                {
                    return;
                }
                // Send the message to the current user in the loop
                swSenderSender = new StreamWriter(tcpClient.GetStream());
                swSenderSender.WriteLine(Message);
                swSenderSender.Flush();
                swSenderSender = null;
                Log.WriteLineMessage(Message);
            }
            catch (Exception ex)// If there was a problem, the user is not there anymore
            {
                Log.WriteLineFail(ex.Message);
            }
        }

        public void StartListening()
        {
            // Get the IP of the first network device, however this can prove unreliable on certain configurations
            IPAddress ipaLocal = ipAddress;

            // Create the TCP listener object using the IP of the server and the specified port
            tlsClient = new TcpListener(ipaLocal, 1986);

            // Start the TCP listener and listen for connections
            tlsClient.Start();

            // The while loop will check for true in this before checking for connections
            ServRunning = true;

            // Start the new tread that hosts the listener
            thrListener = new Thread(KeepListening);
            thrListener.Start();
        }

        private void KeepListening()
        {
            // While the server is running
            while (ServRunning == true)
            {
                try
                {
                    // Accept a pending connection
                    tcpClient = tlsClient.AcceptTcpClient();
                    // Maybe make ServRunning = false so no other client can connect while connected
                    // Create a new instance of Connection
                    Connection newConnection = new Connection(tcpClient);
                }
                catch (Exception ex)
                {
                    Log.WriteLineFail(ex.Message);
                }
            }
        }

        public void StopListening()
        {
            try
            {
                ServRunning = false;
                tlsClient.Stop();
            }
            catch (Exception ex)
            {
                Log.WriteLineFail(ex.Message);
            }
        }

        //TODO
        private static void ontvangbericht(string str)
        {
            OnNewMessage(str);
        }

        // Now, create a public event "NewMessageEventHandler" 
        // whose type is our NewMessageEventHandler.
        //
        public delegate void NewMessageEventHandler(object sender, NewMessageEventsArgs e);

        public static event NewMessageEventHandler messageHandler;

        public static void OnNewMessage(string str)
        {
            NewMessageEventsArgs MessageEvents = new NewMessageEventsArgs(str);
            if (messageHandler != null)
            {
                // Invoke the delegate
                messageHandler(null, MessageEvents);
            }
        }
    }

//=================================================================

    //
    // public delegate void NewMessageEventHandler(object sender, NewMessageEventsArgs e);
    // This class handels connections; there will be as many instances of it as there will be connected users
    class Connection
    {
        Logging Log = new Logging();
        TcpClient tcpClient;
        // The thread that will send information to the client
        private Thread thrSender;
        private StreamReader srReceiver;
        private StreamWriter swSender;
        private string connectMsg;
        private string strResponse;

        // The constructor of the class takes in a TCP connection
        public Connection(TcpClient tcpCon)
        {
            tcpClient = tcpCon;
            // The thread that accepts the client and awaits messages
            thrSender = new Thread(AcceptClient);
            // The thread calls the AcceptClient() method
            thrSender.Start();
        }

        private void CloseConnection()
        {
            // Close the currently open objects
            tcpClient.Close();
            srReceiver.Close();
            swSender.Close();
        }

        // Occures when a new client is accepted
        private void AcceptClient()
        {
            srReceiver = new System.IO.StreamReader(tcpClient.GetStream());
            swSender = new System.IO.StreamWriter(tcpClient.GetStream());

            // Read the account information from the client
            try
            {
                connectMsg = srReceiver.ReadLine();
            }
            catch (Exception ex)
            {
                Log.WriteLineFail(ex.Message);
            }

            // We got a response from the client
            //if (connectMsg != null)
            if (connectMsg == "KHR-1HV")
            {
                // 1 means connected successfully
                swSender.WriteLine("1");
                swSender.Flush();
            }
            else
            {
                CloseConnection();
                return;
            }

            try
            {
                // Keep waiting for a message from the user
                while ((strResponse = srReceiver.ReadLine()) != "")
                {
                    if (strResponse != null)
                    {
                        // send the message to the users
                        Network.OnNewMessage(strResponse);
                    }
                }
            }
            catch(Exception ex)
            {
                // If anything went wrong with this user.
                Log.WriteLineFail(ex.Message);
            }
        }
    }
//=============================================================================

    // Constructor for setting the event message
    public class NewMessageEventsArgs : EventArgs
    {
        private string EventMsg;

        public string NewMessage
        {
            get
            {
                return EventMsg;
            }
            set
            {
                EventMsg = value;
            }
        }

        public NewMessageEventsArgs(string strEventMsg)
        {
            this.EventMsg = strEventMsg;
        }
    }
}
