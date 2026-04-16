using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

public class MusicTrayApp : Form
{
        void ShowWindow()
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.BringToFront();
    }

    void HideToTray()
    {
        this.Hide();
    }
    // WinAPI
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern void SendInput(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    const int WM_HOTKEY = 0x0312;

    const uint MOD_CONTROL = 0x0002;

    const byte VK_MEDIA_PLAY_PAUSE = 0xB3;
    const byte VK_MEDIA_NEXT = 0xB0;
    const byte VK_MEDIA_PREV = 0xB1;
    const byte VK_VOLUME_UP = 0xAF;
    const byte VK_VOLUME_DOWN = 0xAE;
    const byte VK_VOLUME_MUTE = 0xAD;



    NotifyIcon trayIcon;

    Dictionary<int, Action> actions = new Dictionary<int, Action>();
    int hotkeyId = 0;

    public MusicTrayApp()
    {
        // приложуха будет в панели задач
        this.ShowInTaskbar = true;
        this.WindowState = FormWindowState.Normal;
        this.StartPosition = FormStartPosition.CenterScreen;


        // трей иконка
        trayIcon = new NotifyIcon();
        trayIcon.Icon = SystemIcons.Application;
        trayIcon.Visible = true;
        trayIcon.Text = "Mini Music Player";

        ContextMenuStrip menu = new ContextMenuStrip();
        menu.Items.Add("Открыть", null, (s, e) => ShowWindow());
        menu.Items.Add("Скрыть в трей", null, (s, e) => HideToTray());
        menu.Items.Add("Выход", null, (s, e) => Application.Exit());

        trayIcon.ContextMenuStrip = menu;

        trayIcon.MouseDoubleClick += (s, e) => ShowWindow();
    }

        // бинды
protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        RegisterHotkey(MOD_CONTROL, (uint)Keys.Space, PlayPause);
        RegisterHotkey(MOD_CONTROL, (uint)Keys.Right, NextTrack);
        RegisterHotkey(MOD_CONTROL, (uint)Keys.Left, PrevTrack);
        RegisterHotkey(MOD_CONTROL, (uint)Keys.Up, VolumeUp);
        RegisterHotkey(MOD_CONTROL, (uint)Keys.Down, VolumeDown);
        RegisterHotkey(MOD_CONTROL, (uint)Keys.M, Mute);
    }


void RegisterHotkey(uint mod, uint key, Action action)
    {
        hotkeyId++;
        RegisterHotKey(this.Handle, hotkeyId, mod, key);
        actions.Add(hotkeyId, action);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY)
        {
            int id = m.WParam.ToInt32();
            if (actions.ContainsKey(id))
                actions[id]();
        }

        base.WndProc(ref m);
    }

    void PlayPause() => Press(VK_MEDIA_PLAY_PAUSE);
    void NextTrack() => Press(VK_MEDIA_NEXT);
    void PrevTrack() => Press(VK_MEDIA_PREV);
    void VolumeUp() => Press(VK_VOLUME_UP);
    void VolumeDown() => Press(VK_VOLUME_DOWN);
    void Mute() => Press(VK_VOLUME_MUTE);


    void Press(byte key)
    {
        SendInput(key, 0, 0, 0);
        SendInput(key, 0, 2, 0);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        foreach (var id in actions.Keys)
            UnregisterHotKey(this.Handle, id);

        trayIcon.Visible = false;
        base.OnFormClosing(e);
    }

    // полностью убираем окно
    bool firstTime = true;

    protected override void SetVisibleCore(bool value)
    {
        if (firstTime)
        {
            base.SetVisibleCore(false);
            firstTime = false;
            return;
        }

        base.SetVisibleCore(value);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (this.WindowState == FormWindowState.Minimized)
        {
            HideToTray();
        }
    }

}
