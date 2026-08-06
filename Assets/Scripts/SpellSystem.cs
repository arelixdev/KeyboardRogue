using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Attribue a chaque sort une sequence de touches distinctes tiree au hasard, une fois par run.
// Les sorts puisent dans un seul pool melange, consomme sans remise: aucune touche n'est jamais
// partagee entre deux sorts differents, ce qui evite toute ambiguite de saisie (deux sorts ne
// peuvent jamais etre "en cours" en meme temps).
public static class SpellSystem
{
    public static Dictionary<SpellDefinition, char[]> AssignSequences(DungeonFloorConfig[] floors, KeyboardLayoutType layout)
    {
        var result = new Dictionary<SpellDefinition, char[]>();
        if (floors == null)
            return result;

        List<char> pool = KeyboardGenerator.GetAllCharacters(layout).ToList();
        Shuffle(pool);

        IEnumerable<SpellDefinition> allSpells = floors
            .Where(f => f != null && f.possibleSpells != null)
            .SelectMany(f => f.possibleSpells)
            .Distinct();

        int cursor = 0;
        foreach (SpellDefinition spell in allSpells)
        {
            if (result.ContainsKey(spell))
                continue;

            int length = Mathf.Max(1, spell.sequenceLength);
            if (cursor + length > pool.Count)
            {
                Debug.LogWarning($"[SpellSystem] Plus assez de touches disponibles pour \"{spell.spellName}\" ({length} requises, {pool.Count - cursor} restantes): sort ignore pour cette run.");
                continue;
            }

            result[spell] = pool.GetRange(cursor, length).ToArray();
            cursor += length;
        }

        return result;
    }

    private static void Shuffle(List<char> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
