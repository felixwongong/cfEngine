using System.Collections.Generic;
using cfEngine.Command;

namespace cfEngine.Core
{
	public partial class UserDataManager
	{
		[CommandService.RegisterOnInitialized(nameof(Register))]
		public struct DeleteSaveCommand : ICommand
		{
			public static void Register() => CommandService.RegisterCommand(new DeleteSaveCommand(), "userdata", "delete");
			public void Execute(IReadOnlyDictionary<string, string> args)
			{
				Domain.Current.GetUserData().DeleteSave();
			}
		}
	}
}
