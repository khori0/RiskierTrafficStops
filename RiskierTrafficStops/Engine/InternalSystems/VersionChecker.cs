﻿using System;
using System.Net;
using System.Reflection;
using System.Threading;
using Rage;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;

namespace RiskierTrafficStops.Engine.InternalSystems
{
    
    /*
     * CREDIT TO SuperPyroManiac for the orignal code
     * Modifications made by myself
     * https://github.com/SuperPyroManiac/SuperPlugins/blob/master/SuperCallouts/SimpleFunctions/VersionChecker.cs
     */
    
    internal static class VersionChecker
    {
        private enum State
        {
            Failed,
            Update,
            Current
        }

        private static State _state = State.Current;
        private static string _receivedData = string.Empty;
        internal static Thread UpdateThread = new(CheckRTSVersion);

        internal static string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        internal static string OnlineVersion;

        internal static void IsUpdateAvailable()
        {
            try
            {
                UpdateThread.Start();
                GameFiber.Sleep(1000);

                while (UpdateThread.IsAlive) GameFiber.Wait(1000);

                switch (_state)
                {
                    case State.Failed:
                        Normal("Update check failed... please check your internet connection");
                        break;
                    case State.Update:
                        Game.DisplayNotification("3dtextures",
                            "mpgroundlogo_cops",
                            "Riskier Traffic Stops",
                            "~b~By Astro",
                            $"Plugin is ~r~out of to date~s~!\n" +
                            $"Online Version: ~g~{_receivedData}~s~\n" +
                            $"Installed version: ~y~{CurrentVersion}~s~\n" +
                            $"Please update ~r~ASAP~s~!");
                        Logger.Normal($"Online Version: {_receivedData} | Installed Version: {CurrentVersion}");
                        Logger.Normal("Plugin is outdated, please up date to the latest version as soon as possible");
                        break;
                    case State.Current:
                        Logger.Normal($"Online Version: {_receivedData} | Installed Version: {CurrentVersion}");
                        Game.DisplayNotification("3dtextures",
                            "mpgroundlogo_cops",
                            "Riskier Traffic Stops",
                            "~b~By Astro",
                            "Plugin is ~g~up to date!");
                        break;
                }
            }
            catch (WebException)
            {
                _state = State.Failed;
            }
        }

        private static void CheckRTSVersion()
        {
            try
            {
                _receivedData = new WebClient()
                    .DownloadString(
                        "https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=44036&textOnly=1")
                    .Trim();
            }
            catch (WebException)
            {
                _state = State.Failed;
            }

            if (_receivedData == CurrentVersion) return;
            _state = State.Update;
        }
    }
}