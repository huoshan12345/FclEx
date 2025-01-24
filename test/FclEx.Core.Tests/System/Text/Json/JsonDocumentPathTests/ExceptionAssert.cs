namespace System.Text.Json.JsonDocumentPathTests;

public static class ExceptionAssert
{
    public static TException? Throws<TException>(Action action, params string[]? possibleMessages) where TException : Exception
    {
        try
        {
            action();
            //Assert.Throws<TException>(action);
            //Assert.Fail("Exception of type {0} expected. No exception thrown.", typeof(TException).Name);
            return null;
        }
        catch (TException ex)
        {
            if (possibleMessages == null || possibleMessages.Length == 0)
            {
                return ex;
            }
            foreach (var possibleMessage in possibleMessages)
            {
                if (StringAssert.Equals(possibleMessage, ex.Message))
                {
                    return ex;
                }
            }

            throw new Exception("Unexpected exception message." + Environment.NewLine + "Expected one of: " + string.Join(Environment.NewLine, possibleMessages) + Environment.NewLine + "Got: " + ex.Message + Environment.NewLine + Environment.NewLine + ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception of type {typeof(TException).Name} expected; got exception of type {ex.GetType().Name}.", ex);
        }
    }
}