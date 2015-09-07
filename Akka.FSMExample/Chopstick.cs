using System;
using System.Threading;
using Akka.Actor;
using Akka.FSMExample.Messages;

public enum ChopstickState
{
    Available,
    Taken
}

/// <summary>
/// A chopstick is an actor, it can be taken, and put back
/// </summary>
public class Chopstick : FSM<ChopstickState, TakenBy>
{
    public Chopstick(ActorSystem actorSystem)
    {
        // A chopstick begins its existence as available and taken by no one
        StartWith(ChopstickState.Available, new TakenBy(actorSystem.DeadLetters));
        // When a chopstick is available, it can be taken by a some hakker
        When(ChopstickState.Available, @event =>
        {
            if (@event.FsmEvent is Take) 
                return GoTo(ChopstickState.Taken).Using(new TakenBy(Sender)).Replying(new Taken(Self));
            if (@event.FsmEvent is Put)
                throw new Exception("waht?");
            return null;
        });
       
        // When a chopstick is taken by a hakker it will refuse to be taken by other hakkers
        // But the owning hakker can put it back
        When(ChopstickState.Taken, @event =>
        {
            if (@event.FsmEvent is Take)
            {
                return Stay().Replying(new Busy(Self));
            }
            if (@event.FsmEvent is Put)
            {
                if (@event.StateData.Hakker.Equals(Sender))
                    return GoTo(ChopstickState.Available).Using(new TakenBy(actorSystem.DeadLetters));
            }
            return null;
        });
        Initialize();
    }
}