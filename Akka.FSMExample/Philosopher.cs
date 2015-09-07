using System;
using Akka.Actor;
using Akka.FSMExample.Messages;
using Akka.Util.Internal;

namespace Akka.FSMExample
{
    /// <summary>
    /// State container to keep track of which chopsticks we have
    /// </summary>
    public class TakenChopsticks
    {
        public IActorRef Left { get; private set; }
        public IActorRef Right { get; private set; }

        public TakenChopsticks(IActorRef left, IActorRef right)
        {
            Left = left;
            Right = right;
        }
    }

    public enum PhilosopherState
    {
        Waiting,
        Thinking,
        Hungry,
        WaitingForOtherChopstick,
        FirstChopstickDenied,
        Eating
    }

    /// <summary>
    /// A philosopher either thinks about philosophy or has to eat ;-)
    /// </summary>
    public class Philosopher : FSM<PhilosopherState, TakenChopsticks>
    {
        private readonly string _name;
        private readonly IActorRef _left;
        private readonly IActorRef _right;

        public Philosopher(string name, IActorRef left, IActorRef right)
        {
            _name = name;
            _left = left;
            _right = right;
        
            StartWith(PhilosopherState.Waiting, new TakenChopsticks(null, null));
            
            When(PhilosopherState.Waiting, @event =>
            {
                if (@event.FsmEvent is Think)
                {
                    return StartThinking(TimeSpan.FromSeconds(1));
                }
                return null;
            });

            // When a philosopher is thinking they can become hungry
            // and try to pick up its chopsticks and eat
            When(PhilosopherState.Thinking, @event =>
            {
                if (@event.FsmEvent is StateTimeout)
                {
                    _left.Tell(new Take());
                    _right.Tell(new Take());
                    return GoTo(PhilosopherState.Hungry);
                }
                return null;
            });
            
            // When a philosopher is hungry it tries to pick up its chopsticks and eat
            // When it picks one up, it goes into wait for the other
            // If the philosophers first attempt at grabbing a chopstick fails,
            // it starts to wait for the response of the other grab
            When(PhilosopherState.Hungry, @event =>
            {
                if (@event.FsmEvent is Taken)
                {
                    var chopstick = (@event.FsmEvent as Taken).Chopstick;
                    if (chopstick.Equals(_left))
                        return GoTo(PhilosopherState.WaitingForOtherChopstick)
                            .Using(new TakenChopsticks(_left, null));

                    if (chopstick.Equals(_right))
                    {
                        return GoTo(PhilosopherState.WaitingForOtherChopstick)
                            .Using(new TakenChopsticks(null, _right));
                    } 
                }     
                if (@event.FsmEvent is Busy)
                {
                    return GoTo(PhilosopherState.FirstChopstickDenied);
                }
                return null;
            });
            
            // When a philosopher is waiting for the last chopstick it can either obtain it
            // and start eating, or the other chopstick was busy, and the philosopher goes
            // back to think about how he should obtain his chopsticks :-)
            When(PhilosopherState.WaitingForOtherChopstick, @event =>
            {
                State<PhilosopherState, TakenChopsticks> newState = null;
                @event.FsmEvent.Match()
                    .With<Taken>(taken =>
                        // we now have two chopsticks, time to eat!
                        newState = GoTo(PhilosopherState.Eating)
                            .Using(new TakenChopsticks(_left, _right))
                            .ForMax(TimeSpan.FromSeconds(3)))
                    .With<Busy>(busy =>
                        {
                            // we have one chopstick, but the other is in use, 
                            // so put back the one we have to free it up for someone else 
                            if (busy.Chopstick.Equals(_left))
                            {
                                _right.Tell(new Put());
                            }
                            if (busy.Chopstick.Equals(_right))
                            {
                                _left.Tell(new Put());
                            }

                            newState = StartThinking(TimeSpan.FromSeconds(1));
                        });
                return newState;
            });

            // When the results of the other grab comes back,
            // he needs to put it back if he got the other one.
            // Then go back and think and try to grab the chopsticks again
            When(PhilosopherState.FirstChopstickDenied, @event =>
            {
                State<PhilosopherState, TakenChopsticks> newState = null;
                @event.FsmEvent.Match()
                    .With<Taken>(taken => {
                        taken.Chopstick.Tell(new Put());
                        newState = StartThinking(TimeSpan.FromSeconds(1));
                        })
                    .With<Busy>(_ => {
                        newState = StartThinking(TimeSpan.FromSeconds(1)); }
                    )
                    .Default(_=> {});
                
                return newState;
            });

            // When a philosopher is eating, he can decide to start to think,
            // then he puts down his chopsticks and starts to think
            When(PhilosopherState.Eating, @event =>
            {
                State<PhilosopherState, TakenChopsticks> newState = null;
                @event.FsmEvent.Match().With<StateTimeout>(_ =>
                {
                    _left.Tell(new Put());
                    _right.Tell(new Put());
                    newState = StartThinking(TimeSpan.FromSeconds(5));
                });
                return newState;
            });
            Initialize();
        }
    
        private State<PhilosopherState, TakenChopsticks> StartThinking(TimeSpan duration)
        {
            return GoTo(PhilosopherState.Thinking).Using(new TakenChopsticks(null, null)).ForMax(duration);
        }
    }
}