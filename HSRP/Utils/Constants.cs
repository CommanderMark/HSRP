namespace HSRP
{
    public static class Constants
    {
        public const string BotPrefix = "==>";
        public const int DiscordCharLimit = 1950;

        public const int StartingSkillPoints = 24;
        public const int SkillPointsPerLevel = 6;

        public static readonly int[] XPMilestones = { 0, 20, 50, 90, 150, 250, 400, 620, 950, 1450,
            2200, 3300, 5000, 7600, 11500, 17000, 25500, 38500, 58000, 87000, 130000, 200000,
            300000, 450000, 675000, 1000000 };

        // Roles/User.
        public const ulong OWNER = 103547713932529664;
        public const ulong GM_ROLE = 325511738852376576;

        // Guilds
        public const ulong RP_GUILD = 325511181634895872;

        // Channel IDs
        public const ulong RPOOC_CHANNEL = 325511222000615424;
        public const ulong GEN_CHANNEL = 325511181634895872;

        // There are 2 channels for strife: one for testing and one for RPing. This is toggled at runtime.
        public static ulong STRIFE_CHANNEL = RP_STRIFE_CHANNEL;

        public const ulong TEST_STRIFE_CHANNEL = 343972292730028042;
        public const ulong RP_STRIFE_CHANNEL = 351522810008436736;

        // Ailments.
        public const string MIND_CONTROL_AIL = "MindControl";
        public const string PHYSICAL_COUNTER_AIL = "CounterPhy";
    }

    public static class Dirs
    {
        public const string Config = "config";
        public const string Players = "players";
        public const string NPCs = "npcs";
        public const string Strifes = "strifes";
        public const string StrifeLogs = "STRIFE_LOG_";
    }
}
