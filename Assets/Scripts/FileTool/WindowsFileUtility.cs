using System;
//using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class WindowsFileUtility
{
    #region 目录选择对话框（用于选择保存目录）
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class BROWSEINFO
    {
        public IntPtr hwndOwner = IntPtr.Zero;
        public IntPtr pidlRoot = IntPtr.Zero;
        public IntPtr pszDisplayName = IntPtr.Zero;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpszTitle = "";
        public uint ulFlags = 0;
        public IntPtr lpfn = IntPtr.Zero;
        public int lParam = 0;
        public IntPtr iImage = IntPtr.Zero;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SHBrowseForFolder([In, Out] BROWSEINFO lpbi);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern bool SHGetPathFromIDList([In] IntPtr pidl, [Out] StringBuilder pszPath);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetActiveWindow();

    private const uint BIF_RETURNONLYFSDIRS = 0x0001;  // 只返回文件系统目录
    private const uint BIF_NEWDIALOGSTYLE = 0x0040;   // 现代对话框样式

    /// <summary>
    /// 弹出窗口选择保存目录
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="defaultPath">默认打开路径</param>
    /// <returns>选择的目录路径，用户取消则返回空</returns>
    public static string SelectSaveDirectory(string title, string defaultPath = "")
    {
        // 初始化对话框参数
        BROWSEINFO browseInfo = new BROWSEINFO();
        browseInfo.hwndOwner = GetActiveWindow();  // 绑定到当前活动窗口
        browseInfo.lpszTitle = title;
        browseInfo.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE;

        // 显示目录选择对话框
        IntPtr pidl = SHBrowseForFolder(browseInfo);
        if (pidl == IntPtr.Zero)
        {
            return string.Empty;  // 用户取消选择
        }

        // 获取选择的路径
        StringBuilder pathBuilder = new StringBuilder(260);  // 最大路径长度
        bool success = SHGetPathFromIDList(pidl, pathBuilder);
        Marshal.FreeCoTaskMem(pidl);  // 释放非托管内存

        return success ? pathBuilder.ToString() : string.Empty;
    }
    #endregion


    #region 打开资源管理器并选中文件
    /// <summary>
    /// 打开Windows资源管理器并选中指定文件
    /// </summary>
    /// <param name="filePath">文件的完整路径</param>
    /// <returns>操作是否成功</returns>
    public static bool OpenExplorerAndSelectFile(string filePath)
    {
        // 参数验证
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Debug.LogError("文件路径不能为空");
            return false;
        }

        // 检查文件是否存在
        if (!File.Exists(filePath))
        {
            Debug.LogError($"文件不存在: {filePath}");
            return false;
        }

        try
        {
            // 启动资源管理器，参数 /select 用于选中文件
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{filePath}\"",  // 关键参数：选中文件
                UseShellExecute = true  // 确保在Windows上正常运行
            });
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"打开资源管理器失败: {ex.Message}");
            return false;
        }
    }
    #endregion


    #region 快捷方法：选择目录并保存文件后自动选中
    /// <summary>
    /// 一站式流程：选择目录 → 保存文件 → 打开资源管理器并选中
    /// </summary>
    /// <param name="title">目录选择对话框标题</param>
    /// <param name="fileName">要保存的文件名（含扩展名）</param>
    /// <param name="saveAction">保存文件的回调方法（参数为完整路径）</param>
    public static void SaveAndOpenExplorer(string title, string fileName, Action<string> saveAction)
    {
        // 1. 选择保存目录
        string dirPath = SelectSaveDirectory(title);
        if (string.IsNullOrEmpty(dirPath))
        {
            Debug.Log("用户取消了保存");
            return;
        }

        // 2. 拼接完整文件路径
        string fullPath = Path.Combine(dirPath, fileName);

        // 3. 执行保存操作
        try
        {
            saveAction?.Invoke(fullPath);
            Debug.Log($"文件已保存: {fullPath}");

            // 4. 打开资源管理器并选中文件
            OpenExplorerAndSelectFile(fullPath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"保存文件失败: {ex.Message}");
        }
    }
    #endregion
}