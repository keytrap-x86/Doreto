using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doreto.Shared.Models.Proxy;

/// <summary>
///     Represents the user login info that is needed to login to the server.
/// </summary>
/// <param name="CertId"></param>
/// <param name="CertHash"></param>
/// <param name="ApiKey"></param>
public record AutoLoginInfo(string CertId, string CertHash, string ApiKey);
