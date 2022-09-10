namespace ChallengeMod
{
    public enum ChallengeType
    {
        ENEMY_ON_JUMP,
        JUMP_PLUS_50_PERCENT,
        BIG_PARTICLE_BLADES,
        WEAPON_SWITCHING,
        BULLET_HELL,
        INVERTED_CTRL,
        RANDOM,
        NONE
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
                case ChallengeType.BULLET_HELL:
                    return "BULLET HELL MODE!";
                case ChallengeType.INVERTED_CTRL:
                    return "Controls are inverted";
                case ChallengeType.RANDOM:
                    return "A random challenge";
                default: return "No challenge";
            }
        }
        public static ChallengeType GetRandom()
        {
            Random r = new();
            var values = Enum.GetValues(typeof(ChallengeType));
            int random = r.Next(values.Length);
            return (ChallengeType) values.GetValue(random);
        }
    }
}

