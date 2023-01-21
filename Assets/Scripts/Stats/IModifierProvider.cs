using System.Collections.Generic;

namespace Stats
{
    public interface IModifierProvider
    {
        IEnumerable<float> GetAdditiveModifiers(CharacterStat stat);

        IEnumerable<float> GetPercentageModifiers(CharacterStat stat);
    }
}