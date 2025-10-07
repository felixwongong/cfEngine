using cfEngine.Command;

namespace cfEngine.Core
{
    public partial class UserDataManager
    {
        public class DeleteSaveCommand : ICommandHandler
        {
            public void Execute(Parameters @param)
            {
                Domain.Current.Get<UserDataManager>().DeleteSave();
            }
        }
    }
}