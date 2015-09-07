using System;
using Akka.Actor;

namespace Akka.FSMExample
{
    public class ConsoleWriter : ReceiveActor
    {
        public ConsoleWriter()
        {
            Receive<FSMBase.CurrentState<PhilosopherState>>(@event =>
            {
                Console.WriteLine("{0} current state is {1}", @event.FsmRef.Path.Name, @event.State);
            });

            Receive<FSMBase.Transition<PhilosopherState>>(@event =>
            {
                Console.WriteLine("{0} is {1}", @event.FsmRef.Path.Name, @event.To);
            });

            Receive<FSMBase.Transition<ChopstickState>>(@event =>
            {
                Console.WriteLine("{0} transitioning from {1} to {2}", @event.FsmRef.Path.Name, @event.From, @event.To);
            });
        }
    }
}