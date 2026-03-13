using UnityEngine;
public enum NoteType
{
    Tap,
    Hold
}
public class Note : MonoBehaviour
{
    public int laneIndex;       // 0,1,2,3
    public float targetTime;
    public NoteType type;
    public float endTime;         // hold结束时间
    public bool isHeld;           // hold是否正在被按住
}