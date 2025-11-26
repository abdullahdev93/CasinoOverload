using UnityEngine;
using UnityEngine.SceneManagement;

public class QOL : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
