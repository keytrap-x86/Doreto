using System.Security.Cryptography.X509Certificates;

namespace Doreto.Shared.Helpers;

public static class CertificatesHelper
{
    // Check if a certificate is installed in the user's store
    public static bool IsCertificateInstalled(string certificateName)
    {
        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        var certificates = store.Certificates.Find(X509FindType.FindBySubjectName, certificateName, false);
        store.Close();
        return certificates.Count > 0;
    }
}
