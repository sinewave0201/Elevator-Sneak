using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class taskUIManager : MonoBehaviour
{
    [Serializable]
    private class SubTask
    {
        [SerializeField] private string subTaskName;
        [Tooltip("Assign the requirement that completes this subtask.")]
        [SerializeField] private InteractionRequirement subTaskReq;

        public string Name => subTaskName;
        public bool IsComplete => subTaskReq != null && subTaskReq.IsMet(null);
    }

    [Serializable]
    private class MainTaskTracker
    {
        [SerializeField] private string mainTaskName;
        [SerializeField] private List<SubTask> subTaskList = new List<SubTask>();
        [SerializeField] private string completedText = "Complete";

        public string Name => mainTaskName;

        public string GetSubTaskText(int currentSubTaskIndex)
        {
            if (subTaskList.Count == 0)
                return string.Empty;

            if (currentSubTaskIndex >= subTaskList.Count)
                return completedText;

            return subTaskList[currentSubTaskIndex].Name;
        }

        public int GetNextSubTaskIndex(int currentSubTaskIndex)
        {
            while (currentSubTaskIndex < subTaskList.Count && subTaskList[currentSubTaskIndex].IsComplete)
                currentSubTaskIndex++;

            return currentSubTaskIndex;
        }
    }

    [Header("Task UI")]
    [SerializeField] private TMP_Text mainTaskTxt;
    [SerializeField] private TMP_Text subTaskTxt;
    [SerializeField] private MainTaskTracker mainTask = new MainTaskTracker();

    private int displayedSubTaskIndex = -1;

    private void Start()
    {
        if (mainTaskTxt != null)
            mainTaskTxt.text = mainTask.Name;

        UpdateSubTask();
    }

    private void Update()
    {
        UpdateSubTask();
    }

    private void UpdateSubTask()
    {
        PlayerDataManager manager = PlayerDataManager.Instance;
        int index = 0;
        if (manager != null)
        {
            PlayerProgress progress = manager.Progress;
            index = mainTask.GetNextSubTaskIndex(progress.CurrentSubTaskIndex);
            if (index != progress.CurrentSubTaskIndex)
                progress.CurrentSubTaskIndex = index;
        }

        if (subTaskTxt == null || displayedSubTaskIndex == index)
            return;

        displayedSubTaskIndex = index;
        subTaskTxt.text = mainTask.GetSubTaskText(index);
    }
}
