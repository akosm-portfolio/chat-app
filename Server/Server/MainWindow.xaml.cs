using System;
using System.Windows;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Server
{
    public partial class MainWindow : Window
    {
        struct ClientInfo
        {
            public Socket socket;   //Socket of the client
            public string strName;  //Name by which the user logged into the chat room
        }

        ArrayList clientList;
        Socket serverSocket;
        byte[] byteData = new byte[1024];


        public MainWindow()
        {
            clientList = new ArrayList();
            InitializeComponent();

        }

        private delegate void UpdateDelegate(string pMessage);

        private void UpdateMessage(string pMessage)
        {
            this.textBox1.Text += pMessage;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        { 
            try
            {
                //enable when generating new users:
                //GenerateUserData.generateUsers();

                // using TCP sockets
                serverSocket = new Socket(AddressFamily.InterNetwork,
                                          SocketType.Stream,
                                          ProtocolType.Tcp);

                //Assign the any IP of the machine and listen on port number 1000
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 1000);

                //Bind and listen on the given address
                serverSocket.Bind(ipEndPoint);
                serverSocket.Listen(4);

                //Accept the incoming clients
                serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "serverTCP");
            }
        }
        private void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = serverSocket.EndAccept(ar);

                //Start listening for more clients
                serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);

                //Once the client connects then start receiving the commands from her
                clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                    new AsyncCallback(OnReceive), clientSocket);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "serverTCP");
            }
        }

        private bool CheckUser(string username, string message)
        {
            using (var reader = new StreamReader("userdata.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');           

                    if (username != null && username.Equals(values[0]))
                    {
                        string salt = values[2];
                        string saltedHashResult = EncryptPassword.GenerateSaltedHash(message, salt);
                        if (saltedHashResult.Equals(values[1]))
                        {
                            //accept login
                            return true;
                        } 
                    }
                }
            }
            //decline login
            return false;
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = (Socket)ar.AsyncState;
                clientSocket.EndReceive(ar);

                //Transform the array of bytes received from the user into an
                //intelligent form of object Data
                Data msgReceived = new Data(byteData);

                Data msgToSend = new Data();

                byte[] message;

                msgToSend.strSender = msgReceived.strSender;
                msgToSend.strRecipient = msgReceived.strRecipient;

                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:
                        
                        if (!CheckUser(msgReceived.strSender, msgReceived.strMessage))
                        {
                            msgToSend.cmdCommand = Command.Decline;
                            msgToSend.strMessage = "";
                            message = msgToSend.ToByte();
                            clientSocket.BeginSend(message, 0, message.Length, SocketFlags.None,
                                new AsyncCallback(OnSend), clientSocket);

                            clientSocket.Disconnect(true);
                            return;
                        }
                        msgToSend.cmdCommand = Command.Accept;
                        ClientInfo clientInfo = new ClientInfo();
                        clientInfo.socket = clientSocket;
                        clientInfo.strName = msgReceived.strSender;

                        clientList.Add(clientInfo);

                        msgToSend.strMessage = "";
                        message = msgToSend.ToByte();
                        clientSocket.BeginSend(message, 0, message.Length, SocketFlags.None,
                            new AsyncCallback(OnSend), clientSocket);

                        //Set the text of the message that we will broadcast to all users
                        msgToSend.cmdCommand = Command.Message;
                        msgToSend.strRecipient = "Everyone";
                        msgToSend.strMessage = "<<<" + msgReceived.strSender + " joined>>>";
                        break;

                    case Command.Logout:

                        //When a user wants to log out of the server then we search for her 
                        //in the list of clients and close the corresponding connection

                        
                        int nIndex = 0;
                        foreach (ClientInfo client in clientList)
                        {
                            if (client.socket == clientSocket)
                            {
                                clientList.RemoveAt(nIndex);
                                break;
                            }
                            ++nIndex;
                        }

                        clientSocket.Close();
                        msgToSend.cmdCommand = Command.Message;
                        msgToSend.strRecipient = "Everyone";
                        msgToSend.strMessage = "<<<" + msgReceived.strSender + " left>>>";
                        break;

                    case Command.Message:

                        msgToSend.cmdCommand = Command.Message;
                        msgToSend.strRecipient = msgReceived.strRecipient;
                        //Set the text of the message that we will broadcast to all users
                        msgToSend.strMessage = msgReceived.strSender + ": " + msgReceived.strMessage;
                        break;


                    case Command.File:
                        string[] fileInfo = msgReceived.strMessage.Split(';');
                        byte[] buffer = new byte[int.Parse(fileInfo[1])];

                        clientSocket.Receive(buffer, SocketFlags.None);

                        if (msgReceived.strRecipient.Equals("Everyone")) //sending file to every user
                        {
                            foreach (ClientInfo client in clientList)
                            {
                                if (!client.strName.Equals(msgReceived.strSender))
                                {
                                    msgToSend.strRecipient = msgReceived.strRecipient;
                                    msgToSend.strSender = msgReceived.strSender;
                                    msgToSend.cmdCommand = Command.File;
                                    msgToSend.strMessage = msgReceived.strMessage;
                                    client.socket.Send(msgToSend.ToByte(), SocketFlags.None);

                                    client.socket.Send(buffer, SocketFlags.None);
                                    Console.WriteLine("FILE SENT");
                                }     
                            }
                        }
                        else
                        {
                            foreach (ClientInfo client in clientList)
                            {
                                if (client.strName.Equals(msgReceived.strRecipient))
                                {
                                    msgToSend.strRecipient = msgReceived.strRecipient;
                                    msgToSend.strSender = msgReceived.strSender;
                                    msgToSend.cmdCommand = Command.File;
                                    msgToSend.strMessage = msgReceived.strMessage;
                                    client.socket.Send(msgToSend.ToByte(), SocketFlags.None);

                                    client.socket.Send(buffer, SocketFlags.None);
                                    Console.WriteLine("FILE SENT");
                                    break;
                                }
                            }
                        }
                        UpdateDelegate update = new UpdateDelegate(UpdateMessage);
                        this.textBox1.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, update,
                            $"{msgToSend.strSender}: file sent -> to {msgToSend.strRecipient}{Environment.NewLine}");

                        break;

                    case Command.List:
                        msgToSend.cmdCommand = Command.List;
                        break;

                }

                if (msgToSend.cmdCommand != Command.List && msgReceived.cmdCommand != Command.File)   //List messages are not broadcasted
                {
                    message = msgToSend.ToByte();

                    if (msgToSend.strRecipient.Equals("Everyone"))
                    {
                        foreach (ClientInfo clientInfo in clientList)
                        {
                            if (clientInfo.strName != msgToSend.strSender)
                            {
                                clientInfo.socket.Send(message, 0, message.Length, SocketFlags.None);
                            }
                        }
                    }
                    else
                    {
                        foreach(ClientInfo clientInfo in clientList)
                        {

                            if (clientInfo.socket != clientSocket)
                            {
                                if(clientInfo.strName.Equals(msgToSend.strRecipient))
                                    clientInfo.socket.Send(message, 0, message.Length, SocketFlags.None);
                            }
                        }
                    }


                    UpdateDelegate update = new UpdateDelegate(UpdateMessage);
                    this.textBox1.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, update,
                        msgToSend.strMessage + " -> to " + msgToSend.strRecipient + Environment.NewLine);

                }

                //If the user is logging out then we need not listen from her
                if (msgReceived.cmdCommand != Command.Logout)
                {
                    //Start listening to the message send by the user
                    clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
                }

                if (msgReceived.cmdCommand == Command.Login || msgReceived.cmdCommand == Command.Logout)
                    sendUserList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "serverTCP");
            }
        }

        public void OnSend(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndSend(ar);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "serverTCP");
            }
        }

        private void sendUserList()
        {
            Data msgToSend = new Data();
            byte[] message;
            //Send the names of all users in the chat room to the new user
            msgToSend.cmdCommand = Command.List;
            msgToSend.strSender = null;
            msgToSend.strMessage = null;

            //Collect the names of the user in the chat room
            foreach (ClientInfo client in clientList)
            {
                //To keep things simple we use asterisk as the marker to separate the user names
                msgToSend.strMessage += client.strName + "*";
                
            }
            message = msgToSend.ToByte();
            Thread.Sleep(1500);
            foreach(ClientInfo clientInfo in clientList)
            {
                Console.WriteLine($"Sending userlist to: {clientInfo.strName}");
                clientInfo.socket.BeginSend(message, 0, message.Length, SocketFlags.None,
                    new AsyncCallback(OnSend), clientInfo.socket);
            }
        }
    }
}
