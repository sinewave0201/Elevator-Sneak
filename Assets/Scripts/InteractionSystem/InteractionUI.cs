using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text promptText;

    public void Show(Interaction interaction) {
        promptText.text = "[E] " + interaction.InteractionPrompt;
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}