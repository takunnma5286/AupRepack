using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using AupRename.RenameItems;
using Karoterra.AupDotNet;
using Karoterra.AupDotNet.ExEdit;
using Karoterra.AupDotNet.ExEdit.Effects;
using Karoterra.AupDotNet.Extensions;

namespace AupRename
{
    public class Renamer
    {
        public string Filename { get; set; } = "";

        public bool EnableVideo { get; set; }
        public bool EnableImage { get; set; }
        public bool EnableAudio { get; set; }
        public bool EnableWaveform { get; set; }
        public bool EnableShadow { get; set; }
        public bool EnableBorder { get; set; }
        public bool EnableVideoComposition { get; set; }
        public bool EnableImageComposition { get; set; }
        public bool EnableFigure { get; set; }
        public bool EnableMask { get; set; }
        public bool EnableDisplacement { get; set; }
        public bool EnablePartialFilter { get; set; }
        public bool EnableScript { get; set; }
        public bool EnablePsdToolKit { get; set; }

        public string Status { get; set; } = "";

        public void RepackProject()
        {
            if (!File.Exists(Filename))
            {
                ShowError(Properties.Resources.Error_FileNotFound);
                return;
            }

            using var dialog = new FolderBrowserDialog
            {
                Description = "コピー先のフォルダーを選択してください",
                ShowNewFolderButton = true,
            };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ExtractAndRepack(dialog.SelectedPath);
        }

        private void ExtractAndRepack(string destDir)
        {
            AviUtlProject? aup;
            ExEditProject? exedit;
            PsdToolKitProject? psdToolKit;
            var renameItems = new List<IRenameItem>();
            Status = "";

            try
            {
                aup = new AviUtlProject(Filename);
            }
            catch (Exception ex) when (ex is FileFormatException or EndOfStreamException)
            {
                ShowError(Properties.Resources.Error_NotAviUtlProjectFile);
                return;
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.Error_CorruptedAviUtlProjectFile);
                LogException(ex);
                return;
            }

            try
            {
                exedit = null;
                psdToolKit = null;
                for (int i = 0; i < aup.FilterProjects.Count; i++)
                {
                    if (aup.FilterProjects[i] is RawFilterProject filter)
                    {
                        if (filter.Name == "拡張編集")
                        {
                            exedit = new ExEditProject(filter);
                            aup.FilterProjects[i] = exedit;
                        }
                        else if (filter.Name == "Advanced Editing")
                        {
                            exedit = new EnglishExEditProject(filter);
                            aup.FilterProjects[i] = exedit;
                        }
                    }
                    if (aup.FilterProjects[i].Name == "PSDToolKit")
                    {
                        psdToolKit = new PsdToolKitProject(aup.FilterProjects[i]);
                        aup.FilterProjects[i] = psdToolKit;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(Properties.Resources.Error_CorruptedAviUtlProjectFile);
                LogException(ex);
                return;
            }

            if (exedit == null)
            {
                ShowError(Properties.Resources.Error_ExEditNotFound);
                return;
            }

            for (int objIdx = 0; objIdx < exedit.Objects.Count; objIdx++)
            {
                var obj = exedit.Objects[objIdx];
                if (obj.Chain) continue;

                for (int effectIdx = 0; effectIdx < obj.Effects.Count; effectIdx++)
                {
                    var effect = obj.Effects[effectIdx];
                    IRenameItem? renameItem = null;
                    if(EnableVideo && (renameItem = VideoFileRenameItem.CreateIfTarget(effect)) != null)
                    {
                        renameItems.Add(renameItem);
                    }
                    else if (EnableImage && (renameItem = ImageFileRenameItem.CreateIfTarget(effect)) != null)
                    {
                        renameItems.Add(renameItem);
                    }
                    else if (EnableAudio && (renameItem = AudioFileRenameItem.CreateIfTarget(effect)) != null)
                    {
                        renameItems.Add(renameItem);
                    }
                    else if (EnableWaveform && (renameItem = WaveformRenameItem.CreateIfTarget(effect)) != null)
                    {
                        renameItems.Add(renameItem);
                    }
                    else if (EnableShadow && (renameItem = ShadowRenameItem.CreateIfTarget(effect)) != null)
                    {
                        renameItems.Add(renameItem);
                    }
                    else if (EnableBorder && (renameItem = BorderRenameItem.CreateIfTarget(effect)) != null)
                    {
                        renameItems.Add(renameItem);
                    }
                    else if (EnableVideoComposition && (renameItem = VideoCompositionRenameItem.CreateIfTarget(effect)) != null)
                    {
                        renameItems.Add(renameItem);
                    }
                    else if (EnableImageComposition && (renameItem = ImageCompositionRenameItem.CreateIfTarget(effect)) != null)
                    {
                        renameItems.Add(renameItem);
                    }
                    else if (EnableFigure && (renameItem = FigureRenameItem.CreateIfTarget(effect)) != null)
                    {
                        renameItems.Add(renameItem);
                    }
                    else if (EnableMask && (renameItem = MaskRenameItem.CreateIfTarget(effect)) != null)
                    {
                        renameItems.Add(renameItem);
                    }
                    else if (EnableDisplacement && (renameItem = DisplacementRenameItem.CreateIfTarget(effect)) != null)
                    {
                        renameItems.Add(renameItem);
                    }
                    else if (EnablePartialFilter && (renameItem = PartialFilterRenameItem.CreateIfTarget(effect)) != null)
                    {
                        renameItems.Add(renameItem);
                    }
                    else if (EnableScript && (renameItem = ScriptFileRenameItem.CreateIfTarget(effect)) != null)
                    {
                        renameItems.Add(renameItem);
                    }
                }
            }
            if (psdToolKit != null && EnablePsdToolKit)
            {
                foreach (var psdImage in psdToolKit.Images)
                {
                    renameItems.Add(new PsdRenameItem(psdImage, exedit.Objects));
                }
            }

            if (renameItems.Count == 0)
            {
                ShowInfo(Properties.Resources.Message_NoFilesToEdit);
                return;
            }

            var newNames = new Dictionary<string, string>();
            var aupDir = Path.GetDirectoryName(Filename);
            if (string.IsNullOrEmpty(aupDir))
            {
                ShowError("プロジェクトファイルのフォルダーを取得できませんでした。");
                return;
            }

            var copiedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in renameItems)
            {
                if (newNames.TryGetValue(item.OldName, out var existingNewName))
                {
                    try
                    {
                        item.Rename(existingNewName);
                    }
                    catch (MaxByteCountOfStringException)
                    {
                        ShowError($"ファイル名が長すぎます: {existingNewName}");
                        return;
                    }
                    continue;
                }

                string srcPath;
                if (Path.IsPathRooted(item.OldName))
                {
                    srcPath = item.OldName;
                }
                else
                {
                    srcPath = Path.Combine(aupDir, item.OldName);
                }
                srcPath = Path.GetFullPath(srcPath);

                var newFileName = Path.GetFileName(srcPath);
                var destPath = Path.Combine(destDir, newFileName);

                if (!copiedFiles.Contains(srcPath))
                {
                    if (File.Exists(srcPath))
                    {
                        try
                        {
                            File.Copy(srcPath, destPath, true);
                            copiedFiles.Add(srcPath);
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
                        {
                            ShowError($"ファイルのコピーに失敗しました: {item.OldName} -> {destPath} - {ex.Message}");
                            return;
                        }
                    }
                    else
                    {
                        newNames[item.OldName] = item.OldName;
                        try
                        {
                            item.Rename(item.OldName);
                        }
                        catch (MaxByteCountOfStringException)
                        {
                            ShowError($"ファイル名が長すぎます: {item.OldName}");
                            return;
                        }
                        continue;
                    }
                }
                
                var newRelativePath = newFileName;
                newNames[item.OldName] = newRelativePath;

                try
                {
                    item.Rename(newRelativePath);
                }
                catch (MaxByteCountOfStringException)
                {
                    ShowError($"ファイル名が長すぎます: {newRelativePath}");
                    return;
                }
            }

            var newAupPath = Path.Combine(destDir, Path.GetFileName(Filename));
            try
            {
                using BinaryWriter writer = new(File.Create(newAupPath));
                aup.Write(writer);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                ShowError($"プロジェクトファイルの書き込みに失敗しました: {newAupPath} - {ex.Message}");
                return;
            }

            ShowInfo($"プロジェクトの再梱包が完了しました。");
            Status = $"プロジェクトを {destDir} に再梱包しました。";
        }

        private static void ShowError(string message)
        {
            System.Windows.MessageBox.Show(message, "AupRename", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void ShowInfo(string message)
        {
            System.Windows.MessageBox.Show(message, "AupRename", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static void LogException(Exception ex)
        {
            string logFilePath = "log.txt";

            try
            {
                File.AppendAllText(logFilePath,
                    $"[{DateTime.Now}] {ex.GetType()}: {ex.Message}\n{ex.StackTrace}\n\n"
                );
            }
            catch (Exception logEx)
            {
                ShowError(Properties.Resources.Error_LogWriteFailed + logEx.Message);
            }
        }
    }
}