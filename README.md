
# Chat app with login and file sending

This was the final project for Telecommunication Systems course at the University of Debrecen in 2020/2021/2. semester. The main goal of the application was to create a server and a client application with WPF UI for a chat system using socket programming.


## Features

- Sender can send text messages or files
- Multithreaded to handle simultaneous requests and messages
- Login screen before entering the chatroom
- Passwords are stored with SHA256 and salt

**In more details**
- To log in, a password needs to be entered in addition to the username, these are stored in a .csv file on the server side. The password is handled with salt and hash, with attention to secure storage
- Clients can send private messages, for this the selection list is also updated (on a separate thread) taking into account new connections and left users
- The app is also capable of sending files, which can be broadcast or unicast. Received files are placed in the Documents/ChatProgram folder

**Minor Features**
- Pressing ENTER also fulfills the functions of the login and send button
- Closing of the client window is also handled, during which all other clients are informed of leaving the chat
- Scrollbar for long messages

To generate users and passwords the [GenerateUserData](Server/Server/SecurePassword/GenerateUserData.cs) class is used to simplify the project. By running the generateUsers() method, we can create a .csv file containing user data. The uploaded project contains sample user data in [userdata.csv](Server/Server/bin/Debug/userdata.csv) file.

## Run the app
To test the client run the [Client.exe](Client/Client/bin/Release/Client.exe) file, for the server run the [Server.exe](Server/Server/bin/Release/Server.exe) file. The default user details are the following:
| Username  | Password |
| --------- | -------- |
| John      | passw1   |
| Peter     | passw2   |
| Ann       | passw3   |

## Demo and screenshots

https://github.com/akosm-portfolio/chat-app/assets/147773177/37523ea2-3b0a-4fe5-8af8-6e9d47091381

*Screenshot 1*\
![screenshot1](https://am4pap001files.storage.live.com/y4mdgqN0K6qbw-LzRQHrEAOFhDHCW2ooThlWfiv0mYFKIw9vzK3_nktpL7HcpVDNMZcDumY0XbEza1JJNqpPSCrDhuJp8R6s9cajZTlUBKTHgP8qQcg2GwCaSXeWiT2vfxDHjYFrzB6RrCnOJ9AHwxzb0ewx-_wbMk_cK6vXhuh8FWIKIqlIl3XYUi-xr8otcAEJpKvSCDdjmx7IQdpzfZQ3Q?encodeFailures=1&width=930&height=500)

*Screenshot 2*\
![screenshot2](https://am4pap001files.storage.live.com/y4mHUiOOW16N0ms0iP_ugyvaQKPTjyLjzMG-FPPrkArJM1Pnw1m6hR99UKS650OZcGOZdwlplIwhpYWbcMg3KuDbPCOiKUeL9EbBbqdixqoCPYXAN8eH8kmk-z9fNfvQ-Ck7hmgc_9g4aDwUTCGCg_tFWLWtSAG_CcopQI9CSWlMEgfFB5rD1SRfsrKUdqUYrxvkUg9x4gFs5PUzFRILJzqcQ?encodeFailures=1&width=930&height=500)

*Screenshot 3*\
![screenshot3](https://am4pap001files.storage.live.com/y4mmDl_Vq68j6PdQA60mqkObbyCdIKLEDkP05j7hjqkM983Q_X1qH1KQ5kxkk58eE-CvjbslZ2WXCsT_OZw0TQbcOZ2f0HNMfPqS5HpgVOajl_Dj-pWPFGWTH_PcRYBAXIMCw93jPqVuNJ9EKymzQbmwfxYF9pZBkRfKcNOvX2utVC2BHb7B4uGDeB9sK_g1L-3M7E_BaHrI98AkvZVBIof5g?encodeFailures=1&width=930&height=500)


