using UnityEngine;

// Base commune a tous les boss: references standard (clavier/joueur) + Health, exactement comme
// les autres scripts de combat. Chaque boss concret (ex: FistBossController) hérite de ceci et
// implemente son propre pattern d'attaque.
public abstract class BossController : MonoBehaviour
{
    [SerializeField] protected KeyboardGenerator keyboard;
    [SerializeField] protected PlayerKeyboardMover player;

    public Health Health { get; private set; }

    protected virtual void Awake()
    {
        if (keyboard == null)
            keyboard = GameManager.Instance.Keyboard;
        if (player == null)
            player = GameManager.Instance.Player;

        Health = GetComponent<Health>();
    }
}
