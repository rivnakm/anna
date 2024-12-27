using FluentAssertions;

namespace Anna.Common.Test;

public class EnvironmentUtilityTest
{
    [Fact]
    public void TestGetRequiredEnvironmentVariable()
    {
        const string key = "ENV_VARIABLE_KEY";
        const string value = "value";
        Environment.SetEnvironmentVariable(key, value);

        var actual = EnvironmentUtility.GetRequiredEnvironmentVariable(key);

        actual.Should().Be(value);
    }

    [Fact]
    public void TestGetRequiredEnvironmentVariable_Unset_Throws()
    {
        const string key = "ENV_VARIABLE_KEY";

        var get = () => EnvironmentUtility.GetRequiredEnvironmentVariable(key);

        get.Should().Throw<InvalidOperationException>();
    }
}
