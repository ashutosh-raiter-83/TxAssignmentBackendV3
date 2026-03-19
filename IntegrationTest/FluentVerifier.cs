using FluentAssertions.Execution;

namespace IntegrationTest;

/// <summary>
/// https://www.craigwardman.com/blog/using-fluent-assertions-inside-of-a-moq-verify
/// </summary>
public static class FluentVerifier
{
    public static bool VerifyFluentAssertion(Action assertion)
    {
        using (var assertionScope = new AssertionScope())
        {
            assertion();

            return !assertionScope.Discard().Any();
        }
    }

    public static async Task<bool> VerifyFluentAssertion(Func<Task> assertion)
    {
        using (var assertionScope = new AssertionScope())
        {
            await assertion();

            return !assertionScope.Discard().Any();
        }
    }
}
