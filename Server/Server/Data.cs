using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    enum Command
    {
        Login,      //Log into the server
        Logout,     //Logout of the server
        Message,    //Send a text message to all the chat clients
        List,       //Get a list of users in the chat room from the server
        File,       //File information
        Accept,     //Accept login request
        Decline,    //Decline login request
        Null        //No command
    }

    class Data
    {
        //Default constructor
        public Data()
        {
            this.cmdCommand = Command.Null;
            this.strMessage = null;
            this.strSender = null;
            this.strRecipient = null;
        }

        //Converts the bytes into an object of type Data
        public Data(byte[] data)
        {
            //The first four bytes are for the Command
            this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);

            //The next four store the length of the name
            int senderLen = BitConverter.ToInt32(data, 4);

            //The next four store the length of the name
            int recipientLen = BitConverter.ToInt32(data, 8);

            //The next four store the length of the message
            int msgLen = BitConverter.ToInt32(data, 12);

            //This check makes sure that strName has been passed in the array of bytes
            if (senderLen > 0)
                this.strSender = Encoding.Unicode.GetString(data, 16, senderLen * 2);
            else
                this.strSender = null;

            if (recipientLen > 0)
                this.strRecipient = Encoding.Unicode.GetString(data, 16 + senderLen * 2, recipientLen * 2);
            else
                this.strRecipient = null;

            //This checks for a null message field
            if (msgLen > 0)
                this.strMessage = Encoding.Unicode.GetString(data, 16 + senderLen * 2 + recipientLen * 2, msgLen * 2);
            else
                this.strMessage = null;
        }

        //Converts the Data structure into an array of bytes
        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();

            //First four are for the Command
            result.AddRange(BitConverter.GetBytes((int)cmdCommand));

            //Add the length of the Sender
            if (strSender != null)
                result.AddRange(BitConverter.GetBytes(strSender.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Add the length of the Recipient
            if (strRecipient != null)
                result.AddRange(BitConverter.GetBytes(strRecipient.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Add the length of the message
            if (strMessage != null)
                result.AddRange(BitConverter.GetBytes(strMessage.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Add the Sender
            if (strSender != null)
                result.AddRange(Encoding.Unicode.GetBytes(strSender));

            //Add recipient
            if (strRecipient != null)
                result.AddRange(Encoding.Unicode.GetBytes(strRecipient));

            // add the message text to our array of bytes
            if (strMessage != null)
                result.AddRange(Encoding.Unicode.GetBytes(strMessage));

            return result.ToArray();
        }

        public string strSender;        //Name by which the client logs into the room
        public string strMessage;       //Message text
        public string strRecipient;     //Recipient
        public Command cmdCommand;      //Command type (login, logout, send message, etcetera)
    }
}
