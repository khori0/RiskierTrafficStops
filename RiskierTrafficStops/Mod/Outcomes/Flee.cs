﻿namespace RiskierTrafficStops.Mod.Outcomes;

internal class Flee : Outcome
{
    private enum FleeOutcomes
    {
        Flee,
        BurnOut,
        LeaveVehicle,
    }

    private static readonly FleeOutcomes[] AllFleeOutcomes = (FleeOutcomes[])Enum.GetValues(typeof(FleeOutcomes));
    
    public Flee(LHandle handle) : base(handle)
    {
        try
        {
            if (MeetsRequirements(TrafficStopLHandle))
            {
                GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(StartOutcome));
            }
        }
        catch (Exception e)
        {
            if (e is ThreadAbortException) return;
            Error(e, nameof(StartOutcome));
            CleanupOutcome(true);
        }
    }

    internal override void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);
            
        Normal("Getting all vehicle occupants");
        var pedsInVehicle = SuspectVehicle.Occupants.ToList();

        foreach (var ped in pedsInVehicle)
        {
            if (ped.IsAvailable() && PedsToIgnore.Contains(ped))
            {
                pedsInVehicle.Remove(ped);
            }
        }
        
        FleeOutcomes chosenFleeOutcome = AllFleeOutcomes.PickRandom();

        switch (chosenFleeOutcome)
        {
            case FleeOutcomes.Flee:
                Normal("Starting pursuit");

                if (Functions.GetCurrentPullover() == null)
                {
                    CleanupOutcome(false);
                    return;
                }

                PursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                break;
            case FleeOutcomes.BurnOut:
                Normal("Making suspect do burnout");
                Suspect.Tasks.PerformDrivingManeuver(SuspectVehicle, VehicleManeuver.BurnOut, 2000)
                    .WaitForCompletion(2000);
                Normal("Clearing suspect tasks");
                Suspect.Tasks.PerformDrivingManeuver(SuspectVehicle, VehicleManeuver.GoForwardStraight, 750)
                    .WaitForCompletion(750);
                Normal("Starting pursuit");

                if (Functions.GetCurrentPullover() == null)
                {
                    CleanupOutcome(false);
                    return;
                }

                PursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                break;
            case FleeOutcomes.LeaveVehicle:
                foreach (var i in pedsInVehicle)
                {
                    if (i.IsAvailable())
                    {
                        i.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                    }
                }

                if (Functions.GetCurrentPullover() == null) return;

                PursuitLHandle = SetupPursuitWithList(true, pedsInVehicle);
                break;
        }
            
        GameFiberHandling.CleanupFibers();
        InvokeEvent(RTSEventType.End);
    }
}