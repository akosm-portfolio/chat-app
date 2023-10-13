using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Net.Sockets;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;

namespace Client
{

    public partial class CliensMessage : Window
    {
        public Socket ClientSocket;
        public string LoginName;
        byte[] byteData = new byte[1024];

        private bool sendingFile = false;
        FileInfo fileInfo = null;

        private delegate void UpdateDelegate(string pMessage);

        private void UpdateMessage(string pMessage)
        {
            this.textBox1.Text += pMessage;
        }

        public CliensMessage()
        {
            InitializeComponent();
            textBox2.Focus();
        }

        public CliensMessage(Socket pSocket, String pName)
        {
            InitializeComponent();

            ClientSocket = pSocket;
            LoginName = pName;
            this.Title = pName;

            ClientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                    new AsyncCallback(OnReceive), ClientSocket);
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

                switch (msgReceived.cmdCommand)
                {
                    case Command.File:

                        string[] fileInfo = msgReceived.strMessage.Split(';');
                        string fileName = fileInfo[0];
                        int fileSize = int.Parse(fileInfo[1]);
                        byte[] buffer = new byte[fileSize];

                        ClientSocket.Receive(buffer, SocketFlags.None);
                        try
                        {
                            System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/ChatProgram");
                            string pathToSave = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/ChatProgram/" + fileName;
                            File.WriteAllBytes(pathToSave, buffer);
                            Console.WriteLine($"{fileName} was created.\n");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("An error occurred while receiving the file!", "Failed to receive file");
                        }
                        

                        UpdateDelegate update1 = new UpdateDelegate(UpdateMessage);
                        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, update1,
                                msgReceived.strSender + ": sent a file\n" + fileName + " saved to Documents/ChatProgram folder" + "\r\n");

                        ClientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                                        new AsyncCallback(OnReceive), ClientSocket);
                        break;

                    case Command.List:
                        UpdateSendToComboBox(msgReceived.strMessage);
                        Console.WriteLine("userlist updated");
                        break;

                    default:
                        UpdateDelegate update = new UpdateDelegate(UpdateMessage);
                        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, update,
                                msgReceived.strMessage + "\r\n");
                        break;
                }

                if (msgReceived.cmdCommand != Command.File)
                    ClientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                                            new AsyncCallback(OnReceive), ClientSocket);

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message, "client");
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (textBox2.Text.Length < 1)
                return;

            if (sendingFile && fileInfo != null)
            {
                try
                {
                    UpdateDelegate update = new UpdateDelegate(UpdateMessage);
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, update,
                            $"You: {fileInfo.Name} file sent to {SendToComboBox.SelectedItem.ToString()}" + "\r\n");

                    SendSocketMessage(Command.File, fileInfo.Name + ";" + fileInfo.Length + ";**", SendToComboBox.SelectedItem.ToString());
                    textBox2.IsReadOnly = true;

                    byte[] buffer = File.ReadAllBytes(textBox2.Text);

                    ClientSocket.Send(buffer, SocketFlags.None);
                    fileInfo = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured while sending the file!", "Failed to receive file");
                }
                
            }
            else
            {
                SendSocketMessage(Command.Message, textBox2.Text, this.SendToComboBox.Text);
                UpdateDelegate update = new UpdateDelegate(UpdateMessage);
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, update,
                    $"You: {textBox2.Text}" + "\r\n");
            }
            textBox2.Text = "";
            textBox2.IsReadOnly = false;
        }

        private void UpdateSendToComboBox(string usersStr)
        {

            this.Dispatcher.Invoke((Action)(() =>
            {
                string[] users = usersStr.Split('*');

                SendSocketMessage(Command.List, null, null);

                ComboBox cb = this.SendToComboBox;
                string preSelected = cb.Text;
                cb.Items.Clear();
                cb.Items.Add("Everyone");

                foreach (var user in users)
                {
                    if (user.Length > 0 && !user.Equals(LoginName))
                        cb.Items.Add(user);
                }
                cb.SelectedIndex = cb.Items.IndexOf(preSelected);
            }));
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            SendSocketMessage(Command.Logout, null, null);
            Close();
        }

        private void SendSocketMessage(Command command, string msg, string recipient)
        {
            try
            {
                Data msgToSend = new Data();
                msgToSend.cmdCommand = command;

                msgToSend.strSender = LoginName;
                msgToSend.strRecipient = recipient;
                msgToSend.strMessage = msg;

                byte[] b = msgToSend.ToByte();
                ClientSocket.Send(b);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "client");
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SendSocketMessage(Command.Logout, null, null);
        }

        private void OnKeyDownHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Send_Click(sender, e);
            }
        }

        private void File_Click(object sender, RoutedEventArgs e)
        {
            //var fileContent = string.Empty;
            var filePath = string.Empty;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            textBox2.Text = "";
            textBox2.IsReadOnly = false;
            sendingFile = false;

            if (openFileDialog.ShowDialog() == true)
            {
                sendingFile = true;
                //Get the path of specified file
                filePath = openFileDialog.FileName;
                fileInfo = new FileInfo(filePath);

                textBox2.Text = filePath;
                textBox2.IsReadOnly = true;
            }
            textBox2.Focus();
        }
    }
}
