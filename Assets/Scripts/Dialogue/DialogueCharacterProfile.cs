using UnityEngine;

[CreateAssetMenu(fileName = "Employee Profile", menuName = "Dialogue/Employee Profile")]
public class DialogueCharacterProfile : ScriptableObject
{
    [SerializeField] private DialogueNameSource displayNameSource = DialogueNameSource.Custom;
    [SerializeField] private string displayName = "UNKNOWN";
    [SerializeField] private string caseNumber = "CASE # HR-00-0000 / REVIEW PENDING";
    [SerializeField] private string employeeId = "UNLISTED";
    [SerializeField] private string department = "UNKNOWN";
    [SerializeField] private string clearance = "LEVEL 00";
    [SerializeField] private string status = "ACTIVE";
    [SerializeField, Range(0, 100)] private int suspicion = 18;
    [SerializeField] private Sprite portrait;

    public string DisplayName => displayName;
    public string CaseNumber => caseNumber;
    public string EmployeeId => employeeId;
    public string Department => department;
    public string Clearance => clearance;
    public string Status => status;
    public int Suspicion => suspicion;
    public Sprite Portrait => portrait;

    public string GetDisplayName(string playerName)
    {
        return displayNameSource == DialogueNameSource.PlayerName ? playerName : displayName;
    }
}
