using EloBuddy.SDK;
using System;
using EloBuddy;
using EloBuddy.SDK.Events;

namespace AJS
{
    class Program
    {
        static void Main(string[] args)
        {
             Loading.OnLoadingComplete += Game_OnLoad;
        }

        private static void Game_OnLoad(EventArgs args)
        {
            var ChampionName = ObjectManager.Player.BaseSkinName;

            switch (ChampionName)
            {
                case "Example":
                    //new Example();
                    break;

                default:
                    Chat.Print("[AJS]This Champion is not supported. Running AJS Utility.");
                    Utility.Wardsystem.WardTracker.AttachToMenu();
                    Utility.Wardsystem.WardTracker.WardTrackers();          
                    break;
            }
        }
    }
}
