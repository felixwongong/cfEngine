using cfEngine.Util;

namespace cfEngine.Service.Auth
{
    public partial class AuthService
    {
        public class Builder
        {
            private IAuthService _authService;
            
            public Builder SetService(IAuthService authService)
            {
                _authService = authService;
                return this;
            }
            
            public Builder RegisterPlatform(PlatformAuth platform)
            {
                SanityCheck.WhenNull(_authService, $"AuthService is not set, call {nameof(SetService)} first");
                _authService.RegisterPlatform(platform);
                return this;
            }

            public IAuthService Build()
            {
                return _authService;
            }
        }
    }
}