using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Restarter
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class RestartCommand
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0100;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = new Guid("5123368a-ca20-4630-b2c2-de9dca3621fb");

		/// <summary>
		/// VS Package that provides this command, not null.
		/// </summary>
		private readonly AsyncPackage _package;

		/// <summary>
		/// Initializes a new instance of the <see cref="RestartCommand"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		/// <param name="commandService">Command service to add command to, not null.</param>
		private RestartCommand(AsyncPackage package, OleMenuCommandService commandService)
		{
			_package = package ?? throw new ArgumentNullException(nameof(package));

			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(Restart, menuCommandID);
			commandService.AddCommand(menuItem);
		}

		private void Restart(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var service = _package.GetService<SVsShell, IVsShell4>();
			service.Restart(0);
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static RestartCommand Instance { get; private set; }

		/// <summary>
		/// Gets the service provider from the owner package.
		/// </summary>
		private IAsyncServiceProvider ServiceProvider => _package;

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static async Task InitializeAsync(AsyncPackage package)
		{
			// Switch to the main thread - the call to AddCommand in RestartCommand's constructor requires
			// the UI thread.
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

			OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			Instance = new RestartCommand(package, commandService);
		}
	}
}
