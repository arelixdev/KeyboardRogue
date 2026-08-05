using UnityEngine;

// Applique les effets d'un BonusDefinition a l'etat de run persistant (LevelSession).
public static class BonusEffects
{
    public static void ApplyGlobal(BonusDefinition def)
    {
        switch (def.globalEffectType)
        {
            case GlobalEffectType.MaxHpUp:
                int add = Mathf.RoundToInt(def.globalEffectValue);
                LevelSession.PlayerMaxHp += add;
                LevelSession.PlayerHp += add;

                // Le joueur (et son HUD) sont encore dans la scene de combat en cours: sans ca,
                // le changement ne serait applique qu'au chargement de la prochaine scene
                // (PlayerRunSync.Start), donc invisible tant qu'on n'a pas quitte cet ecran.
                Health liveHealth = GameManager.Instance != null && GameManager.Instance.Player != null
                    ? GameManager.Instance.Player.GetComponent<Health>()
                    : null;
                if (liveHealth != null)
                {
                    liveHealth.SetMax(LevelSession.PlayerMaxHp);
                    liveHealth.SetCurrent(LevelSession.PlayerHp);
                }
                break;
            case GlobalEffectType.DashSpeedUp:
                LevelSession.BonusDashSpeed += def.globalEffectValue;
                break;
            case GlobalEffectType.ContactDamageUp:
                LevelSession.BonusContactDamage += def.globalEffectValue;
                break;
            case GlobalEffectType.EnemySpawnIntervalDelta:
                LevelSession.BonusEnemySpawnInterval += def.globalEffectValue;
                break;
        }
    }

    public static void BindKeyEffect(char character, PermanentKeyEffectType effect)
    {
        LevelSession.PermanentKeyEffects[character] = effect;
    }

    public static KeyModifierType ToKeyModifier(this PermanentKeyEffectType effect)
    {
        return effect == PermanentKeyEffectType.Heal ? KeyModifierType.Heal : KeyModifierType.Broken;
    }
}
