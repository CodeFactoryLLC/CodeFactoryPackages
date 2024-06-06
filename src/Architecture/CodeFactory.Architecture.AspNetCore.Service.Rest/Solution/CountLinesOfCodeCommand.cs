using CodeFactory.Automation.Standard.Logic.Extensions;
using CodeFactory.WinVs;
using CodeFactory.WinVs.Commands;
using CodeFactory.WinVs.Commands.SolutionExplorer;
using CodeFactory.WinVs.Logging;
using CodeFactory.WinVs.Models.ProjectSystem;
using Drives.Automation.Standards.Logic.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Drives.Architecture.Commands.Solution
{
	/// <summary>
	/// Code factory command for automation of a C# document when selected from a project in solution explorer.
	/// </summary>
	public class CountLinesOfCodeCommand : SolutionCommandBase
	{
		private static readonly string commandTitle = "Count Lines of Code";
		private static readonly string commandDescription = "Counds the lines of code within in this Project.";
		public IVsActions _vsActions { get; set; }
#pragma warning disable CS1998

		/// <inheritdoc />
		public CountLinesOfCodeCommand(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
		{
			//Intentionally blank
			_vsActions = vsActions;
		}

		#region Overrides of VsCommandBase<IVsCSharpDocument>

		#region External Configuration

		/// <summary>
		/// The fully qualified name of the command to be used with configuration.
		/// </summary>
		public static string Type = typeof(CountLinesOfCodeCommand).FullName;

		/// <summary>
		/// Loads the external configuration definition for this command.
		/// </summary>
		/// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
		public override ConfigCommand LoadExternalConfigDefinition()
		{
			var config = new ConfigCommand
			{
				CommandType = Type,
				Category = "Code Analysis",
				Name = nameof(CountLinesOfCodeCommand),
				Guidance = "Automation command that calculates a total number of lines included in this Sollution."
			};

			return config;
		}
		#endregion

		/// <summary>
		/// Validation logic that will determine if this command should be enabled for execution.
		/// </summary>
		/// <param name="result">The target model data that will be used to determine if this command should be enabled.</param>
		/// <returns>Boolean flag that will tell code factory to enable this command or disable it.</returns>
		public override async Task<bool> EnableCommandAsync(VsSolution result)
		{
			//Result that determines if the command is enabled and visible in the context menu for execution.
			bool isEnabled = false;

			try
			{
				var projects = await result.GetProjectsAsync(true);
				foreach (var project in projects)
				{
					var children = await project.GetChildrenAsync(true);
					if (children.Any(p => p.ModelType.Equals(VisualStudioModelType.Document) && (p.IsMarkupCode() || p.IsSourceCode())))
					{
						isEnabled = true;
						break;
					}
				}
			}
			catch (Exception unhandledError)
			{
				_logger.Error($"The following unhandled error occurred while checking if the solution explorer C# document command {commandTitle} is enabled. ",
					unhandledError);
				isEnabled = false;
			}

			return isEnabled;
		}

		/// <summary>
		/// Code factory framework calls this method when the command has been executed. 
		/// </summary>
		/// <param name="result">The code factory model that has generated and provided to the command to process.</param>
		public override async Task ExecuteCommandAsync(VsSolution result)
		{
			try
			{
				// Prompt Dialog for user input.				
				MessageBox.Show($"There are {await result.CountLinesOfCodeAsync()} lines of code in the {result.Name} solution.");
			}
			catch (Exception unhandledError)
			{
				_logger.Error($"The following unhandled error occurred while executing the solution explorer C# document command {commandTitle}. ",
					unhandledError);
			}
		}
		#endregion
	}
}