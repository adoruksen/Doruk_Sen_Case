namespace RubyCase.BoxSystem
{
    public interface IBoxJourneyService
    {
        bool CanStartJourney(BoxController box);
        void StartJourney(BoxController box);
    }
}