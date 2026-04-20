using UnityEngine;

public class AppManager : PersistentSingleton<AppManager>
{
    private float backBtnTime = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.time - backBtnTime < 2f)
            {
                Application.Quit();
            }
            else
            {
                backBtnTime = Time.time;
            }
        }
    }

   
}
