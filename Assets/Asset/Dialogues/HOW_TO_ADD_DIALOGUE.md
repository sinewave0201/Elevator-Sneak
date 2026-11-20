# How to Add Dialogue and Assign It to an NPC

(For quick testing, use **Tools > Dialogue > Create Main Demo**. This creates or selects a `DialogueTestObject`; assign your conversation to its **Dialogue Trigger** component to test it in the Main scene.)

This project stores reusable conversations in `DialogueConversation` assets. You can add dialogue lines, choices, and branches in the Unity Inspector without editing C# scripts.

## 1. Create a conversation asset

1. In the Unity **Project** window, open the folder where you want to store the conversation, such as `Assets/Asset/Dialogues`.
2. Right-click an empty area.
3. Select **Create > Dialogue > Conversation**.
4. Give the asset a descriptive name, such as `Guard conversation`.
5. Select the new asset to edit it in the Inspector.

## 2. Create the starting node

1. Set **Start Node Id** to `start`.
2. Expand **Nodes** and click **+**.
3. Expand the new element and set:
   - **Id**: `start`
   - **Speaker Name**: the name displayed above the dialogue
4. Expand **Dialogue Lines** and click **+** once for every line the character should say.
5. Enter the text in each element's **Text** field.

When the game is running, the player presses **F** to advance through these lines.

## 3. Add choices

1. Expand the node's **Options** list.
2. Click **+** for every choice you want to display.
3. For each option, set:
   - **Text**: the text displayed on the button
   - **Next Node Id**: the ID of the node that should play after this choice
   - **Ends Conversation**: enable this when the choice should close the dialogue immediately

The Dialogue Manager creates the required buttons automatically. You do not need to duplicate UI buttons manually.

If **Ends Conversation** is disabled, **Next Node Id** must exactly match another node's **Id**, including capitalization.

## 4. Add response or branch nodes

1. Add another element to the top-level **Nodes** list.
2. Give it a unique **Id**, such as `identity` or `permission`.
3. Add one or more **Dialogue Lines**.
4. Add more **Options** if the conversation should continue branching.

If a node has no options, the dialogue displays an **OK** button after its final line and the player can close it with **OK** or **F**.

Example structure:

```text
start
  Lines: Stop! / This area is restricted.
  Options:
    Who are you?  -> identity
    Let me pass.  -> permission
    Goodbye.      -> Ends Conversation

identity
  Lines: I am the elevator guard.
  Options: none

permission
  Lines: You do not have permission to enter.
  Options: none
```

## 5. Assign the conversation to an NPC

1. Select the NPC GameObject in the **Hierarchy**.
2. Confirm that it has a `Collider2D` with **Is Trigger** enabled.
3. Add or locate the **Dialogue Trigger** component.
4. Drag the conversation asset from the Project window into **Dialogue Trigger > Dialogue Asset > Conversation**.
5. Enter Play Mode, move the Player into the NPC trigger, and press **F**.

When a Conversation asset is assigned, the older Character and Conversation fields on `DialogueTrigger` are ignored. They remain visible only to keep older scene data compatible.

## Troubleshooting

- **Create > Dialogue > Conversation is missing**: wait for Unity to finish compiling and resolve any red Console errors.
- **F does nothing**: confirm the Player Input Actions contain `PC-Default/Dialogue` and that it is bound to F.
- **Dialogue does not open**: confirm the scene contains one active `DialogueManager` and the NPC collider is a trigger.
- **A choice reports a missing node**: make its **Next Node Id** exactly match the target node's **Id**, or enable **Ends Conversation**.
- **The wrong conversation appears**: check which Conversation asset is assigned to the NPC's Dialogue Trigger.
