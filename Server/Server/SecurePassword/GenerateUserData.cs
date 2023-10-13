using System;
using System.Collections.Generic;
using System.IO;

namespace Server
{
    class GenerateUserData
    {
        

        static public void generateUsers()
        {
            List<UserData> users = new List<UserData>();
            users.Add(new UserData("John", "passw1"));
            users.Add(new UserData("Peter", "passw2"));
            users.Add(new UserData("Ann", "passw3"));

            string[] lines = new string[users.Count];

            foreach (var user in users)
            {
                user.Salt = EncryptPassword.GenerateSalt(10);
                user.EncryptedPassw = EncryptPassword.GenerateSaltedHash(user.Password, user.Salt);
            }

            for (int i = 0; i < users.Count; i++)
            {
                UserData tmp = users[i];
                lines[i] = $"{tmp.Username};{tmp.EncryptedPassw};{tmp.Salt}";
            }

            File.WriteAllLines("userdata.csv", lines);
            Console.WriteLine("---userdata.csv is created---");
        }

        class UserData
        {
            public UserData(string username, string password)
            {
                Username = username;
                Password = password;
                EncryptedPassw = null;
                Salt = null;
            }

            public string Username { get; set; }
            public string Password { get; set; }
            public string EncryptedPassw { get; set; }
            public string Salt { get; set; }
        }

}

    

}
