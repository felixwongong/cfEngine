using cfEngine.Command;
using cfEngine.Logging;

namespace cfEngine.Core
{
    public partial class UserDataManager
    {
        public class DeleteSaveCommand : ICommandHandler
        {
            public void Execute(Parameters @param)
            {
                var getUserData = Domain.Current.Get<UserDataManager>();

                if (getUserData.HasError(out var error))
                {
                    Log.LogException(error);
                    return;
                }
                
                getUserData.value.DeleteSave();
            }
        }
    }
}