using UnityEngine;

public class ToggleGameobject : MonoBehaviour
{
    [SerializeField] private bool m_enableOnDesktop = true;
    [SerializeField] private bool m_enableOnMobile = true;
    [SerializeField] private bool m_enableOnConsole = true;

    private void Awake()
    {
        gameObject.SetActive(false);

        switch (SystemInfo.deviceType)
        {
            case DeviceType.Handheld:
                if (m_enableOnMobile) gameObject.SetActive(true);
                break;
            case DeviceType.Console:
                if (m_enableOnConsole) gameObject.SetActive(true);
                break;
            case DeviceType.Desktop:
                if (m_enableOnDesktop) gameObject.SetActive(true);
                break;
        }
    }
}