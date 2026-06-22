using UnityEngine; // Dòng này cực kỳ quan trọng để Unity hiểu QualitySettings và Application

public class FixLoeTrang : MonoBehaviour
{
    void Start()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
    }
}