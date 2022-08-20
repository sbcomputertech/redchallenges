namespace ChallengeMod
{
    public enum ChallengeType
    {
        ENEMY_ON_JUMP,
        JUMP_PLUS_50_PERCENT,
        BIG_PARTICLE_BLADES,
        WEAPON_SWITCHING,
        RANDOM
    }
    public static class CHHelper
    {
        public static string ToString(ChallengeType challengeType)
        {
            switch(challengeType)
            {
                case ChallengeType.BIG_PARTICLE_BLADES:
                    return "All particle blades are bigger";
                case ChallengeType.ENEMY_ON_JUMP:
                    return "A new enemy spawns whenever you jump";
                case ChallengeType.JUMP_PLUS_50_PERCENT:
                    return "Your jump force increases by 50% every time you jump";
                case ChallengeType.WEAPON_SWITCHING:
                    return "You throw away your weapon every 10 seconds";
                default: return "A random challenge";
            }
        }
        public static ChallengeType GetRandom()
        {
            Random r = new();
            switch(r.Next(Enum.GetValues(typeof(ChallengeType)).Length - 1))
            {
                case 0:
                    return ChallengeType.ENEMY_ON_JUMP;
                case 1:
                    return ChallengeType.JUMP_PLUS_50_PERCENT;
                case 2:
                    return ChallengeType.BIG_PARTICLE_BLADES;
                case 3:
                    return ChallengeType.WEAPON_SWITCHING;
                default: return ChallengeType.ENEMY_ON_JUMP;
            }
        }
    }
}

