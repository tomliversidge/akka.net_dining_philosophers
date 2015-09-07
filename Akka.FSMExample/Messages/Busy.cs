using Akka.Actor;

namespace Akka.FSMExample.Messages
{
    public class Busy
    {
        public IActorRef Chopstick { get; private set; }

        public Busy(IActorRef chopstick)
        {
            Chopstick = chopstick;
        }
    }
}