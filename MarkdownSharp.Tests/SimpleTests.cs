using MarkdownSharp;
using NUnit.Framework;

namespace MarkdownSharp.Tests
{
    [TestFixture]
    public class SimpleTests : BaseTest
    {
        private Markdown m = new Markdown();

        [Test]
        public void Bold()
        {
            string input = "This is **bold**. This is also __bold__.";
            string expected = "<p>This is <strong>bold</strong>. This is also <strong>bold</strong>.</p>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Italic()
        {
            string input = "This is *italic*. This is also _italic_.";
            string expected = "<p>This is <em>italic</em>. This is also <em>italic</em>.</p>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Link()
        {
            string input = "This is [a link][1].\n\n  [1]: http://www.example.com";
            string expected = "<p>This is <a href=\"http://www.example.com\">a link</a>.</p>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void LinkBracket()
        {
            string input = "Have you visited <http://www.example.com> before?";
            string expected = "<p>Have you visited <a href=\"http://www.example.com\">http://www.example.com</a> before?</p>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void LinkBare_withoutAutoHyperLink()
        {
            string input = "Have you visited http://www.example.com before?";
            string expected = "<p>Have you visited http://www.example.com before?</p>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        /*
        [Test]
        public void LinkBare_withAutoHyperLink()
        {
            //TODO: implement some way of setting AutoHyperLink programmatically
            //to run this test now, just change the _autoHyperlink constant in Markdown.cs
            string input = "Have you visited http://www.example.com before?";
            string expected = "<p>Have you visited <a href=\"http://www.example.com\">http://www.example.com</a> before?</p>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }*/

        [Test]
        public void LinkAlt()
        {
            string input = "Have you visited [example](http://www.example.com) before?";
            string expected = "<p>Have you visited <a href=\"http://www.example.com\">example</a> before?</p>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Image()
        {
            string input = "An image goes here: ![alt text][1]\n\n  [1]: http://www.google.com/intl/en_ALL/images/logo.gif";
            string expected = "<p>An image goes here: <img src=\"http://www.google.com/intl/en_ALL/images/logo.gif\" alt=\"alt text\" /></p>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Blockquote()
        {
            string input = "Here is a quote\n\n> Sample blockquote\n";
            string expected = "<p>Here is a quote</p>\n\n<blockquote>\n  <p>Sample blockquote</p>\n</blockquote>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NumberList()
        {
            string input = "A numbered list:\n\n1. a\n2. b\n3. c\n";
            string expected = "<p>A numbered list:</p>\n\n<ol>\n<li>a</li>\n<li>b</li>\n<li>c</li>\n</ol>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void BulletList()
        {
            string input = "A bulleted list:\n\n- a\n- b\n- c\n";
            string expected = "<p>A bulleted list:</p>\n\n<ul>\n<li>a</li>\n<li>b</li>\n<li>c</li>\n</ul>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Header1()
        {
            string input = "#Header 1\nHeader 1\n========";
            string expected = "<h1>Header 1</h1>\n\n<h1>Header 1</h1>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Header2()
        {
            string input = "##Header 2\nHeader 2\n--------";
            string expected = "<h2>Header 2</h2>\n\n<h2>Header 2</h2>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CodeBlock()
        {
            string input = "code sample:\n\n    <head>\n    <title>page title</title>\n    </head>\n";
            string expected = "<p>code sample:</p>\n\n<pre><code>&lt;head&gt;\n&lt;title&gt;page title&lt;/title&gt;\n&lt;/head&gt;\n</code></pre>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CodeSpan()
        {
            string input = "HTML contains the `<blink>` tag";
            string expected = "<p>HTML contains the <code>&lt;blink&gt;</code> tag</p>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void HtmlPassthrough()
        {
            string input = "<div>\nHello World!\n</div>\n";
            string expected = "<div>\nHello World!\n</div>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Escaping()
        {
            string input = @"\`foo\`";
            string expected = "<p>`foo`</p>\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void HorizontalRule()
        {
            string input = "* * *\n\n***\n\n*****\n\n- - -\n\n---------------------------------------\n\n";
            string expected = "<hr />\n\n<hr />\n\n<hr />\n\n<hr />\n\n<hr />\n";

            string actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }
    	[Test]
    	public void SimpleTable() {
			const string input = @"
|First Header  | Second Header|
|------------- | -------------|
|Content Cell  | Content Cell|
|Content Cell  | Content Cell|
";
    		const string expected = "<table>\n<thead>\n<tr><th>First Header</th><th>Second Header</th></tr>\n</thead>\n"+
"<tbody>\n<tr><td>Content Cell</td><td>Content Cell</td></tr>\n<tr><td>Content Cell</td><td>Content Cell</td></tr>\n</tbody>\n</table>\n";
			var actual = m.Transform(input);

			Assert.AreEqual(expected, actual);
		}

        [Test]
        public void SimpleTableWithNoWrap() {
            const string input = @"
|First Header  | Second Header|
| ++++++++++++ | -------------|
|Content Cell  | Content Cell|
|Content Cell  | Content Cell|
";
            const string expected = "<table>\n<thead>\n<tr><th style=\"white-space:nowrap;\">First Header</th><th>Second Header</th></tr>\n</thead>\n" +
"<tbody>\n<tr><td style=\"white-space:nowrap;\">Content Cell</td><td>Content Cell</td></tr>\n<tr><td style=\"white-space:nowrap;\">Content Cell</td><td>Content Cell</td></tr>\n</tbody>\n</table>\n";
            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void SimpleTableWithNoWrap2() {
            const string input = @"
|First Header  | Second Header|
| ++++++++++++ | ++++++++++++ |
|Content Cell  | Content Cell|
|Content Cell  | Content Cell|
";
            const string expected = "<table>\n<thead>\n<tr><th style=\"white-space:nowrap;\">First Header</th><th style=\"white-space:nowrap;\">Second Header</th></tr>\n</thead>\n" +
"<tbody>\n<tr><td style=\"white-space:nowrap;\">Content Cell</td><td style=\"white-space:nowrap;\">Content Cell</td></tr>\n<tr><td style=\"white-space:nowrap;\">Content Cell</td><td style=\"white-space:nowrap;\">Content Cell</td></tr>\n</tbody>\n</table>\n";
            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }
        [Test]
		public void SimplestTable() {
			const string input = @"
First Header  | Second Header
------------- | -------------
Content Cell  | Content Cell
Content Cell  | Content Cell
";
			const string expected = "<table>\n<thead>\n<tr><th>First Header</th><th>Second Header</th></tr>\n</thead>\n" +
"<tbody>\n<tr><td>Content Cell</td><td>Content Cell</td></tr>\n<tr><td>Content Cell</td><td>Content Cell</td></tr>\n</tbody>\n</table>\n";
			var actual = m.Transform(input);

			Assert.AreEqual(expected, actual);
		}
		[Test]
		public void SimpleTableWithEscapes() {
			const string input = @"|My\|Name | My\\Backslash\\|
|------------- | -------------|
|Content Cell  | Content Cell|
|Content Cell  | Content Cell|
";
			const string expected = "<table>\n<thead>\n<tr><th>My|Name</th><th>My\\Backslash\\</th></tr>\n</thead>\n" +
"<tbody>\n<tr><td>Content Cell</td><td>Content Cell</td></tr>\n<tr><td>Content Cell</td><td>Content Cell</td></tr>\n</tbody>\n</table>\n";

			var actual = m.Transform(input);

			Assert.AreEqual(expected, actual);
		}


        [Test]
        public void MultiRowTable() {
            const string input = @"| Role			| Description															| ImageVault component
|--------------	|---------------------------------------------------------------------	|-----------------------------
| Resource		| An entity capable of granting access to a protected resource.			| User using ImageVault 
| owner			| When the resource owner is a person, it is referred to as an			|
|				| end-user.																|
|--------------	|---------------------------------------------------------------------	|-----------------------------
| Resource		| The server hosting the protected resources, capable of accepting		| ImageVault Core
| server		| and responding to protected resource requests using access tokens.	|
|--------------	|---------------------------------------------------------------------	|-----------------------------
| Client		| An application making protected resource requests on behalf of the	| ImageVault Ui ImageVault
|				| resource owner and with its authorization.  The term ""client"" does	| ImageVault Plugin
|				| not imply any particular implementation characteristics (e.g.,		| ImageVault EPiServer 7 Add-on
|				| whether the application executes on a server, a desktop, or other		| Any 3rd part application
|				| devices).																|
|--------------	|---------------------------------------------------------------------	|-----------------------------
| Authorization	| The server issuing access tokens to the client after successfully		| ImageVault Core
| server		| authenticating the resource owner and obtaining authorization.		|
|--------------	|---------------------------------------------------------------------	|-----------------------------
";
            string expected = @"<table>
<thead>
<tr><th>Role</th><th>Description</th><th>ImageVault component</th></tr>
</thead>
<tbody>
<tr><td>Resource owner</td><td>An entity capable of granting access to a protected resource. When the resource owner is a person, it is referred to as an end-user.</td><td>User using ImageVault</td></tr>
<tr><td>Resource server</td><td>The server hosting the protected resources, capable of accepting and responding to protected resource requests using access tokens.</td><td>ImageVault Core</td></tr>
<tr><td>Client</td><td>An application making protected resource requests on behalf of the resource owner and with its authorization.  The term ""client"" does not imply any particular implementation characteristics (e.g., whether the application executes on a server, a desktop, or other devices).</td><td>ImageVault Ui ImageVault ImageVault Plugin ImageVault EPiServer 7 Add-on Any 3rd part application</td></tr>
<tr><td>Authorization server</td><td>The server issuing access tokens to the client after successfully authenticating the resource owner and obtaining authorization.</td><td>ImageVault Core</td></tr>
</tbody>
</table>
";
            expected = expected.Replace("\r", "");
            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void MultiRowTable2() {
            const string input = @"Parameter Name	| Description
---------------	|-----------------------------------------------------------------------------------------------------------------------------
response\_type	| Defines the type of grant to be requested. Must be set to ""code"".
---------------	|-----------------------------------------------------------------------------------------------------------------------------
client\_id      | The id of the client application that requests access to ImageVault ([See OAuth Clients](oauth-clients.html))
---------------	|-----------------------------------------------------------------------------------------------------------------------------
redirect\_uri   | \[Optional] The uri where the response should be redirected. If omitted, the clients registered redirect\_uri will be used.
---------------	|-----------------------------------------------------------------------------------------------------------------------------
state           | RECOMMENDED.  An opaque value used by the client to maintain state between the request and callback.  The authorization
                | server includes this value when redirecting the user-agent back to the client.  The parameter SHOULD be used for preventing
                | cross-site request forgery as described in [Section 10.12](https://tools.ietf.org/html/rfc6749#section-10.12).
---------------	|-----------------------------------------------------------------------------------------------------------------------------
";
            string expected = @"<table>
<thead>
<tr><th>Parameter Name</th><th>Description</th></tr>
</thead>
<tbody>
<tr><td>response_type</td><td>Defines the type of grant to be requested. Must be set to ""code"".</td></tr>
<tr><td>client_id</td><td>The id of the client application that requests access to ImageVault (<a href=""oauth-clients.html"">See OAuth Clients</a>)</td></tr>
<tr><td>redirect_uri</td><td>[Optional] The uri where the response should be redirected. If omitted, the clients registered redirect_uri will be used.</td></tr>
<tr><td>state</td><td>RECOMMENDED.  An opaque value used by the client to maintain state between the request and callback.  The authorization server includes this value when redirecting the user-agent back to the client.  The parameter SHOULD be used for preventing cross-site request forgery as described in <a href=""https://tools.ietf.org/html/rfc6749#section-10.12"">Section 10.12</a>.</td></tr>
</tbody>
</table>
";
            expected = expected.Replace("\r", "");
            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }
	}
}
