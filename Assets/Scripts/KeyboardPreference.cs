using UnityEngine;

// Layout physique du joueur, choisi une fois et sauvegarde: c'est son clavier a lui, pas la carte qui decide.
public static class KeyboardPreference
{
    private const string PrefKey = "KeyboardLayout";

    public static bool HasChosen => PlayerPrefs.HasKey(PrefKey);

    public static KeyboardLayoutType Layout
    {
        get => (KeyboardLayoutType)PlayerPrefs.GetInt(PrefKey, (int)KeyboardLayoutType.AZERTY);
        set
        {
            PlayerPrefs.SetInt(PrefKey, (int)value);
            PlayerPrefs.Save();
        }
    }
}
