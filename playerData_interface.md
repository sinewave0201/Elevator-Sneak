# Player Data Interface


## Quick start

```csharp
PlayerDataManager manager = PlayerDataManager.Instance;

PlayerStats stats = manager.Stats;
PlayerInventory inventory = manager.Inventory;
PlayerProgress progress = manager.Progress;
PlayerDisguise disguise = manager.Disguise;
PlayerTaskTimer taskTimer = manager.TaskTimer;
```

The five modules store different kinds of player data:

| Module | C# data | Meaning in the game |
|---|---|---|
| Stats | `float` | Numeric values such as suspicion |
| Inventory | `ItemDefinition` + `int` | Items owned and their quantities |
| Progress | `ProgressFlag` + `bool` | Story facts that are complete or incomplete |
| Disguise | `DisguiseType` | What the player is currently wearing |
| Task Timer | `TimedTaskDefinition` + `float` | The current timed task and remaining seconds |

The manager prefab must be instantiated before `PlayerDataManager.Instance` is used:

```text
Assets/Prefabs/Systems/PlayerDataManager.prefab
```

It has not been added to a production scene yet because scene integration will be done separately.

## 1. Stats

`PlayerStatType` selects which number to use:

```text
PlayerStatType.Suspicion                current suspicion, normally 0-100
PlayerStatType.SuspicionGainMultiplier  multiplier used when adding suspicion
PlayerStatType.SuspicionDecayRate       intended suspicion reduction per second
```

### Get

```csharp
float Get(PlayerStatType type)
```

```csharp
PlayerStats stats = PlayerDataManager.Instance.Stats;
float currentSuspicion = stats.Get(PlayerStatType.Suspicion);
```

### Set

```csharp
bool Set(PlayerStatType type, float value)
```

```csharp
PlayerStats stats = PlayerDataManager.Instance.Stats;
bool suspicionWasSet = stats.Set(PlayerStatType.Suspicion, 40f);
// suspicionWasSet is true when Suspicion is configured and Set succeeded.
// It is false when that stat is not configured, so no value could be set.
```

### Modify

```csharp
bool Modify(PlayerStatType type, float amount)
```

```csharp
PlayerStats stats = PlayerDataManager.Instance.Stats;
bool suspicionWasIncreased = stats.Modify(PlayerStatType.Suspicion, 5f);
bool suspicionWasDecreased = stats.Modify(PlayerStatType.Suspicion, -5f);
// Each bool is true if Suspicion was modified, or false if that stat was not configured.
```

### Add or reduce suspicion

```csharp
void AddSuspicion(float amount)
void ReduceSuspicion(float amount)
```

```csharp
PlayerStats stats = PlayerDataManager.Instance.Stats;
stats.AddSuspicion(15f);
stats.ReduceSuspicion(10f);
// These methods return nothing. They directly update the Suspicion value.
```

`AddSuspicion` applies `SuspicionGainMultiplier`. `Set` and `Modify` do not.

### Get a 0-1 value for UI

```csharp
float GetNormalized(PlayerStatType type)
```

```csharp
PlayerStats stats = PlayerDataManager.Instance.Stats;
float suspicionBarValue = stats.GetNormalized(PlayerStatType.Suspicion);
```

`GetNormalized` converts a stat from its configured minimum-maximum range to a value between `0` and `1`. For example, if Suspicion is `68` on a `0-100` range, this returns `0.68`, which a UI progress bar can display as 68% full.

## 2. Inventory

An `ItemDefinition` represents an item type such as a Work Uniform. A gameplay script normally receives the item through the Inspector:

```csharp
[SerializeField] private ItemDefinition workUniform;
```

### Check for an item

```csharp
bool HasItem(ItemDefinition item, int amount = 1)
```

```csharp
PlayerInventory inventory = PlayerDataManager.Instance.Inventory;
bool hasUniform = inventory.HasItem(workUniform, 1);
// hasUniform is true if the player owns at least one Work Uniform; otherwise it is false.
```

### Get item quantity

```csharp
int GetItemCount(ItemDefinition item)
```

```csharp
PlayerInventory inventory = PlayerDataManager.Instance.Inventory;
int uniformCount = inventory.GetItemCount(workUniform);
// uniformCount is the total number of Work Uniforms currently owned by the player.
```

### Add an item

```csharp
bool AddItem(ItemDefinition item, int amount = 1)
```

```csharp
PlayerInventory inventory = PlayerDataManager.Instance.Inventory;
bool uniformWasAdded = inventory.AddItem(workUniform, 1);
// uniformWasAdded is true if the complete amount was added; otherwise it is false.
```

### Remove an item

```csharp
bool RemoveItem(ItemDefinition item, int amount = 1)
```

```csharp
PlayerInventory inventory = PlayerDataManager.Instance.Inventory;
bool uniformWasRemoved = inventory.RemoveItem(workUniform, 1);
// uniformWasRemoved is true if the complete amount was removed; otherwise it is false.
```

## 3. Progress

Progress stores story facts:

```text
ProgressFlag.SpokeToGuard
ProgressFlag.WorkUniformObtained
ProgressFlag.ElevatorAccessGranted
```

### Check progress

```csharp
bool HasFlag(ProgressFlag flag)
```

```csharp
PlayerProgress progress = PlayerDataManager.Instance.Progress;
bool spokeToGuard = progress.HasFlag(ProgressFlag.SpokeToGuard);
// spokeToGuard is true if the player has spoken to the guard; 
```

### Set progress

```csharp
bool SetFlag(ProgressFlag flag, bool completed = true)
```

```csharp
PlayerProgress progress = PlayerDataManager.Instance.Progress;

//set progress "has spoken to guard" as complete.
bool conversationWasRecorded = progress.SetFlag(ProgressFlag.SpokeToGuard); 

//set progress "elevator access granted" as imcomplete.
bool accessWasRemoved = progress.SetFlag(ProgressFlag.ElevatorAccessGranted, false); 

// Every progress flag is incomplete by default; Use `SetFlag(flag, false)` only when completed progress needs to be revoked.
// It returns true if the progress changed; otherwise it returns false.
```

## 4. Disguise

```text
DisguiseType.None    no disguise equipped
DisguiseType.Worker  worker disguise equipped
```

Inventory means the player owns a uniform. Disguise means the player is currently wearing it.

### Check a disguise

```csharp
bool IsWearing(DisguiseType disguise)
```

```csharp
PlayerDisguise disguise = PlayerDataManager.Instance.Disguise;
bool wearingUniform = disguise.IsWearing(DisguiseType.Worker);
// wearingUniform is true if the Worker disguise is currently equipped; 
```

### Equip a disguise

```csharp
bool Equip(DisguiseType disguise)
```

```csharp
PlayerDisguise disguise = PlayerDataManager.Instance.Disguise;
bool uniformWasEquipped = disguise.Equip(DisguiseType.Worker);
// uniformWasEquipped is true if the disguise changed to Worker.
// It is false if the player was already wearing the Worker disguise.
```

### Remove a disguise

```csharp
bool Remove()
```

```csharp
PlayerDisguise disguise = PlayerDataManager.Instance.Disguise;
bool disguiseWasRemoved = disguise.Remove();
// disguiseWasRemoved is true if an equipped disguise was removed.
// It is false if the player was already wearing no disguise.
```

## 5. Task Timer

The Task Timer manages one timed task at a time. It stores the active task, counts down its remaining time, and reports when time runs out.

### What the timer data means in the game

| Data | Type | Meaning in the game |
|---|---|---|
| `TimedTaskDefinition.DisplayName` | `string` | The task name shown to the player, such as `Get Work Uniform` |
| `TimedTaskDefinition.TimeLimitSeconds` | `float` | The total number of seconds allowed for that task |
| `CurrentTask` | `TimedTaskDefinition` | The task the player is currently trying to finish; `null` means no timed task is active |
| `RemainingSeconds` | `float` | The number of seconds the player has left |
| `IsRunning` | `bool` | `true` while the timer is counting down; otherwise `false` |
| `CurrentTimeMultiplier` | `float` | The current countdown speed: normally `1`, or `2` in the staircase scene by default |
| `TravelMethod.Elevator` | `TravelMethod` | Use the elevator travel multiplier; currently `1x` |
| `TravelMethod.Stairs` | `TravelMethod` | Use the stairs travel multiplier; currently `2x` |
| `TimeChanged` | Event | Tells the UI that the displayed time should be updated |
| `TimeExpired` | Event | Tells the future Game Over system that the player ran out of time |

### Create a timed task

Create one task asset for each timed objective with:

```text
Create > Player Data > Timed Task
```

For example, select `GetUniformTask.asset` in Unity and set its `Display Name` to `Get Work Uniform` and its `Time Limit Seconds` to `180` in the asset's Inspector.

A gameplay `MonoBehaviour` can then contain this field:

```csharp
[SerializeField] private TimedTaskDefinition getUniformTask;
```

The task's name and time limit are edited on the task asset

### Start a task

```csharp
void StartTask(TimedTaskDefinition task)
```

```csharp
PlayerTaskTimer timer = PlayerDataManager.Instance.TaskTimer;
timer.StartTask(getUniformTask);
// The timer reads the time limit from getUniformTask and begins counting down.
```

### Read the current timer state

```csharp
TimedTaskDefinition CurrentTask { get; }
float RemainingSeconds { get; }
bool IsRunning { get; }
float CurrentTimeMultiplier { get; }
```

```csharp
PlayerTaskTimer timer = PlayerDataManager.Instance.TaskTimer;
TimedTaskDefinition activeTask = timer.CurrentTask;
float secondsLeft = timer.RemainingSeconds;
bool timerIsRunning = timer.IsRunning;
float countdownSpeed = timer.CurrentTimeMultiplier;
// activeTask is the current objective, secondsLeft is its remaining time,
// timerIsRunning tells whether the countdown is active, and countdownSpeed
// shows the multiplier selected from the current scene.
```

### Complete the current task

```csharp
void CompleteCurrentTask()
```

```csharp
PlayerTaskTimer timer = PlayerDataManager.Instance.TaskTimer;
timer.CompleteCurrentTask();
// The task was completed in time, so the timer stops and clears the active task.
```

### How the clock continuously counts down

Gameplay code does not repeatedly call `SpendTravelTime`. `PlayerDataManager` runs the normal clock automatically every frame:

```csharp
void Update()
{
    data.TaskTimer.Tick(Time.deltaTime);
}
```

`Time.deltaTime` is the scaled game time that passed since the previous frame. `Tick` multiplies it by `CurrentTimeMultiplier` and removes that amount every frame. At `1x`, one real-time second removes about one timer second. At `2x`, one real-time second removes about two timer seconds.

### Automatic scene multiplier

`PlayerDataManager` listens for scene changes and selects the timer multiplier automatically:

```text
StaircaseScene -> Stairs multiplier, currently 2x
ElevatorScene  -> Elevator multiplier, currently 1x
Any other scene -> Normal 1x multiplier
```

The Interaction System only needs to load the correct scene. It should not call `SpendTravelTime`, because the Time System now handles the scene multiplier itself.

### Spend a fixed amount of time

```csharp
void SpendTime(float seconds)
```

```csharp
PlayerTaskTimer timer = PlayerDataManager.Instance.TaskTimer;
timer.SpendTime(15f);
// Removes 15 seconds from the active task without applying a travel multiplier.
```

### Handle time running out

```csharp
PlayerTaskTimer timer = PlayerDataManager.Instance.TaskTimer;
timer.TimeExpired += HandleTimeExpired;
// += subscribes the HandleTimeExpired function to the event.
// It does not add a number to TimeExpired.

void HandleTimeExpired()
{
    // Tell the future Game Over system that the player failed.
}
```

`TimeExpired` is an event: it stores a list of functions that should be called later. When `RemainingSeconds` reaches zero, `PlayerTaskTimer` stops itself and runs `TimeExpired?.Invoke()`, which calls every subscribed function. Because `IsRunning` becomes `false` first, later frames cannot expire the same timer again, so the event runs once.

`Tick(float deltaTime)` and `Reset()` are called by `PlayerDataManager`; normal gameplay scripts do not need to call them directly.

## 6. Gameplay Examples

### Complete timed task flow

This example uses one shared `manager` field and one shared `timer` field. Dialogue and reward code call different methods on the same integration component. Scene changes do not need timer code because `PlayerDataManager` applies the travel multiplier automatically.

```csharp
public class TimedUniformTaskIntegration : MonoBehaviour
{
    [SerializeField] private TimedTaskDefinition getUniformTask;
    [SerializeField] private ItemDefinition workUniform;

    private PlayerDataManager manager;
    private PlayerTaskTimer timer;

    private void Awake()
    {
        manager = PlayerDataManager.Instance;
        timer = manager.TaskTimer;
        timer.TimeExpired += HandleTimeExpired;
    }

    public void OnUniformTaskAccepted()
    {
        // Dialogue calls this when the NPC gives the player the objective.
        timer.StartTask(getUniformTask);
    }

    public void OnUniformReceived()
    {
        // The reward or pickup system calls this when the uniform is obtained.
        bool uniformWasAdded = manager.Inventory.AddItem(workUniform, 1);

        if (!uniformWasAdded)
            return;

        bool progressWasRecorded = manager.Progress.SetFlag(
            ProgressFlag.WorkUniformObtained);

        timer.CompleteCurrentTask();
    }

    private void HandleTimeExpired()
    {
        // The future Game Over system fails the game here.
    }

    private void OnDestroy()
    {
        timer.TimeExpired -= HandleTimeExpired;
    }
}
```

The methods are connected through the shared fields: `Awake` gets the one Singleton manager once, `OnUniformTaskAccepted` starts its timer, and `OnUniformReceived` completes that same timer. If the player enters `StaircaseScene` between those actions, the Time System automatically changes the countdown to the stairs multiplier. `OnDestroy` unsubscribes the event when this integration component is removed.

### Give a uniform after dialogue

```csharp
PlayerDataManager manager = PlayerDataManager.Instance;
PlayerInventory inventory = manager.Inventory;
PlayerProgress progress = manager.Progress;

bool uniformWasAdded = inventory.AddItem(workUniform, 1);

if (uniformWasAdded)
{
    bool progressWasRecorded = progress.SetFlag(ProgressFlag.WorkUniformObtained);
}
```

### Equip a uniform only when it is owned

```csharp
PlayerDataManager manager = PlayerDataManager.Instance;
PlayerInventory inventory = manager.Inventory;
PlayerDisguise disguise = manager.Disguise;

bool hasUniform = inventory.HasItem(workUniform, 1);

if (hasUniform)
{
    bool uniformWasEquipped = disguise.Equip(DisguiseType.Worker);
}
```

### Check whether the elevator can be used

```csharp
PlayerDataManager manager = PlayerDataManager.Instance;

float suspicion = manager.Stats.Get(PlayerStatType.Suspicion);
bool hasAccess = manager.Progress.HasFlag(ProgressFlag.ElevatorAccessGranted);
bool wearingUniform = manager.Disguise.IsWearing(DisguiseType.Worker);

bool canEnterElevator =
    suspicion <= 50f &&
    hasAccess &&
    wearingUniform;
```

## 7. New Game Reset

```csharp
void StartNewGame()
```

```csharp
PlayerDataManager manager = PlayerDataManager.Instance;
manager.StartNewGame();
```

This restores starting stats, empties Inventory, clears Progress, restores the starting Disguise, and clears the current task timer. Save/load to disk is not implemented.

## 8. Less Common Methods

Other functions that may not be used that often:
(for simplicity purpose, I didn't include them above)

```csharp
bool HasStat(PlayerStatType type)
bool TryGet(PlayerStatType type, out float value)
float GetMinimum(PlayerStatType type)
float GetMaximum(PlayerStatType type)

bool CanAddItem(ItemDefinition item, int amount = 1)
int Capacity { get; }
IReadOnlyList<InventorySlot> Slots { get; }

DisguiseType Current { get; }
```

Use `Get` for normal stat reads. `HasStat` and `TryGet` only exist for optional data and generic tools. Change events also exist for future UI work, but they are intentionally omitted from this quick interface.
