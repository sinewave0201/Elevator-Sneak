using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Conversation")]
public class DialogueConversation : ScriptableObject
{
    [SerializeField] private string startNodeId = "start";
    [SerializeField] private List<DialogueNode> nodes = new List<DialogueNode>();

    public string StartNodeId => startNodeId;
    public IReadOnlyList<DialogueNode> Nodes => nodes;

    public DialogueNode GetStartNode()
    {
        DialogueNode startNode = FindNode(startNodeId);
        return startNode ?? (nodes.Count > 0 ? nodes[0] : null);
    }

    public DialogueNode FindNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return null;

        return nodes.Find(node => node != null && node.Id == nodeId);
    }

    public bool Validate(out string error)
    {
        if (nodes == null || nodes.Count == 0)
        {
            error = "The conversation does not contain any dialogue nodes.";
            return false;
        }

        HashSet<string> nodeIds = new HashSet<string>();

        foreach (DialogueNode node in nodes)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.Id))
            {
                error = "Every dialogue node must have an ID.";
                return false;
            }

            if (!nodeIds.Add(node.Id))
            {
                error = $"Dialogue node ID '{node.Id}' is used more than once.";
                return false;
            }
        }

        if (FindNode(startNodeId) == null)
        {
            error = $"Start node '{startNodeId}' does not exist.";
            return false;
        }

        foreach (DialogueNode node in nodes)
        {
            foreach (DialogueOption option in node.Options)
            {
                if (!option.EndsConversation && FindNode(option.NextNodeId) == null)
                {
                    error = $"Option '{option.Text}' points to missing node '{option.NextNodeId}'.";
                    return false;
                }
            }
        }

        error = null;
        return true;
    }

    public static DialogueConversation CreateLegacy(
        string speakerName,
        string openingLine,
        string optionOne,
        string responseToOptionOne,
        string optionTwo,
        string responseToOptionTwo)
    {
        DialogueConversation conversation = CreateInstance<DialogueConversation>();
        conversation.hideFlags = HideFlags.HideAndDontSave;
        conversation.startNodeId = "start";

        List<DialogueOption> openingOptions = new List<DialogueOption>();

        if (!string.IsNullOrWhiteSpace(optionOne))
            openingOptions.Add(new DialogueOption(optionOne, "response_one", false));

        if (!string.IsNullOrWhiteSpace(optionTwo))
            openingOptions.Add(new DialogueOption(optionTwo, "response_two", false));

        conversation.nodes = new List<DialogueNode>
        {
            new DialogueNode("start", speakerName, new[] { openingLine }, openingOptions)
        };

        if (!string.IsNullOrWhiteSpace(optionOne))
        {
            conversation.nodes.Add(new DialogueNode(
                "response_one",
                speakerName,
                new[] { responseToOptionOne },
                Array.Empty<DialogueOption>()));
        }

        if (!string.IsNullOrWhiteSpace(optionTwo))
        {
            conversation.nodes.Add(new DialogueNode(
                "response_two",
                speakerName,
                new[] { responseToOptionTwo },
                Array.Empty<DialogueOption>()));
        }

        return conversation;
    }
}

[Serializable]
public class DialogueNode
{
    [SerializeField] private string id = "node";
    [SerializeField] private string speakerName = "Stranger";
    [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    [SerializeField] private List<DialogueOption> options = new List<DialogueOption>();

    public string Id => id;
    public string SpeakerName => speakerName;
    public IReadOnlyList<DialogueLine> DialogueLines => dialogueLines;
    public IReadOnlyList<DialogueOption> Options => options;

    public DialogueNode(
        string id,
        string speakerName,
        IEnumerable<string> dialogueLines,
        IEnumerable<DialogueOption> options)
    {
        this.id = id;
        this.speakerName = speakerName;
        this.dialogueLines = new List<DialogueLine>();

        foreach (string line in dialogueLines)
            this.dialogueLines.Add(new DialogueLine(line));

        this.options = new List<DialogueOption>(options);
    }
}

[Serializable]
public class DialogueLine
{
    [SerializeField, TextArea(2, 5)] private string text;

    public string Text => text;

    public DialogueLine(string text)
    {
        this.text = text;
    }
}

[Serializable]
public class DialogueOption
{
    [SerializeField] private string text = "Continue";
    [SerializeField] private string nextNodeId;
    [SerializeField] private bool endsConversation;

    public string Text => text;
    public string NextNodeId => nextNodeId;
    public bool EndsConversation => endsConversation;

    public DialogueOption(string text, string nextNodeId, bool endsConversation)
    {
        this.text = text;
        this.nextNodeId = nextNodeId;
        this.endsConversation = endsConversation;
    }
}
