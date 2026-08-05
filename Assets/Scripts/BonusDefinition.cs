using UnityEngine;

// Effet applique immediatement au joueur (stat globale), pas lie a une touche precise.
public enum GlobalEffectType
{
    MaxHpUp,
    DashSpeedUp,
    ContactDamageUp,
    // Ajoute des secondes a l'intervalle d'apparition des ennemis: positif = ennemis plus rares
    // (bonus), negatif = ennemis plus frequents (malus).
    EnemySpawnIntervalDelta,
}

// Effet permanent applique a une touche precise choisie par le joueur, actif pour tous les
// combats restants de la run.
public enum PermanentKeyEffectType
{
    Heal,
    Broken,
}

public enum BonusScope
{
    Global,
    KeyBound,
}

// Bonus ou malus tire aleatoirement a la fin d'un combat. Scope Global: applique immediatement.
// Scope KeyBound: le joueur choisit sur quelle touche du clavier l'effet se pose.
[CreateAssetMenu(fileName = "BonusDefinition", menuName = "KeyboardRogue/Bonus Or Malus")]
public class BonusDefinition : ScriptableObject
{
    public string bonusName = "Bonus";
    [TextArea] public string description = "Description de l'effet.";

    // Poids du tirage aleatoire parmi la liste de l'etage (pas une probabilite absolue).
    [Range(0.1f, 10f)] public float weight = 1f;

    public BonusScope scope = BonusScope.Global;

    [Header("Si Global")]
    public GlobalEffectType globalEffectType;
    public float globalEffectValue = 1f;

    [Header("Si KeyBound")]
    public PermanentKeyEffectType keyEffectType;
}
