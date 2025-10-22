// See https://aka.ms/new-console-template for more information
using BsDiff;
using System.CommandLine;
using System.ComponentModel;

// 1. 生成补丁
void CreatePatch(string oldFile, string newFile, string patchFile)
{
    Directory.CreateDirectory(Path.GetDirectoryName(patchFile)!);
    var oldBytes = File.ReadAllBytes(oldFile);
    var newBytes = File.ReadAllBytes(newFile);
    using var fs = File.Create(patchFile);
    BinaryPatch.Create(oldBytes, newBytes, fs);
    Console.WriteLine($"[生成] {patchFile}");
}

// 2. 应用补丁
void ApplyOnePatch(string oldFile, string patchFile, string outFile)
{
    Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);
    using var oldFs = File.OpenRead(oldFile);
    using var outFs = File.Create(outFile);
    BinaryPatch.Apply(oldFs, () => File.OpenRead(patchFile), outFs);
    Console.WriteLine($"[打补丁] {outFile}");
}
/* 批量：把 <patchDir> 里所有 *.patch 打到 <oldDir> 同名文件，输出到 <outDir> */
void ApplyDir(string oldDir, string patchDir, string outDir, bool inPlace)
{
    var patches = Directory.EnumerateFiles(patchDir, "*.patch", SearchOption.AllDirectories);
    foreach (var p in patches)
    {
        var rel = Path.GetRelativePath(patchDir, p);          // 保持子目录结构
        var old = Path.Combine(oldDir, rel[..^6]);           // 去掉 .patch
        var @out = inPlace ? old : Path.Combine(outDir, rel[..^6]);

        if (!File.Exists(old))
        {
            Console.WriteLine($"[跳过] 旧文件不存在：{old}");
            continue;
        }
        ApplyOnePatch(old, p, @out);
    }
}


// ==== 命令行定义 ====
var root = new RootCommand("BsDiff 补丁工具");

/* create 子命令 */
var createCmd = new Command("create", "生成补丁");
var oldArg = new Argument<string>("old") { Description= "旧文件"};
var newArg = new Argument<string>("new") { Description = "新文件"};
var patchArg = new Argument<string>("patch") { Description = "补丁文件"};
createCmd.Add(oldArg);
createCmd.Add(newArg);
createCmd.Add(patchArg);
root.Add(createCmd);


/* apply 子命令 */
var applyCmd = new Command("apply", "应用补丁");
var oldA = new Argument<string>("old") { Description = "旧文件" };
var patchA = new Argument<string>("patch") { Description = "新文件" };
var outA = new Argument<string>("out") { Description = "补丁文件" };
applyCmd.Add(oldA);
applyCmd.Add(patchA);
applyCmd.Add(outA);
root.Add(applyCmd);

/* apply-dir 子命令（批量目录） */
var applyDir = new Command("apply-dir", "批量应用目录下所有 *.patch");
var oldDir = new Argument<string>("oldDir") { Description = "新文件" };
var patchDir = new Argument<string>("patchDir") { Description = "新文件" };
var outDir = new Argument<string>("outDir") { Description = "新文件" };
var inPlace = new Option<bool>("--in-place");

root.Add(applyDir);

// 手动解析并分支
var parseResult = root.Parse(args);

if (parseResult.CommandResult.Command == createCmd)
{
    var old = parseResult.GetValue(oldArg);
    var @new = parseResult.GetValue(newArg);
    var patch = parseResult.GetValue(patchArg);
    CreatePatch(old!, @new!, patch!);
}
else if (parseResult.CommandResult.Command == applyCmd)
{
    var old = parseResult.GetValue(oldA);
    var patch = parseResult.GetValue(patchA);
    var @out = parseResult.GetValue(outA);
    ApplyOnePatch(old!, patch!, @out!);
}
else if (parseResult.CommandResult.Command == applyDir)
{
    var old = parseResult.GetValue(oldDir);
    var patch = parseResult.GetValue(patchDir);
    var @out = parseResult.GetValue(outDir);
    var inPlaceFlag = parseResult.GetValue(inPlace);
    ApplyDir(old!, patch!, @out!, inPlaceFlag);
}
else
{
    // 未匹配子命令 → 显示帮助
    parseResult.Invoke();   // 会打印自动生成的帮助
}