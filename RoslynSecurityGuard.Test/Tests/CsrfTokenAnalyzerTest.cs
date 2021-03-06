﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoslynSecurityGuard.Analyzers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using TestHelper;

namespace RoslynSecurityGuard.Test.Tests
{
    [TestClass]
    public class CsrfTokenAnalyzerTest : DiagnosticVerifier
    {
        protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
        {
            return new[] { new CsrfTokenAnalyzer() };
        }

        protected override IEnumerable<MetadataReference> GetAdditionnalReferences()
        {
            return new[] { MetadataReference.CreateFromFile(typeof(ValidateAntiForgeryTokenAttribute).Assembly.Location) };
        }

        [TestMethod]
        public async Task CsrfDetectMissingToken()
        {
            var cSharpTest = @"
using System.Web.Mvc;

namespace VulnerableApp
{
    public class TestController
    {
        [HttpPost]
        public ActionResult ControllerMethod(string input) {
            return null;
        }
    }
}
";
            var visualBasicTest = @"
Imports System.Web.Mvc

Namespace VulnerableApp
	Public Class TestController
		<HttpPost> _
		Public Function ControllerMethod(input As String) As ActionResult
			Return Nothing
		End Function
	End Class
End Namespace
";
            var expected = new DiagnosticResult
            {
                Id = "SG0016",
                Severity = DiagnosticSeverity.Warning
            };

            await VerifyCSharpDiagnostic(cSharpTest, expected);
            await VerifyVisualBasicDiagnostic(visualBasicTest, expected);
        }


        [TestMethod]
        public async Task CsrfValidateAntiForgeryTokenPresent()
        {
            var cSharpTest = @"
using System.Web.Mvc;

namespace VulnerableApp
{
    public class TestController
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ControllerMethod(string input) {

            return null;
        }
    }
}
                ";
            var visualBasicTest = @"
Imports System.Web.Mvc

Namespace VulnerableApp
	Public Class TestController
		<HttpPost> _
		<ValidateAntiForgeryToken> _
		Public Function ControllerMethod(input As String) As ActionResult
			Return Nothing
		End Function
	End Class
End Namespace
";

            await VerifyCSharpDiagnostic(cSharpTest);
            await VerifyVisualBasicDiagnostic(visualBasicTest);
        }

        [TestMethod]
        public async Task CsrfValidateAntiForgeryTokenPresentWithInlinedAttributes()
        {
            var cSharpTest = @"
using System.Web.Mvc;

namespace VulnerableApp
{
    public class TestController
    {
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ControllerMethod(string input) {
            return null;
        }
    }
}
                ";
            var visualBasicTest = @"
Imports System.Web.Mvc

Namespace VulnerableApp
	Public Class TestController
		<HttpPost, ValidateAntiForgeryToken> _
		Public Function ControllerMethod(input As String) As ActionResult
			Return Nothing
		End Function
	End Class
End Namespace
";
            await VerifyCSharpDiagnostic(cSharpTest);
            await VerifyVisualBasicDiagnostic(visualBasicTest);
        }
    }
}
