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
void ApplyPatch(string oldFile, string patchFile, string outFile)
{
    Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);
    using var oldFs = File.OpenRead(oldFile);
    using var outFs = File.Create(outFile);
    BinaryPatch.Apply(oldFs, () => File.OpenRead(patchFile), outFs);
    Console.WriteLine($"[打补丁] {outFile}");
}


// ==== 命令行定义 ====
var root = new RootCommand("BsDiff 补丁工具");

/* create 子命令 */
var createCmd = new Command("create", "生成补丁");
var oldArg = new Argument<string>("old");
var newArg = new Argument<string>("new");
var patchArg = new Argument<string>("patch");
createCmd.Add(oldArg);
createCmd.Add(newArg);
createCmd.Add(patchArg);
root.Add(createCmd);

/* apply 子命令 */
var applyCmd = new Command("apply", "应用补丁");
var oldA = new Argument<string>("old");
var patchA = new Argument<string>("patch");
var outA = new Argument<string>("out");
applyCmd.Add(oldA);
applyCmd.Add(patchA);
applyCmd.Add(outA);
root.Add(applyCmd);

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
    ApplyPatch(old!, patch!, @out!);
}
else
{
    // 未匹配子命令 → 显示帮助
    parseResult.Invoke();   // 会打印自动生成的帮助
}