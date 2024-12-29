using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using Anna.IntegrationTests.Contexts;
using FluentAssertions;
using Reqnroll;

namespace Anna.IntegrationTests.Hooks;

[Binding]
public class ContainerHook
{
    private static string ContainerEngine = ChooseContainerEngine();
    private static string ContainerId = null;
    private static bool Enable = bool.Parse(Environment.GetEnvironmentVariable("RUN_INTEGRATION_TESTS"));

    [BeforeTestRun]
    public static void StartContainer(HttpContext httpContext)
    {
        if (!Enable)
        {
            return;
        }

        // TODO: semaphore is probably better, also prevents a race condition
        if (ContainerId is not null)
        {
            throw new InvalidOperationException("Container is already running, cannot start");
        }

        // Build container
        var buildProc = new Process();
        buildProc.StartInfo.FileName = ContainerEngine;
        buildProc.StartInfo.WorkingDirectory = "../../../.."; // This is not great...
        buildProc.StartInfo.Arguments = "build -t anna:edge .";
        buildProc.StartInfo.CreateNoWindow = true;
        buildProc.Start();
        buildProc.WaitForExit();
        buildProc.ExitCode.Should().Be(0);

        const int HttpPort = 8080;

        var runProc = new Process();
        runProc.StartInfo.FileName = ContainerEngine;
        runProc.StartInfo.Arguments = $"run --rm -d -p {HttpPort}:8080 anna:edge";
        runProc.StartInfo.CreateNoWindow = true;
        runProc.StartInfo.RedirectStandardOutput = true;
        runProc.Start();
        runProc.WaitForExit();
        runProc.ExitCode.Should().Be(0);

        Thread.Sleep(TimeSpan.FromSeconds(3));

        var host = new Uri($"http://localhost:{HttpPort}");

        ContainerId = runProc.StandardOutput.ReadToEnd().Trim();

        httpContext.HttpClient = new HttpClient
        {
            BaseAddress = host
        };
    }

    [AfterTestRun]
    public static void StopContainer()
    {
        if (!Enable)
        {
            return;
        }

        if (bool.TryParse(Environment.GetEnvironmentVariable("INTEGRATION_TESTS_WRITE_SERVER_LOGS"), out var enableLogs) && enableLogs)
        {
            var logsProc = new Process();
            logsProc.StartInfo.FileName = ContainerEngine;
            logsProc.StartInfo.Arguments = $"logs {ContainerId}";
            logsProc.StartInfo.CreateNoWindow = true;
            logsProc.StartInfo.RedirectStandardOutput = true;
            logsProc.Start();
            logsProc.WaitForExit();
            logsProc.ExitCode.Should().Be(0);

            var timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            using var logFile = File.Open(Path.Combine("../../../..", $"log-integration-tests-server-{timestamp}.txt"), FileMode.OpenOrCreate);

            logsProc.StandardOutput.BaseStream.CopyTo(logFile);
        }

        var stopProc = new Process();
        stopProc.StartInfo.FileName = ContainerEngine;
        stopProc.StartInfo.Arguments = $"stop {ContainerId}";
        stopProc.StartInfo.CreateNoWindow = true;
        stopProc.Start();
        stopProc.WaitForExit();
        stopProc.ExitCode.Should().Be(0);
    }

    private static string ChooseContainerEngine()
    {
        var knownEngines = new List<string> { "podman", "docker", "buildah" };

        foreach (var engine in knownEngines)
        {
            if (CommandExists(engine))
            {
                return engine;
            }
        }

        throw new InvalidOperationException($"Could not find a valid container engine. Known engines: [{string.Join(", ", knownEngines)}]");
    }

    private static bool CommandExists(string command)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        foreach (var entry in path.Split(Path.PathSeparator))
        {
            if (File.Exists(Path.Combine(entry, command)))
            {
                return true;
            }

            if (OperatingSystem.IsWindows() && !command.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) && File.Exists(Path.Combine(entry, command)))
            {
                return true;
            }
        }

        return false;
    }
}
