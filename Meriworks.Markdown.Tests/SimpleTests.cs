using NUnit.Framework;

namespace Meriworks.Markdown.Tests
{
    [TestFixture]
    public class SimpleTests : BaseTest
    {
        private MarkdownParser m = new MarkdownParser();

        [Test]
        public void Bold()
        {
            var input = "This is **bold**. This is also __bold__.";
            var expected = "<p>This is <strong>bold</strong>. This is also <strong>bold</strong>.</p>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Italic()
        {
            var input = "This is *italic*. This is also _italic_.";
            var expected = "<p>This is <em>italic</em>. This is also <em>italic</em>.</p>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Link()
        {
            var input = "This is [a link][1].\n\n  [1]: http://www.example.com";
            var expected = "<p>This is <a href=\"http://www.example.com\">a link</a>.</p>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void LinkBracket()
        {
            var input = "Have you visited <http://www.example.com> before?";
            var expected = "<p>Have you visited <a href=\"http://www.example.com\">http://www.example.com</a> before?</p>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void LinkBare_withoutAutoHyperLink()
        {
            var input = "Have you visited http://www.example.com before?";
            var expected = "<p>Have you visited http://www.example.com before?</p>\n";

            var actual = m.Transform(input);

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
            var input = "Have you visited [example](http://www.example.com) before?";
            var expected = "<p>Have you visited <a href=\"http://www.example.com\">example</a> before?</p>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Image()
        {
            var input = "An image goes here: ![alt text][1]\n\n  [1]: http://www.google.com/intl/en_ALL/images/logo.gif";
            var expected = "<p>An image goes here: <img src=\"http://www.google.com/intl/en_ALL/images/logo.gif\" alt=\"alt text\" /></p>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Blockquote()
        {
            var input = "Here is a quote\n\n> Sample blockquote\n";
            var expected = "<p>Here is a quote</p>\n\n<blockquote>\n  <p>Sample blockquote</p>\n</blockquote>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NumberList()
        {
            var input = "A numbered list:\n\n1. a\n2. b\n3. c\n";
            var expected = "<p>A numbered list:</p>\n\n<ol>\n<li>a</li>\n<li>b</li>\n<li>c</li>\n</ol>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void BulletList()
        {
            var input = "A bulleted list:\n\n- a\n- b\n- c\n";
            var expected = "<p>A bulleted list:</p>\n\n<ul>\n<li>a</li>\n<li>b</li>\n<li>c</li>\n</ul>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Header1()
        {
            var input = "#Header 1\nHeader 1\n========";
            var expected = "<h1>Header 1</h1>\n\n<h1>Header 1</h1>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Header2()
        {
            var input = "##Header 2\nHeader 2\n--------";
            var expected = "<h2>Header 2</h2>\n\n<h2>Header 2</h2>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CodeBlock()
        {
            var input = "code sample:\n\n    <head>\n    <title>page title</title>\n    </head>\n";
            var expected = "<p>code sample:</p>\n\n<pre><code>&lt;head&gt;\n&lt;title&gt;page title&lt;/title&gt;\n&lt;/head&gt;\n</code></pre>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void CodeBlockBacktics()
        {
            var input = "code sample:\n\n```    <head>\n    <title>page title</title>\n    </head>\n```No more code\n";
            var expected = "<p>code sample:</p>\n\n<pre><code class=\"code-block\">    &lt;head&gt;\n    &lt;title&gt;page title&lt;/title&gt;\n    &lt;/head&gt;</code></pre>\n\n<p>No more code</p>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void CodeBlockBackticsWithEmbeddedHeadline()
        {
            var input = "```\n#test```";
            var expected = "<pre><code class=\"code-block\">#test</code></pre>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void CodeSpan()
        {
            var input = "HTML contains the `<blink>` tag";
            var expected = "<p>HTML contains the <code>&lt;blink&gt;</code> tag</p>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void HtmlPassthrough()
        {
            var input = "<div>\nHello World!\n</div>\n";
            var expected = "<div>\nHello World!\n</div>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Escaping()
        {
            var input = @"\`foo\`";
            var expected = "<p>`foo`</p>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void StrikeThroughTest()
        {
            var input = "some ~data~\n\nanother~test~ prod";
            var expected = "<p>some <del>data</del></p>\n\n<p>another<del>test</del> prod</p>\n";

            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void HorizontalRule()
        {
            var input = "* * *\n\n***\n\n*****\n\n- - -\n\n---------------------------------------\n\n";
            var expected = "<hr />\n\n<hr />\n\n<hr />\n\n<hr />\n\n<hr />\n";

            var actual = m.Transform(input);

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
        public void SimpleTableWithNoWrap2()
        {
            const string input = @"
|First Header  | Second Header|
| ============ | -------------|
|Content Cell  | Content Cell|
|Content Cell  | Content Cell|
";
            const string expected = "<table>\n<thead>\n<tr><th style=\"white-space:nowrap;\">First Header</th><th>Second Header</th></tr>\n</thead>\n" +
                                    "<tbody>\n<tr><td style=\"white-space:nowrap;\">Content Cell</td><td>Content Cell</td></tr>\n<tr><td style=\"white-space:nowrap;\">Content Cell</td><td>Content Cell</td></tr>\n</tbody>\n</table>\n";
            var actual = m.Transform(input);

            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void SimpleTableWithNoWrap3() {
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
            var expected = @"<table>
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
            var expected = @"<table>
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
