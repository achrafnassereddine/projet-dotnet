using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace DevSecOpsSaaS.SecurityAuditTest;

[ApiController]
[Route("api/audit-probe")]
public class AuditProbeController : ControllerBase
{
    // GITLEAKS TRIGGER: High-entropy AWS Access Key ID pattern (dummy/non-existent key)
    private const string DummyAwsAccessKey = "AKIAIOSFODNN7EXAMPLE";
    private const string DummyAwsSecretKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY";

    // CODEQL TRIGGER (cs/weak-crypto): Broken MD5 hash algorithm
    [HttpGet("hash")]
    public string ComputeLegacyHash(string input)
    {
        using var md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash);
    }

    // CODEQL TRIGGER (cs/path-injection): Unsanitized user-controlled file path read
    [HttpGet("read")]
    public IActionResult ReadAuditLog([FromQuery] string fileName)
    {
        string content = System.IO.File.ReadAllText("/var/logs/" + fileName);
        return Ok(content);
    }
}