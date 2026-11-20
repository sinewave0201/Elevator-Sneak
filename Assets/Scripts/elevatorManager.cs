using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class elevatorManager : MonoBehaviour
{
    private static readonly int OpenTrigger = Animator.StringToHash("open");
    private static readonly int CloseTrigger = Animator.StringToHash("close");
    private static readonly int OpenState = Animator.StringToHash("open");
    private static readonly int CloseState = Animator.StringToHash("close");
    private static readonly int IdleState = Animator.StringToHash("idl");

    [Serializable]
    private struct elevatorStorage
    {
        public int entryLevel;
        public DialogueTrigger NPC;
        public GameObject NPCportrait;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("UI")]
    [SerializeField] private TMP_Text level;

    [Header("Elevator logic")]
    [SerializeField] private int startLevel;
    [SerializeField] private int secondsPerLevel;
    [SerializeField] private int targetLevel;
    private int curLevel;
    private PlayerTaskTimer taskTimer;
    private float previousRemainingTime;
    private float floorTimer;

    [SerializeField] private GameObject diaCanvas;
    [SerializeField] private List<elevatorStorage> NPCList = new List<elevatorStorage>();
    private bool haveNPC = false;

    [Header("Animation")]
    [SerializeField] private Animator elevatorAnim;

    void Start()
    {
        curLevel = startLevel;

        taskTimer = PlayerDataManager.Instance.TaskTimer;
        previousRemainingTime = taskTimer.RemainingSeconds;

        taskTimer.TimeChanged += HandleTimeChanged;

        UpdateLevelUI();
    }

    void Update()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;

        if (dialogueManager == null || !dialogueManager.IsOpen)
            return;

        bool pressedF =
            Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame;

        bool clicked =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        if (pressedF || clicked)
            dialogueManager.AdvanceOrClose();
    }

    private void OnDestroy()
    {
        if (taskTimer != null)
            taskTimer.TimeChanged -= HandleTimeChanged;
    }

    private void HandleTimeChanged(float remainingTime)
    {
        float elapsedTime = Mathf.Max(0f, previousRemainingTime - remainingTime);
        previousRemainingTime = remainingTime;

        floorTimer += elapsedTime;

        while (floorTimer >= secondsPerLevel && curLevel != targetLevel)
        {
            floorTimer -= secondsPerLevel;

            int direction = targetLevel > curLevel ? 1 : -1;
            curLevel += direction;

            UpdateLevelUI();
            CheckNpcAtCurrentLevel();
        }
    }

    private void UpdateLevelUI()
    {
        level.text = curLevel.ToString();
    }
    
    private void CheckNpcAtCurrentLevel()
    {
        if (haveNPC)
            return;

        foreach (elevatorStorage npcEntry in NPCList)
        {
            if (npcEntry.entryLevel != curLevel || npcEntry.NPC == null)
                continue;

            haveNPC = true;
            StartCoroutine(HandleNpcArrival(npcEntry));
            break;
        }
    }

    private IEnumerator HandleNpcArrival(elevatorStorage npcEntry)
    {
        diaCanvas.SetActive(true);

        if (elevatorAnim != null)
        {
            elevatorAnim.ResetTrigger(CloseTrigger);
            elevatorAnim.SetTrigger(OpenTrigger);

            yield return WaitForAnimatorState(OpenState);
            yield return WaitForAnimationToFinish(OpenState);
        }

        if (npcEntry.NPCportrait != null)
            npcEntry.NPCportrait.SetActive(true);

        if (elevatorAnim != null)
        {
            elevatorAnim.SetTrigger(CloseTrigger);

            yield return WaitForAnimatorState(CloseState);
            yield return WaitForAnimatorState(IdleState, true);
        }

        DialogueManager.Instance.StartDialogue(npcEntry.NPC);
    }

    private IEnumerator WaitForAnimatorState(int stateHash, bool waitForTransition = false)
    {
        while (true)
        {
            AnimatorStateInfo currentState = elevatorAnim.GetCurrentAnimatorStateInfo(0);
            bool reachedState = currentState.shortNameHash == stateHash;

            if (reachedState && (!waitForTransition || !elevatorAnim.IsInTransition(0)))
                yield break;

            yield return null;
        }
    }

    private IEnumerator WaitForAnimationToFinish(int stateHash)
    {
        while (true)
        {
            AnimatorStateInfo state = elevatorAnim.GetCurrentAnimatorStateInfo(0);

            if (state.shortNameHash != stateHash ||
                (state.normalizedTime >= 1f && !elevatorAnim.IsInTransition(0)))
            {
                yield break;
            }

            yield return null;
        }
    }
}
