﻿using System;
using LSPD_First_Response.Mod.API;
using Rage;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;
using RiskierTrafficStops.Engine.FrontendSystems;
using RiskierTrafficStops.Engine.Helpers;
using RiskierTrafficStops.Engine.InternalSystems;

namespace RiskierTrafficStops
{
    public class Main : Plugin
    {
        internal static bool OnDuty;

        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
        }

        private static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            OnDuty = onDuty;
            if (onDuty)
            {
                GameFiber.StartNew(() =>
                {
                    if (!Helper.VerifyDependencies()) return;

                    // Setting up INI And checking for updates
                    Debug("Setting up INIFile...");
                    GameFiber.StartNew(Settings.IniFileSetup);
                    Debug("Creating menu...");
                    GameFiber.StartNew(ConfigMenu.CreateMenu);
                    Debug("Adding console commands...");
                    Game.AddConsoleCommands();
                    Debug("Checking for updates...");
                    GameFiber.StartNew(VersionChecker.IsUpdateAvailable);
                    // Displaying startup Notification
                    Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro",
                        "Watch your back out there officer!");
                    Debug("Checking Auto Log status...");
                    switch (Settings.AutoLogEnabled)
                    {
                        //Displaying Auto-log Notification
                        case true:
                            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops",
                                "~b~Auto Logging Status", "Auto Logging is ~g~Enabled~s~");
                            break;
                        case false:
                            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops",
                                "~b~Auto Logging Status", "Auto Logging is ~r~Disabled~s~");
                            break;
                    }

                    Debug("Auto log status: " + Settings.AutoLogEnabled);
                    //Subscribes to events
                    PulloverEventHandler.SubscribeToEvents();

                    AppDomain.CurrentDomain.DomainUnload += Cleanup;
                    Debug("Loaded successfully");
                });
            }
        }

    private static void Cleanup(object sender, EventArgs e)
        {
            try
            {
                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Riskier Traffic Stops", "~b~By Astro", "Did you crash or are you a dev?");
                //Unsubscribes from events
                PulloverEventHandler.UnsubscribeToEvents();
                if (VersionChecker.UpdateThread.IsAlive)
                {
                    VersionChecker.UpdateThread.Abort();
                }
                Debug("Unloaded successfully");
            }
            catch (Exception ex)
            {
                if (e is System.Threading.ThreadAbortException) return;
                
                Error(ex, nameof(Cleanup));
            }
        }
    
            
        public override void Finally()
        {
        }
    }
}