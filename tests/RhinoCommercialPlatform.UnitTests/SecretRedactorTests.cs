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

    [TestMethod]
    public void Redact_RemovesAuthorizationBearerToken()
    {
        var result = SecretRedactor.Redact("Authorization: Bearer abc.def.123");

        Assert.IsTrue(result.Contains("Authorization:"));
        Assert.IsTrue(result.Contains("[REDACTED]"));
        Assert.IsFalse(result.Contains("Bearer"));
        Assert.IsFalse(result.Contains("abc.def.123"));
    }

    [TestMethod]
    public void Redact_RemovesAuthorizationBasicToken()
    {
        var result = SecretRedactor.Redact("Authorization: Basic dXNlcjpwYXNz");

        Assert.IsTrue(result.Contains("Authorization:"));
        Assert.IsTrue(result.Contains("[REDACTED]"));
        Assert.IsFalse(result.Contains("Basic"));
        Assert.IsFalse(result.Contains("dXNlcjpwYXNz"));
    }

    [TestMethod]
    public void Redact_RemovesQuotedJsonToken()
    {
        var result = SecretRedactor.Redact("{\"access_token\":\"secret json value\"}");

        Assert.IsFalse(result.Contains("secret json value"));
        Assert.IsTrue(result.Contains("access_token"));
        Assert.IsTrue(result.Contains("[REDACTED]"));
    }

    [TestMethod]
    public void Redact_RemovesQuotedPasswordWithSpaces()
    {
        var result = SecretRedactor.Redact("password=\"my secret pass phrase\"");

        Assert.IsFalse(result.Contains("my secret pass phrase"));
        Assert.IsTrue(result.Contains("password"));
        Assert.IsTrue(result.Contains("[REDACTED]"));
    }

    [TestMethod]
    public void Redact_PreservesTextAfterUnquotedSecret()
    {
        var result = SecretRedactor.Redact("User token=secret-value logged in.");

        Assert.IsFalse(result.Contains("secret-value"));
        Assert.IsTrue(result.Contains("User"));
        Assert.IsTrue(result.Contains("logged in."));
        Assert.IsTrue(result.Contains("[REDACTED]"));
    }

    [TestMethod]
    public void Redact_DoesNotMatchTokenInsideLargerWord()
    {
        var result = SecretRedactor.Redact("This is a tokenization example.");

        Assert.AreEqual("This is a tokenization example.", result);
    }
}
