using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.FSMExample.Messages;

namespace Akka.FSMExample
{
    class Program
    {
        private static ActorSystem _actorSystem;

        private static void Main(string[] args)
        {
            _actorSystem = ActorSystem.Create("TestSystem");
            
            // have a separate actor to write transition changes to the console
            var consoleWriter = _actorSystem.ActorOf(Props.Create(() => new ConsoleWriter()), "consoleWriter");
            
            var chopsticks = new List<IActorRef>();
            // Create 5 chopsticks
            for (int i = 0; i < 5; i++)
            {
                var chopstick = _actorSystem.ActorOf(Props.Create(() => new Chopstick(_actorSystem)), "chopstick" + i);
                chopsticks.Add(chopstick);
            }

            // Create 5 philosophers and assign them their left and right chopstick
            var philosophers = new List<IActorRef>();
            var names = new List<string>() {"Aristotle", "Plato", "Locke", "Socrates", "Marx"};
            for (int i = 0; i < names.Count; i++)
            {
                var leftChopstick = chopsticks[i];
                var rightChopstick = i == 4 ? chopsticks[0] : chopsticks[i + 1];
                var philosopher = _actorSystem.ActorOf(Props.Create(() => new Philosopher(names[i],leftChopstick,rightChopstick)), names[i]);
                philosophers.Add(philosopher);
                // subscribe the console writer to receive state change transitions
                philosopher.Tell(new FSMBase.SubscribeTransitionCallBack(consoleWriter));
            }

            foreach (var philosopher in philosophers)
            {
                philosopher.Tell(new Think());
            }
            Console.ReadLine();
        }
    }
}