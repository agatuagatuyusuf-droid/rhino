using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhinoCommercialPlatform.Infrastructure;

namespace RhinoCommercialPlatform.UnitTests;

[TestClass]
public class SecretRedactorTests
{
    [TestMethod]
    public void Redact_RemovesTokenAndPasswordValues()
    {
        var result = SecretRedactor.Redact("token=secret123 password=mypass");
        Assert.IsFalse(result.Contains("secret123"));
        Assert.IsFalse(result.Contains("mypass"));
        Assert.IsTrue(result.Contains("[REDACTED]"));
    }

    [TestMethod]
    public void Redact_IsCaseInsensitive()
    {
        var lower = SecretRedactor.Redact("token=abc");
        var upper = SecretRedactor.Redact("TOKEN=abc");
        var mixed = SecretRedactor.Redact("Token=abc");

        Assert.IsFalse(lower.Contains("abc"));
        Assert.IsFalse(upper.Contains("abc"));
        Assert.IsFalse(mixed.Contains("abc"));
    }

    [TestMethod]
    public void Redact_DoesNotRemoveNormalMessage()
    {
        var message = "This is a normal log message without secrets.";
        var result = SecretRedactor.Redact(message);
        Assert.AreEqual(message, result);
    }
}
