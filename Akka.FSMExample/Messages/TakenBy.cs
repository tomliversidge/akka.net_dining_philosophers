using Akka.Actor;

namespace Akka.FSMExample.Messages
{
    public class TakenBy
    {
        public IActorRef Philosopher { get; private set; }

        public TakenBy(IActorRef philosopher)
        {
            Philosopher = philosopher;
        }
    }
}