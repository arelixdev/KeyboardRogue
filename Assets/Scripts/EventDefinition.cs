using UnityEngine;

// Evenement de carte (noeud Event, hors Repos): un SO = un effet fixe. La carte tire un
// EventDefinition au hasard dans la liste de l'etage courant (DungeonFloorConfig.possibleEvents).
[CreateAssetMenu(fileName = "EventDefinition", menuName = "KeyboardRogue/Map Event")]
public class EventDefinition : ScriptableObject
{
    public string eventName = "Evenement";
    [TextArea] public string message = "Il ne se passe rien de particulier ici.";

    // 0 = pas de soin.
    public int healAmount = 0;
}
