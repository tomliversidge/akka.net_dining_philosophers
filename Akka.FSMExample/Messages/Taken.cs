using Akka.Actor;

namespace Akka.FSMExample.Messages
{
    public class Taken
    {
        public IActorRef Chopstick { get; private set; }

        public Taken(IActorRef chopstick)
        {
            Chopstick = chopstick;
        }
    }
}