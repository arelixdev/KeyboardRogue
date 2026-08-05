using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKeyboardMover : MonoBehaviour
{
    [SerializeField] private KeyboardGenerator keyboard;
    [SerializeField] private float moveSpeed = 25f;
    [SerializeField] private float heightOffset = 0.9f;
    [SerializeField] private float frozenPassThroughRadius = 0.5f;

    private Health health;
    private Vector3 targetPosition;
    private bool arrivedProcessed = true;
    private bool isStuck;
    private KeyView stuckKey;
    private readonly HashSet<KeyView> frozenKeysHitThisDash = new HashSet<KeyView>();
    // Une touche de soin (Phase 4) ne soigne qu'une fois par combat, pas a chaque passage.
    private readonly HashSet<KeyView> healedKeysThisCombat = new HashSet<KeyView>();

    public KeyView CurrentKey { get; private set; }
    // Vrai tant que le dash n'est pas physiquement arrive a destination (anime via Update()).
    public bool IsMoving => !arrivedProcessed;

    private void Start()
    {
        if (keyboard == null)
            keyboard = GameManager.Instance.Keyboard;

        health = GetComponent<Health>();
        moveSpeed += LevelSession.BonusDashSpeed;

        PlaceOnRandomKey();
    }

    private void OnEnable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += HandleTextInput;
    }

    private void OnDisable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= HandleTextInput;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        CheckFrozenPassThrough();

        if (!arrivedProcessed && transform.position == targetPosition)
            OnArrived();
    }

    private void HandleTextInput(char character)
    {
        char upper = char.ToUpperInvariant(character);
        if (!keyboard.Keys.TryGetValue(upper, out KeyView key))
            return;

        if (isStuck)
        {
            // Englue: seule la touche gluante elle-meme repond, il faut la spammer pour se degager.
            if (key == stuckKey && stuckKey.RegisterStickyHit())
            {
                isStuck = false;
                stuckKey = null;
            }
            return;
        }

        if (key.Modifier == KeyModifierType.Disabled || key.Modifier == KeyModifierType.Broken)
            return;

        MoveTo(key);
    }

    private void PlaceOnRandomKey()
    {
        KeyView[] allKeys = keyboard.Keys.Values.ToArray();
        MoveTo(allKeys[Random.Range(0, allKeys.Length)]);
        transform.position = targetPosition;
    }

    private void MoveTo(KeyView key)
    {
        CurrentKey = key;
        targetPosition = GetStandPosition(key);
        arrivedProcessed = false;
        frozenKeysHitThisDash.Clear();
    }

    private void OnArrived()
    {
        arrivedProcessed = true;

        if (CurrentKey.Modifier == KeyModifierType.Frozen)
            health.TakeDamage(1);
        else if (CurrentKey.Modifier == KeyModifierType.Sticky)
        {
            isStuck = true;
            stuckKey = CurrentKey;
        }
        else if (CurrentKey.Modifier == KeyModifierType.Heal && !healedKeysThisCombat.Contains(CurrentKey))
        {
            // La charge unique n'est consommee que si le soin sert reellement: rester a PV max
            // en passant dessus (ex: au spawn) ne doit pas gaspiller la charge pour rien.
            if (health.Current < health.Max)
            {
                health.Heal(1);
                healedKeysThisCombat.Add(CurrentKey);
            }
        }
    }

    // Une case gelee situee entre le point de depart et la destination du dash inflige aussi
    // des degats au simple passage, pas seulement si le joueur s'y arrete.
    private void CheckFrozenPassThrough()
    {
        foreach (KeyView key in keyboard.Keys.Values)
        {
            if (key.Modifier != KeyModifierType.Frozen || key == CurrentKey || frozenKeysHitThisDash.Contains(key))
                continue;

            if (Vector3.Distance(transform.position, GetStandPosition(key)) <= frozenPassThroughRadius)
            {
                health.TakeDamage(1);
                frozenKeysHitThisDash.Add(key);
            }
        }
    }

    private Vector3 GetStandPosition(KeyView key)
    {
        return key.transform.position + Vector3.up * heightOffset;
    }
}
