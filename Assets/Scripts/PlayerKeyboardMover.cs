using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKeyboardMover : MonoBehaviour
{
    [SerializeField] private KeyboardGenerator keyboard;
    [SerializeField] private float moveSpeed = 25f;
    [SerializeField] private float heightOffset = 0.9f;

    private Vector3 targetPosition;

    public KeyView CurrentKey { get; private set; }

    private void Start()
    {
        if (keyboard == null)
            keyboard = GameManager.Instance.Keyboard;

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
    }

    private void HandleTextInput(char character)
    {
        char upper = char.ToUpperInvariant(character);
        if (keyboard.Keys.TryGetValue(upper, out KeyView key))
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
    }

    private Vector3 GetStandPosition(KeyView key)
    {
        return key.transform.position + Vector3.up * heightOffset;
    }
}
