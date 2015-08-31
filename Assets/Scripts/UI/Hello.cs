using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 场景导航
/// </summary>
public class Hello : MonoBehaviour
{
    public void EnterScene(string sceneName)
    {
        Application.LoadLevel(sceneName);
    }
    public void EnterLevel(int id)
    {
        Application.LoadLevel(id);
    }
}