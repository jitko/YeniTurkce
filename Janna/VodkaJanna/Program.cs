﻿using System;
using System.Drawing;
using EloBuddy;
using EloBuddy.SDK.Events;

namespace VodkaJanna
{
    public static class Program
    {
        public const string ChampName = "Janna";

        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        private static void OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != ChampName)
            {
                return;
            }
            Config.Initialize();
            SpellManager.Initialize();
            ModeManager.Initialize();
            Events.Initialize();

            WelcomeMsg();
        }

        private static void WelcomeMsg()
        {
            Chat.Print("Vodka{0} Yuklendi. Ceviri TekinTR!", Color.LightBlue, ChampName);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Vodka{0} Yuklendi. Ceviri TekinTR!", ChampName);
            Console.ResetColor();
        }
    }
}
