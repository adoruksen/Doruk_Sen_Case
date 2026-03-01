using RubyCase.Gameplay.BoxSystem;

namespace RubyCase.Core.Session
{
    public interface IBoxJourneyService
    {
        bool CanStartJourney(BoxController box);
        void StartJourney(BoxController box);
    }
}