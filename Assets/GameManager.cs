using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public AudioSource myMusic;
    public TMP_Text judgmentText;
    public float judgmentDisplayDuration = 0.5f;
    public GameObject notePrefab;
    public Transform[] spawnPoints;
    public Transform judgeLine;
    public float noteSpeed = 6f;
    public float songTime = 0f;
    private List<NoteData> chart = new List<NoteData>();//chart存储所有音符数据(相当于谱面导入)
    private List<Note> activeNotes = new List<Note>();//activeNotes存储当前场上存在的音符对象
    public class NoteData
    {
        public int lane;
        public float time;
        public NoteType type;
        public float endTime;
        public NoteData(int lane, float time, NoteType type, float endTime)
        {
            this.lane = lane;
            this.time = time;
            this.type = type;
            this.endTime = endTime;
        }
    }//存储单个音符的数据结构（轨道 出现时间 类型 结束时间）
    void Start()
    {
        chart = new List<NoteData>
        {
            new NoteData(0, 0f, NoteType.Tap, 0f),
            new NoteData(1, 0.5f, NoteType.Tap, 0f),
            new NoteData(2, 1.0f, NoteType.Tap, 0f),
            new NoteData(3, 1.5f, NoteType.Tap, 0f),

            new NoteData(0, 2.0f, NoteType.Tap, 0f),
            new NoteData(2, 2.0f, NoteType.Tap, 0f),

            new NoteData(1, 2.5f, NoteType.Tap, 0f),
            new NoteData(3, 2.5f, NoteType.Tap, 0f),

            new NoteData(0, 3f, NoteType.Tap, 0f),
            new NoteData(1, 3.5f, NoteType.Tap, 0f),
            new NoteData(2, 4.0f, NoteType.Tap, 0f),
            new NoteData(3, 4.5f, NoteType.Tap, 0f),

            new NoteData(3, 5f, NoteType.Tap, 0f),
            new NoteData(2, 5.5f, NoteType.Tap, 0f),
            new NoteData(1, 6.0f, NoteType.Tap, 0f),
            new NoteData(0, 6.5f, NoteType.Tap, 0f),

            new NoteData(1, 7.0f, NoteType.Tap, 0f),
            new NoteData(0, 7.25f, NoteType.Tap, 0f),
            new NoteData(1, 7.5f, NoteType.Tap, 0f),
            new NoteData(0, 7.75f, NoteType.Tap, 0f),
            new NoteData(1, 8.0f, NoteType.Tap, 0f),
            new NoteData(0, 8.25f, NoteType.Tap, 0f),

            new NoteData(2, 9.0f, NoteType.Hold, 3f),
            new NoteData(3, 9.5f, NoteType.Hold, 3f),
            new NoteData(0, 10.0f, NoteType.Hold, 3f),
            new NoteData(1, 10.5f, NoteType.Hold, 3f),
            new NoteData(2, 11.0f, NoteType.Hold, 3f),
            new NoteData(3, 11.5f, NoteType.Hold, 3f)
            //hold测试（8hold）
            //之后生成谱面大概这么写（？
        };
    }
    void Update()
    {
        songTime += Time.deltaTime;//时间更新

        while (chart.Count > 0 && chart[0].time <= songTime + 1f)//提前1s
        {
            NoteData data = chart[0];
            chart.RemoveAt(0);
            SpawnNote(data);//生成音符
        }

        UpdateNotes();
        HandleInput();
    }

    void SpawnNote(NoteData data)
    {
        Vector3 spawnPos = spawnPoints[data.lane].position;
        GameObject newNote = Instantiate(notePrefab, spawnPos, Quaternion.identity);
        Note noteComp = newNote.GetComponent<Note>();
        noteComp.laneIndex = data.lane;
        noteComp.targetTime = data.time;
        noteComp.type = data.type;
        noteComp.endTime = data.endTime;
        noteComp.isHeld = false;
        activeNotes.Add(noteComp);
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.A)) JudgeLane(0, true);
        if (Input.GetKeyDown(KeyCode.S)) JudgeLane(1, true);
        if (Input.GetKeyDown(KeyCode.D)) JudgeLane(2, true);
        if (Input.GetKeyDown(KeyCode.F)) JudgeLane(3, true);

        if (Input.GetKeyUp(KeyCode.A)) JudgeLane(0, false);
        if (Input.GetKeyUp(KeyCode.S)) JudgeLane(1, false);
        if (Input.GetKeyUp(KeyCode.D)) JudgeLane(2, false);
        if (Input.GetKeyUp(KeyCode.F)) JudgeLane(3, false);//判断抬手hold用
    }

    void JudgeLane(int lane, bool isPressDown)
    {
        Note bestNote = null;
        float minDiff = float.MaxValue;

        foreach (Note note in activeNotes)
        {
            if (note.laneIndex != lane) continue;
            float diff = Mathf.Abs(note.targetTime - songTime);
            if (diff < minDiff)
            {
                minDiff = diff;
                bestNote = note;
            }
        }

        float perfect = 0.1f;
        float great = 0.2f;
        float good = 0.3f;
        float miss = 0.4f;

        if (bestNote != null && minDiff <= miss)
        {
            string result;
            if (minDiff <= perfect) result = "Perfect";
            else if (minDiff <= great) result = "Great";
            else if (minDiff <= good) result = "Good";
            else result = "Miss";

            Debug.Log($"Lane {lane} {result} ({minDiff:F2}s)");
            if (judgmentText != null)
            {
                judgmentText.text = result;
                judgmentText.color = Color.black;
                CancelInvoke(nameof(ClearJudgmentText));
                Invoke(nameof(ClearJudgmentText), judgmentDisplayDuration);
            }

            activeNotes.Remove(bestNote);
            Destroy(bestNote.gameObject);
        }
    }

    private void UpdateNotes()//移动销毁音符
    {
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            Note note = activeNotes[i];
            float timeLeft = note.targetTime - songTime;
            float targetZ = judgeLine.position.z + (timeLeft * noteSpeed);
            Vector3 pos = note.transform.position;
            pos.z = targetZ;
            note.transform.position = pos;

            if (note.transform.position.z < judgeLine.position.z - 2f)
            {
                activeNotes.RemoveAt(i);
                Destroy(note.gameObject);
                judgmentText.text = "Miss";
                judgmentText.color = Color.gray;
            }
        }
    }

    // 清除文本内容
    private void ClearJudgmentText()
    {
        if (judgmentText != null)
            judgmentText.text = "";
    }
}