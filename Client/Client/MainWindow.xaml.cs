using System;
using System.Windows;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    public partial class MainWindow : Window
    {
        
        public Socket clientSocket;
        public string strName;

        public delegate string getNameDelegate();
        public delegate void UjFormDelegate(); 

        public MainWindow()
        {
            InitializeComponent();
            this.textBox1.Focus();
            this.textBox3.Text = "localhost";
        }

        public string getLoginName() 
        {
            return this.textBox1.Text;
        }

        public string getPassword()
        {
            return this.textBox2.Password;
        }

        public string getIP()
        {
            return this.textBox3.Text;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                if (!this.textBox3.Text.Equals("localhost"))
                    ipAddress = IPAddress.Parse(this.textBox3.Text);
                    
                //Server is listening on port 1000
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

                clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "client");
            } 
        }

        private void OnReceive(IAsyncResult ar)
        {
        }
        private void OnSend(IAsyncResult ar)
        {
            try
            {           
                clientSocket.EndSend(ar);
                byte[] byteData = new byte[1024];
                
                clientSocket.Receive(byteData,0,1024,SocketFlags.None);
                
                Data msg = new Data(byteData);

                if (msg.cmdCommand == Command.Decline)
                {
                    MessageBox.Show("Bas username or password", "Error!");
                }
                else if(msg.cmdCommand == Command.Accept)
                {
                    UjFormDelegate pForm = new UjFormDelegate(UjForm);
                    this.Dispatcher.Invoke(pForm, null);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "client");
            }
        }

        private void UjForm()
        {
            CliensMessage uj_form;
            uj_form = new CliensMessage(clientSocket,textBox1.Text);
            uj_form.Show();
            Close();
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndConnect(ar);
                string l_fhName;
                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Login;

                getNameDelegate fhName = new getNameDelegate(getLoginName);
                l_fhName = (string)this.textBox1.Dispatcher.Invoke(fhName, null);

                msgToSend.strSender = l_fhName;
                msgToSend.strRecipient = null;
                msgToSend.strMessage = textBox2.Password;

                byte[] b = msgToSend.ToByte();

                //Send the message to the server
                clientSocket.BeginSend(b, 0, b.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "client");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OnKeyDownHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                Login_Click(sender, e);
            }
        }
    }
}
