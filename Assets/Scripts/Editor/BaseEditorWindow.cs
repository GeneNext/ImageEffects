using UnityEngine;
using UnityEditor;
using System.Collections;

public class BaseEditorWindow<WindowType> : EditorWindow where WindowType : EditorWindow
{
    public static WindowType Window
    {
        get;
        private set;
    }

    public static bool IsOpen
    {
        get { return Window != null; }
    }

    protected GUILayoutOption[] m_options;

    protected virtual void OnEnable()
    {
        m_options = new GUILayoutOption[0];
    }

    public static void OpenWindow()
    {
        if (IsOpen)
        {
            return;
        }

        Window = EditorWindow.GetWindow<WindowType>();
        Window.Show();
    }

    public static void CloseWindow()
    {
        if (!IsOpen)
        {
            return;
        }

        Window.Close();
        Window = null;
    }
}
