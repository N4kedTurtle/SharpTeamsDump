# SharpTeamsDump
This is a .Net implementation of the research published at https://infinitelogins.com/2021/06/06/your-microsoft-teams-chats-arent-as-private-as-you-think/

Really just a re-implementation of the work done here: https://github.com/Xenov-X/PoSH_Teams_Message_Theif with a couple added features

*Must be elevated to use

USAGE:

SharpTeamsDump.exe - This will parse and dump every users' teams conversations currently stored

SharpTeamsDump.exe Searchword1 searchword2 etc - This will parse and dummp every users' conversations and only print the message that contain the listed words (case insensitive, NOT regex because I'm lazy)
