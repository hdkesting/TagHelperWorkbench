using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TagHelperWorkbench.TagHelpers
{
    /// <summary>
    /// Optionally remove the parent from around a block of html.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Razor.TagHelpers.TagHelper" />
    [HtmlTargetElement(Attributes = "hide")]
    public class HideParentTagHelper : TagHelper
    {
        public override int Order => -99;

        // Pascal case gets translated into lower-kebab-case.
        public bool Hide { get; set; }

        /// <summary>
        /// Asynchronously executes the <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper" /> with the given <paramref name="context" /> and
        /// <paramref name="output" />.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" /> that on completion updates the <paramref name="output" />.
        /// </returns>
        /// <remarks>
        /// By default this calls into <see cref="M:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper.Process(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext,Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput)" />.
        /// </remarks>
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.RemoveAll(nameof(Hide));

            if (Hide)
            {
                // replace the tagname
                output.TagName = "span";
                foreach (var attr in context.AllAttributes)
                {
                    output.Attributes.Remove(attr);
                }

                //var children = await output.GetChildContentAsync(true);
                //output.Content.SetHtmlContent("<!-- redacted -->");

                //output.PostContent.SetHtmlContent(children.GetContent());
            }

            return Task.CompletedTask;
        }
    }
}
