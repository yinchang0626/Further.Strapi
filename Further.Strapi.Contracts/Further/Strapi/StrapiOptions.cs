using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Further.Strapi;

public class StrapiOptions
{
    public static readonly string HttpClientName = "StrapiClient";
    public string StrapiUrl { get; set; } = null!;

    public string StrapiToken { get; set; } = null!;
}
