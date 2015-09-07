using Akka.Actor;

namespace Akka.FSMExample.Messages
{
    public class TakenBy
    {
        public IActorRef Hakker { get; private set; }

        public TakenBy(IActorRef hakker)
        {
            Hakker = hakker;
        }
    }
}