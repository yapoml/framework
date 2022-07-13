﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Text;
using Yapoml.Framework.Workspace.Parsers;

BenchmarkRunner.Run<BenchmarkFixture>();

[MemoryDiagnoser]
public class BenchmarkFixture
{
    private string _pageContent;

    public BenchmarkFixture()
    {
        var stringBuilder = new StringBuilder();
        for (int i = 0; i < 100; i++)
        {
            stringBuilder.AppendLine($"{new string(' ', i * 2)}MyComponent{i}:");
            stringBuilder.AppendLine($"{new string(' ', (i + 1) * 2)}by: ./qwe");

            for (int j = 0; j < 5; j++)
            {
                stringBuilder.AppendLine($"{new string(' ', (i + 1) * 2)}MyOtherComponent{j}:");
                stringBuilder.AppendLine($"{new string(' ', (i + 1) * 2 + 2)}by: ./qwe");
            }
        }
        
        _pageContent = stringBuilder.ToString();
    }

    [Benchmark]
    public void Test500()
    {
        var builder = new Yapoml.Framework.Workspace.WorkspaceContextBuilder(Environment.CurrentDirectory, "A.B", new WorkspaceParser());

        builder.AddFile(Environment.CurrentDirectory + "/MyPage.po.yaml", _pageContent);

        builder.Build();
    }
}