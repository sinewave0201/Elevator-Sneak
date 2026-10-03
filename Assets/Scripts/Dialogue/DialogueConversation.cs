using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Conversation")]
public class DialogueConversation : ScriptableObject
{
    [SerializeField] private DialogueCharacterProfile defaultCharacter;
    [SerializeField] private string startNodeId = "start";
    [SerializeField] private List<DialogueNode> nodes = new();
    public DialogueCharacterProfile DefaultCharacter => defaultCharacter;
    public string StartNodeId => startNodeId;
    public IReadOnlyList<DialogueNode> Nodes => nodes;
    public DialogueNode GetStartNode() => FindNode(startNodeId) ?? (nodes.Count > 0 ? nodes[0] : null);
    public DialogueNode FindNode(string id) => string.IsNullOrWhiteSpace(id) ? null : nodes.Find(node => node != null && node.Id == id);
    public bool Validate(out string error)
    {
        if (nodes == null || nodes.Count == 0) { error = "The conversation does not contain any dialogue nodes."; return false; }
        HashSet<string> ids = new();
        foreach (DialogueNode current in nodes)
        {
            if (current == null || string.IsNullOrWhiteSpace(current.Id)) { error = "Every dialogue node must have an ID."; return false; }
            if (!ids.Add(current.Id)) { error = $"Dialogue node ID '{current.Id}' is used more than once."; return false; }
        }
        if (FindNode(startNodeId) == null) { error = $"Start node '{startNodeId}' does not exist."; return false; }
        foreach (DialogueNode current in nodes) foreach (DialogueOption option in current.Options)
            if (!option.EndsConversation && FindNode(option.NextNodeId) == null) { error = $"Option '{option.Text}' points to missing node '{option.NextNodeId}'."; return false; }
        error = null; return true;
    }
    public static DialogueConversation CreateLegacy(string speaker, string opening, string first, string firstResponse, string second, string secondResponse)
    {
        DialogueConversation result = CreateInstance<DialogueConversation>(); result.hideFlags = HideFlags.HideAndDontSave;
        List<DialogueOption> openingOptions = new();
        if (!string.IsNullOrWhiteSpace(first)) openingOptions.Add(new DialogueOption(first, "response_one", false));
        if (!string.IsNullOrWhiteSpace(second)) openingOptions.Add(new DialogueOption(second, "response_two", false));
        result.nodes = new List<DialogueNode> { new("start", speaker, new[] { opening }, openingOptions) };
        if (!string.IsNullOrWhiteSpace(first)) result.nodes.Add(new DialogueNode("response_one", speaker, new[] { firstResponse }, Array.Empty<DialogueOption>()));
        if (!string.IsNullOrWhiteSpace(second)) result.nodes.Add(new DialogueNode("response_two", speaker, new[] { secondResponse }, Array.Empty<DialogueOption>()));
        return result;
    }
}

[Serializable] public class DialogueNode
{
    [SerializeField] private string id = "node";
    [SerializeField] private string speakerName = "Stranger";
    [SerializeField] private DialogueCharacterProfile characterProfile;
    [SerializeField] private List<DialogueLine> dialogueLines = new();
    [SerializeField] private List<DialogueOption> options = new();
    public string Id => id; public string SpeakerName => speakerName; public DialogueCharacterProfile CharacterProfile => characterProfile;
    public IReadOnlyList<DialogueLine> DialogueLines => dialogueLines; public IReadOnlyList<DialogueOption> Options => options;
    public DialogueNode(string id, string speaker, IEnumerable<string> lines, IEnumerable<DialogueOption> options) { this.id = id; speakerName = speaker; dialogueLines = new(); foreach (string line in lines) dialogueLines.Add(new DialogueLine(line)); this.options = new(options); }
}
[Serializable] public class DialogueLine { [SerializeField, TextArea(2, 5)] private string text; public string Text => text; public DialogueLine(string text) => this.text = text; }
[Serializable] public class DialogueOption { [SerializeField] private string text = "Continue"; [SerializeField] private string nextNodeId; [SerializeField] private bool endsConversation; public string Text => text; public string NextNodeId => nextNodeId; public bool EndsConversation => endsConversation; public DialogueOption(string text, string next, bool ends) { this.text = text; nextNodeId = next; endsConversation = ends; } }
