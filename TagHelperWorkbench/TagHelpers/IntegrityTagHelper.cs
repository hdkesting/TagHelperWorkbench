﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.IO;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace TagHelperWorkbench.TagHelpers
{
    /// <summary>
    /// Implement https://developer.mozilla.org/nl/docs/Web/Security/Subresource_Integrity.
    /// Change attribute 'integrity' or 'integrity="sha265"' to a full integrity value.
    /// </summary>
    /// <remarks>
    /// Required in Startup.ConfigureServices:
    /// <code>
    ///        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    ///        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
    /// </code>
    /// 
    /// Usage: add integrity attibute to &lt:link href=..&gt; or &lt;script src=..&gt;.
    /// Either with an empty value (defaults to sha256) or one of sha256, sha384 or sha512. A full integrity attribute is ignored.
    /// Works only on local files!
    /// </remarks>
    [HtmlTargetElement("script", Attributes = "integrity")]
    [HtmlTargetElement("link", Attributes = "integrity")]
    public class IntegrityTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory urlHelperFactory;
        private readonly IActionContextAccessor actionContextAccessor;
        private readonly IHostingEnvironment environment;

        public IntegrityTagHelper(
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IHostingEnvironment environment)
        {
            // https://stackoverflow.com/questions/40001242/aspnetcore-get-path-to-wwwroot-in-taghelper

            this.urlHelperFactory = urlHelperFactory;
            this.actionContextAccessor = actionContextAccessor;
            this.environment = environment;
        }

        /// <summary>
        /// Gets or sets the value of the integrity attribute.
        /// </summary>
        /// <value>
        /// The integrity.
        /// </value>
        public string Integrity { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(this.Integrity))
            {
                // default value (attribute without value)
                this.Integrity = "sha256";
            }

            if (this.Integrity.Length > 6)
            {
                // assume already filled if longer than 6, so ignore
                return;
            }

            HashAlgorithm csp = null;
            this.Integrity = this.Integrity.ToLowerInvariant();
            switch (this.Integrity)
            {
                case "sha256":
                    csp = SHA256.Create();
                    break;

                case "sha384":
                    csp = SHA384.Create();
                    break;

                case "sha512":
                    csp = SHA512.Create();
                    break;
            }

            if (csp == null)
            {
                // unknown hash, just ignore
                return;
            }

            IUrlHelper urlHelper = this.urlHelperFactory.GetUrlHelper(this.actionContextAccessor.ActionContext);

            var source = (context.AllAttributes["src"] ?? context.AllAttributes["href"]).Value.ToString();

            output.Attributes.SetAttribute("integrity", string.Join(" ", this.GetHashes(source, csp, this.Integrity, urlHelper)));
        }

        private IEnumerable<string> GetHashes(string source, HashAlgorithm csp, string algorithm, IUrlHelper urlHelper)
        {
            // possible enhancement: cache the hashes (should I then handle changes to the files or leave that for a restart?)

            foreach (var path in this.GetFiles(source))
            {
                var fullPath = Path.Combine(this.environment.WebRootPath, urlHelper.Content(path));
                if (File.Exists(fullPath))
                {
                    var data = File.ReadAllBytes(fullPath);
                    var hash = csp.ComputeHash(data);
                    var b64 = Convert.ToBase64String(hash);
                    yield return algorithm + "-" + b64;
                }
            }
        }

        private IEnumerable<string> GetFiles(string source)
        {
            if (source.StartsWith("~"))
            {
                source = source.Substring(1);
            }

            if (source.StartsWith("/"))
            {
                source = source.Substring(1);
            }

            yield return source;
            /*
            if (source.Contains(".min."))
            {
                source = source.Replace(".min.", ".");
            }
            else
            {
                // met en zonder ".min."
                var ext = System.IO.Path.GetExtension(source);
                source = source.Replace(ext, ".min" + ext);
            }

            yield return source;
            */
        }
    }
}