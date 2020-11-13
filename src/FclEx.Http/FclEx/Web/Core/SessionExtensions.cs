namespace FclEx.Web.Core
{
    public static class SessionExtensions
    {
        public static bool IsCaptchaRequired(this ISession session)
        {
            return session.State == SessionState.CaptchaRequired;
        }

        public static bool IsLogining(this ISession session)
        {
            return session.State == SessionState.Logining;
        }

        public static void Offline(this ISession session)
        {
            session.State = SessionState.Offline;
        }

        public static void Online(this ISession session)
        {
            session.State = SessionState.Online;
        }
    }
}
