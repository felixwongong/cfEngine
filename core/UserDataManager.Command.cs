using cfEngine.Command;

namespace cfEngine.Core
{
    public partial class UserDataManager
    {
        public class DeleteSaveCommand : ICommandHandler
        {
            public bool Execute(Parameters @param)
            {
                var getUserData = Domain.Current.Get<UserDataManager>();

                if (getUserData.HasError(out var error))
                {
                    Log.LogException(error);
                    return false;
                }
                
                getUserData.value.DeleteSave();
                return true;
            }
        }
    }
}