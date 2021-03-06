﻿using System.ComponentModel;
using Inedo.Extensions.Credentials;
using System.Security;
using Inedo.Documentation;
using Inedo.Extensions.Clients;
using Inedo.Extensions.Clients.CommandLine;
using Inedo.Agents;
using Inedo.Diagnostics;
using Inedo.Extensions.Clients.LibGitSharp.Remote;

#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Credentials;
using Inedo.BuildMaster.Extensibility.Operations;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Credentials;
using Inedo.Otter.Extensibility.Operations;
#endif

namespace Inedo.Extensions.Operations
{
    public abstract class GitOperation : ExecuteOperation
    {
        public abstract string CredentialName { get; set; }

        [Category("Connection/Identity")]
        [ScriptAlias("UserName")]
        [DisplayName("User name")]
        [PlaceholderText("Use user name from credentials")]
        [MappedCredential(nameof(GitCredentials.UserName))]
        public string UserName { get; set; }

        [Category("Connection/Identity")]
        [ScriptAlias("Password")]
        [DisplayName("Password")]
        [PlaceholderText("Use password from credentials")]
        [MappedCredential(nameof(GitCredentials.Password))]
        public SecureString Password { get; set; }

        [Category("Advanced")]
        [ScriptAlias("GitExePath")]
        [DisplayName("Git executable path")]
        [DefaultValue("$DefaultGitExePath")]
        public string GitExePath { get; set; }

        [Category("Advanced")]
        [ScriptAlias("RecurseSubmodules")]
        [DisplayName("Recurse submodules")]
        public bool RecurseSubmodules { get; set; }

        [Category("Advanced")]
        [ScriptAlias("WorkspaceDiskPath")]
        [DisplayName("Workspace disk path")]
        [PlaceholderText("Automatically managed")]
        [Description("If not set, a workspace name will be automatically generated and persisted based on the Repository URL or other host-specific information (e.g. GitHub's repository name).")]
        public string WorkspaceDiskPath { get; set; }

        [Category("Advanced")]
        [ScriptAlias("CleanWorkspace")]
        [DisplayName("Clean workspace")]
        [Description("If set to true, the workspace directory will be cleared before any Git-based operations are performed.")]
        public bool CleanWorkspace { get; set; }

        protected GitClient CreateClient(IOperationExecutionContext context, string repositoryUrl, WorkspacePath workspacePath)
        {
            if (!string.IsNullOrEmpty(this.GitExePath))
            {
                this.LogDebug($"Executable path specified, using Git command line client at '{this.GitExePath}'...");
                return new GitCommandLineClient(
                    this.GitExePath,
                    context.Agent.GetService<IRemoteProcessExecuter>(),
                    context.Agent.GetService<IFileOperationsExecuter>(),
                    new GitRepositoryInfo(workspacePath, repositoryUrl, this.UserName, this.Password),
                    this,
                    context.CancellationToken
                );
            }
            else
            {
                this.LogDebug("No executable path specified, using built-in Git library...");
                return new RemoteLibGitSharpClient(
                    context.Agent.TryGetService<IRemoteJobExecuter>(),
                    context.WorkingDirectory,
                    context.Simulation,
                    context.CancellationToken,
                    new GitRepositoryInfo(workspacePath, repositoryUrl, this.UserName, this.Password),
                    this
                );
            }
        }
    }
}
