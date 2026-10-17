namespace GladiusAI
{
    public enum CombatResult { Victory, Defeat, Surrender }

    public readonly struct GladiatorDiedEvent
    {
        public readonly GladiatorNPC gladiator;

        public GladiatorDiedEvent(GladiatorNPC gladiator)
        {
            this.gladiator = gladiator;
        }
    }

    public readonly struct GladiatorSurrenderedEvent
    {
        public readonly GladiatorNPC gladiator;

        public GladiatorSurrenderedEvent(GladiatorNPC gladiator)
        {
            this.gladiator = gladiator;
        }
    }

    public readonly struct CombatEndedEvent
    {
        public readonly CombatResult result;

        public CombatEndedEvent(CombatResult result)
        {
            this.result = result;
        }
    }

    public readonly struct CombatStartedEvent
    {
    }
}
